<#
.SYNOPSIS
Checks or initializes BrewNest on a Windows machine with .NET 10 and SQL Server Express.
.EXAMPLE
.\scripts\Setup-BrewNestWindows.ps1
.EXAMPLE
.\scripts\Setup-BrewNestWindows.ps1 -Apply -AdministratorEmail admin@brewnest.local -DemoData
#>
[CmdletBinding()]
param(
    [string]$SqlInstance = '.\SQLEXPRESS',
    [ValidatePattern('^[A-Za-z0-9_]+$')]
    [string]$DatabaseName = 'BrewNest',
    [string]$AdministratorEmail,
    [switch]$DemoData,
    [switch]$Apply
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $projectRoot 'backend\src\BrewNest.Api'

function Get-SecretMap {
    $lines = @(dotnet user-secrets list --project $apiProject --json)
    if ($LASTEXITCODE -ne 0) { throw 'Could not read the local user-secrets inventory.' }
    $json = (@($lines | Where-Object { $_ -notmatch '^//(BEGIN|END)' }) -join "`n").Trim()
    if ([string]::IsNullOrWhiteSpace($json)) { return [pscustomobject]@{} }
    return ($json | ConvertFrom-Json)
}

function Set-Secret([string]$name, [string]$value) {
    # Pipe JSON so secret values do not appear in process command-line arguments.
    @{ $name = $value } | ConvertTo-Json -Compress | dotnet user-secrets set --project $apiProject | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Could not set local secret $name." }
}

function Get-RandomSecret([int]$byteCount) {
    $bytes = New-Object byte[] $byteCount
    $random = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $random.GetBytes($bytes) } finally { $random.Dispose() }
    return [Convert]::ToBase64String($bytes)
}

Push-Location $projectRoot
try {
    $sdk = dotnet --version
    if ($LASTEXITCODE -ne 0 -or $sdk -notmatch '^10\.') { throw '.NET 10 SDK is required.' }

    $sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
    if ($null -eq $sqlcmd) { throw 'sqlcmd is required to verify local SQL Server connectivity.' }
    $sqlProbe = & $sqlcmd.Source -S $SqlInstance -E -C -b -Q 'SELECT 1' -h -1 -W 2>&1
    if ($LASTEXITCODE -ne 0) { throw "SQL Server $SqlInstance is not reachable with Windows authentication. $($sqlProbe -join ' ')" }

    $secrets = Get-SecretMap
    $keys = @($secrets.PSObject.Properties.Name)
    $databaseProbe = & $sqlcmd.Source -S $SqlInstance -E -C -b -d master -Q "SELECT CASE WHEN DB_ID(N'$DatabaseName') IS NULL THEN 0 ELSE 1 END" -h -1 -W 2>&1
    if ($LASTEXITCODE -ne 0) { throw 'Could not check whether the database exists.' }
    $databaseExists = @($databaseProbe | Where-Object { $_.Trim() -eq '1' }).Count -gt 0
    Write-Host "SDK $sdk; SQL Server $SqlInstance is reachable."
    Write-Host "Local database $DatabaseName exists: $databaseExists; connection secret present: $($keys -contains 'ConnectionStrings:BrewNestDatabase'); JWT key present: $($keys -contains 'Jwt:SigningKey')."

    if (-not $Apply) {
        Write-Host 'Check complete. Add -Apply to set missing local secrets and apply module migrations.'
        return
    }

    if ($keys -notcontains 'ConnectionStrings:BrewNestDatabase') {
        Set-Secret 'ConnectionStrings:BrewNestDatabase' "Server=$SqlInstance;Database=$DatabaseName;Trusted_Connection=True;TrustServerCertificate=True"
    }
    else {
        $connection = [System.Data.SqlClient.SqlConnectionStringBuilder]::new($secrets.'ConnectionStrings:BrewNestDatabase')
        if ($connection.InitialCatalog -ne $DatabaseName) {
            throw 'The existing connection secret targets a different database. Check it before applying migrations.'
        }
        if ($connection.DataSource -ne $SqlInstance) {
            throw 'The existing connection secret targets a different SQL Server instance. Check it before applying migrations.'
        }
    }
    if ($keys -notcontains 'Jwt:SigningKey') {
        Set-Secret 'Jwt:SigningKey' (Get-RandomSecret 48)
    }

    if (-not [string]::IsNullOrWhiteSpace($AdministratorEmail)) {
        if ($AdministratorEmail -notmatch '^[^@\s]+@[^@\s]+\.[^@\s]+$') { throw 'AdministratorEmail must be a valid email address.' }
        Write-Host 'Development seeding creates a missing administrator; it does not reset an existing account password.'
        $securePassword = Read-Host 'Enter a strong new-development administrator password (not echoed)' -AsSecureString
        $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
        try {
            $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
            if ([string]::IsNullOrWhiteSpace($password) -or $password.Length -lt 12 -or
                $password -cnotmatch '[A-Z]' -or $password -cnotmatch '[a-z]' -or
                $password -notmatch '[0-9]' -or $password -notmatch '[^A-Za-z0-9]' -or
                @($password.ToCharArray() | Select-Object -Unique).Count -lt 4) {
                throw 'Use at least 12 characters, 4 distinct characters, and upper/lowercase letters, a digit, and a symbol.'
            }
            Set-Secret 'Identity:DevelopmentSeed:AdministratorEmail' $AdministratorEmail.Trim()
            Set-Secret 'Identity:DevelopmentSeed:AdministratorPassword' $password
            Set-Secret 'Identity:DevelopmentSeed:AdministratorFullName' 'BrewNest Administrator'
            Set-Secret 'Identity:DevelopmentSeed:Enabled' 'true'
        }
        finally {
            [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
            $password = $null
        }
    }
    if ($DemoData) { Set-Secret 'DemoData:Enabled' 'true' }

    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'Local EF tool restore failed.' }
    dotnet restore BrewNest.sln
    if ($LASTEXITCODE -ne 0) { throw 'Solution restore failed.' }
    dotnet build BrewNest.sln --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Solution build failed.' }

    $modules = @(
        @('Identity', 'Identity'), @('Administration', 'Administration'),
        @('Catalog', 'Catalog'), @('Inventory', 'Inventory'),
        @('Sales', 'Sales'), @('Kitchen', 'Kitchen'),
        @('Production', 'Production'), @('Procurement', 'Procurement'),
        @('Returns', 'Returns'), @('Payments', 'Payments')
    )
    foreach ($module in $modules) {
        $name = $module[0]
        $context = "$($module[1])DbContext"
        $moduleProject = Join-Path $projectRoot "backend\src\Modules\BrewNest.Modules.$name"
        Write-Host "Applying $name migrations..."
        dotnet tool run dotnet-ef database update --project $moduleProject --startup-project $apiProject --context $context --no-build
        if ($LASTEXITCODE -ne 0) { throw "$name migration failed; later migrations were not attempted." }
    }

    dotnet test BrewNest.sln --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
    Write-Host 'Local setup passed. Start the API and frontend with the README commands.'
}
finally {
    Pop-Location
}
