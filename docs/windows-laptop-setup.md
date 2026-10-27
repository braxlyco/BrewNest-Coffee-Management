# Windows setup on another computer

The source code moves through Git, but SQL Server databases, ASP.NET user-secrets, local HTTPS certificates, and browser sessions do not. Complete these steps **on the laptop** after pulling the repository. The original PC's `BrewNest` database is not changed by these steps.

## Prerequisites

Install the .NET 10 SDK, Git, SQL Server Express (with Database Engine running), and `sqlcmd`. Use Windows authentication for the local SQL Express instance. In a PowerShell terminal at the repository root, run:

```powershell
dotnet --info
Get-Service 'MSSQL$SQLEXPRESS'
sqlcmd -S '.\SQLEXPRESS' -E -C -Q 'SELECT 1'
powershell.exe -NoProfile -ExecutionPolicy RemoteSigned -File .\scripts\Setup-BrewNestWindows.ps1
```

The setup script's default mode is read-only. It checks the SDK, SQL connectivity, database presence, and whether the required local secrets exist, without printing secret values. If SQL Express uses another instance name, pass `-SqlInstance 'SERVER\INSTANCE'` in both setup commands.

## Fresh development database

For a new database containing only the optional Catalog/Inventory sample records:

```powershell
powershell.exe -NoProfile -ExecutionPolicy RemoteSigned -File .\scripts\Setup-BrewNestWindows.ps1 -Apply -AdministratorEmail admin@brewnest.local -DemoData
```

The script prompts privately for a strong development administrator password, stores it with .NET user-secrets, generates a local JWT signing key, restores/builds/tests the solution, and applies each module's EF migration to the one `BrewNest` SQL Server database. Existing secrets are preserved. The password prompt creates a missing development user; it does **not** reset an existing user's password. Do not run with `-AdministratorEmail` if you restored an existing database and intend to keep its login. If the laptop already has a `BrewNest` database, the script applies pending migrations without deleting it.

For a different database name, pass `-DatabaseName BrewNestDev` when running both the read-only check and `-Apply`. If a connection secret already points to another database, the script stops before migrations; inspect the local secret and make a deliberate choice.

## Copy the original PC database instead

If you need the existing transactions and users on the laptop, make a SQL Server backup on the original PC, transfer the `.bak` file privately, and restore it into the laptop's SQL Server Express instance before running `-Apply` **without** `-DemoData` or `-AdministratorEmail`. Confirm the source and target database names and make a laptop backup before replacing any existing laptop database. A backup can contain personal, transaction, and password-hash data: never put it in Git or a shared folder. Existing user passwords remain unchanged; use the original account credentials. Local user-secrets still need to be set on the laptop, including its own JWT key and connection string.

## Start and verify

Trust a local HTTPS development certificate through the normal Windows prompt:

```powershell
dotnet dev-certs https --trust
dotnet run --project backend/src/BrewNest.Api --launch-profile https
```

In a second terminal:

```powershell
dotnet run --project frontend/src/BrewNest.Web --launch-profile https
```

Open `https://localhost:7240/`. The API health endpoint is `https://localhost:7241/health`. The frontend's API URL is in `frontend/src/BrewNest.Web/wwwroot/appsettings.json` and matches these default ports. If either port is occupied, choose new ports and update the frontend API URL and the API's allowed CORS origin together.

Never copy `secrets.json`, database backups, SQL credentials, or live payment keys into the repository. The current wallet gateways are local simulators. Delivery, Finance, and Reporting remain planned modules.
