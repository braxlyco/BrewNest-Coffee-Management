using System.Net.Mail;
using System.Security.Claims;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using BrewNest.Contracts.Administration;
using BrewNest.Modules.Administration.Domain.Entities;
using BrewNest.Modules.Administration.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Administration.Application.Services;

internal enum AdministrationResultStatus
{
    Success,
    Validation,
    NotFound,
    Conflict
}

internal sealed record AdministrationResult<T>(AdministrationResultStatus Status, T? Value, string[] Errors)
{
    public bool Succeeded => Status == AdministrationResultStatus.Success;
    public static AdministrationResult<T> Success(T value) => new(AdministrationResultStatus.Success, value, []);
    public static AdministrationResult<T> Failure(AdministrationResultStatus status, params string[] errors) => new(status, default, errors);
}

internal sealed record AdministrationRequestContext(string Actor)
{
    public static AdministrationRequestContext FromHttpContext(HttpContext context) => new(
        context.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? context.User.Identity?.Name
        ?? "anonymous");
}

internal sealed class AdministrationService(AdministrationDbContext dbContext, TimeProvider timeProvider)
{
    private static readonly JsonSerializerOptions AuditJsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AdministrationResult<CompanySettingsResponse>> GetCompanyAsync(CancellationToken cancellationToken)
    {
        var entity = await dbContext.CompanySettings.AsNoTracking().SingleAsync(cancellationToken);
        return AdministrationResult<CompanySettingsResponse>.Success(Map(entity));
    }

    public async Task<AdministrationResult<CompanySettingsResponse>> UpdateCompanyAsync(UpdateCompanySettingsRequest request, AdministrationRequestContext context, CancellationToken cancellationToken)
    {
        var errors = ValidateCompany(request);
        if (errors.Length > 0) return AdministrationResult<CompanySettingsResponse>.Failure(AdministrationResultStatus.Validation, errors);
        var entity = await dbContext.CompanySettings.SingleAsync(cancellationToken);
        if (!TryApplyVersion(entity, request.Version)) return VersionFailure<CompanySettingsResponse>();
        var before = Map(entity);
        entity.LegalName = request.LegalName.Trim();
        entity.TradingName = request.TradingName.Trim();
        entity.TaxRegistrationNumber = request.TaxRegistrationNumber.Trim();
        entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
        entity.TimeZoneId = request.TimeZoneId.Trim();
        entity.Address = request.Address.Trim();
        entity.Phone = request.Phone.Trim();
        entity.Email = request.Email.Trim();
        Touch(entity, context);
        AddAudit("CompanySettings", entity.Id, "Updated", context, before, Map(entity));
        return await SaveAsync(() => Map(entity), cancellationToken);
    }

    public async Task<AdministrationResult<BranchResponse[]>> GetBranchesAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.Branches.AsNoTracking().OrderBy(entity => entity.Code).ToArrayAsync(cancellationToken);
        return AdministrationResult<BranchResponse[]>.Success(entities.Select(Map).ToArray());
    }

    public async Task<AdministrationResult<BranchResponse>> CreateBranchAsync(CreateBranchRequest request, AdministrationRequestContext context, CancellationToken cancellationToken)
    {
        var errors = ValidateBranch(request.Code, request.Name, request.TimeZoneId);
        if (errors.Length > 0) return AdministrationResult<BranchResponse>.Failure(AdministrationResultStatus.Validation, errors);
        var code = request.Code.Trim().ToUpperInvariant();
        if (await dbContext.Branches.AnyAsync(entity => entity.Code == code, cancellationToken)) return Conflict<BranchResponse>("A branch with this code already exists.");
        var entity = new Branch { Id = Guid.NewGuid(), Code = code, Name = request.Name.Trim(), Address = request.Address.Trim(), TimeZoneId = request.TimeZoneId.Trim(), CreatedAt = Now, CreatedBy = context.Actor };
        dbContext.Branches.Add(entity);
        AddAudit("Branch", entity.Id, "Created", context, null, Map(entity));
        return await SaveAsync(() => Map(entity), cancellationToken);
    }

    public async Task<AdministrationResult<BranchResponse>> UpdateBranchAsync(Guid id, UpdateBranchRequest request, AdministrationRequestContext context, CancellationToken cancellationToken)
    {
        var errors = ValidateBranch(request.Code, request.Name, request.TimeZoneId);
        if (errors.Length > 0) return AdministrationResult<BranchResponse>.Failure(AdministrationResultStatus.Validation, errors);
        var entity = await dbContext.Branches.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (entity is null) return NotFound<BranchResponse>("The branch was not found.");
        if (!TryApplyVersion(entity, request.Version)) return VersionFailure<BranchResponse>();
        var code = request.Code.Trim().ToUpperInvariant();
        if (await dbContext.Branches.AnyAsync(candidate => candidate.Code == code && candidate.Id != id, cancellationToken)) return Conflict<BranchResponse>("A branch with this code already exists.");
        var before = Map(entity);
        entity.Code = code; entity.Name = request.Name.Trim(); entity.Address = request.Address.Trim(); entity.TimeZoneId = request.TimeZoneId.Trim(); entity.IsActive = request.IsActive; Touch(entity, context);
        AddAudit("Branch", entity.Id, "Updated", context, before, Map(entity));
        return await SaveAsync(() => Map(entity), cancellationToken);
    }

    public async Task<AdministrationResult<FiscalYearResponse[]>> GetFiscalYearsAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.FiscalYears.AsNoTracking().OrderByDescending(entity => entity.StartsOn).ToArrayAsync(cancellationToken);
        return AdministrationResult<FiscalYearResponse[]>.Success(entities.Select(Map).ToArray());
    }

    public async Task<AdministrationResult<FiscalYearResponse>> CreateFiscalYearAsync(CreateFiscalYearRequest request, AdministrationRequestContext context, CancellationToken cancellationToken)
    {
        var errors = ValidateFiscalYear(request.Name, request.StartsOn, request.EndsOn);
        if (errors.Length > 0) return AdministrationResult<FiscalYearResponse>.Failure(AdministrationResultStatus.Validation, errors);
        if (await HasFiscalOverlapAsync(null, request.StartsOn, request.EndsOn, cancellationToken)) return Conflict<FiscalYearResponse>("The fiscal year overlaps an existing fiscal year.");
        var entity = new FiscalYear { Id = Guid.NewGuid(), Name = request.Name.Trim(), StartsOn = request.StartsOn, EndsOn = request.EndsOn, CreatedAt = Now, CreatedBy = context.Actor };
        dbContext.FiscalYears.Add(entity); AddAudit("FiscalYear", entity.Id, "Created", context, null, Map(entity));
        return await SaveAsync(() => Map(entity), cancellationToken);
    }

    public async Task<AdministrationResult<FiscalYearResponse>> UpdateFiscalYearAsync(Guid id, UpdateFiscalYearRequest request, AdministrationRequestContext context, CancellationToken cancellationToken)
    {
        var errors = ValidateFiscalYear(request.Name, request.StartsOn, request.EndsOn);
        if (!Enum.TryParse<FiscalYearStatus>(request.Status, true, out var status)) errors = errors.Append("A valid fiscal-year status is required.").ToArray();
        if (errors.Length > 0) return AdministrationResult<FiscalYearResponse>.Failure(AdministrationResultStatus.Validation, errors);
        var entity = await dbContext.FiscalYears.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (entity is null) return NotFound<FiscalYearResponse>("The fiscal year was not found.");
        if (!TryApplyVersion(entity, request.Version)) return VersionFailure<FiscalYearResponse>();
        if (await HasFiscalOverlapAsync(id, request.StartsOn, request.EndsOn, cancellationToken)) return Conflict<FiscalYearResponse>("The fiscal year overlaps an existing fiscal year.");
        var before = Map(entity); entity.Name = request.Name.Trim(); entity.StartsOn = request.StartsOn; entity.EndsOn = request.EndsOn; entity.Status = status; Touch(entity, context);
        AddAudit("FiscalYear", entity.Id, "Updated", context, before, Map(entity));
        return await SaveAsync(() => Map(entity), cancellationToken);
    }

    public async Task<AdministrationResult<DocumentNumberSequenceResponse[]>> GetDocumentNumbersAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.DocumentNumberSequences.AsNoTracking().OrderBy(entity => entity.DocumentType).ToArrayAsync(cancellationToken);
        return AdministrationResult<DocumentNumberSequenceResponse[]>.Success(entities.Select(Map).ToArray());
    }

    public async Task<AdministrationResult<DocumentNumberSequenceResponse>> CreateDocumentNumberAsync(CreateDocumentNumberSequenceRequest request, AdministrationRequestContext context, CancellationToken cancellationToken)
    {
        var validation = ValidateDocumentNumber(request.DocumentType, request.NextNumber, request.Padding, request.ResetPolicy, out var resetPolicy);
        if (validation.Length > 0) return AdministrationResult<DocumentNumberSequenceResponse>.Failure(AdministrationResultStatus.Validation, validation);
        var documentType = request.DocumentType.Trim();
        if (await dbContext.DocumentNumberSequences.AnyAsync(entity => entity.DocumentType == documentType && entity.BranchId == request.BranchId, cancellationToken)) return Conflict<DocumentNumberSequenceResponse>("This document type and branch already have a sequence.");
        if (request.BranchId is not null && !await dbContext.Branches.AnyAsync(entity => entity.Id == request.BranchId, cancellationToken)) return AdministrationResult<DocumentNumberSequenceResponse>.Failure(AdministrationResultStatus.Validation, "The selected branch is invalid.");
        var entity = new DocumentNumberSequence { Id = Guid.NewGuid(), DocumentType = documentType, BranchId = request.BranchId, Prefix = request.Prefix.Trim(), NextNumber = request.NextNumber, Padding = request.Padding, Suffix = request.Suffix.Trim(), ResetPolicy = resetPolicy, CreatedAt = Now, CreatedBy = context.Actor };
        dbContext.DocumentNumberSequences.Add(entity); AddAudit("DocumentNumberSequence", entity.Id, "Created", context, null, Map(entity));
        return await SaveAsync(() => Map(entity), cancellationToken);
    }

    public async Task<AdministrationResult<DocumentNumberSequenceResponse>> UpdateDocumentNumberAsync(Guid id, UpdateDocumentNumberSequenceRequest request, AdministrationRequestContext context, CancellationToken cancellationToken)
    {
        var validation = ValidateDocumentNumber("existing", request.NextNumber, request.Padding, request.ResetPolicy, out var resetPolicy);
        if (validation.Length > 0) return AdministrationResult<DocumentNumberSequenceResponse>.Failure(AdministrationResultStatus.Validation, validation);
        var entity = await dbContext.DocumentNumberSequences.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (entity is null) return NotFound<DocumentNumberSequenceResponse>("The document sequence was not found.");
        if (!TryApplyVersion(entity, request.Version)) return VersionFailure<DocumentNumberSequenceResponse>();
        var before = Map(entity); entity.Prefix = request.Prefix.Trim(); entity.NextNumber = request.NextNumber; entity.Padding = request.Padding; entity.Suffix = request.Suffix.Trim(); entity.ResetPolicy = resetPolicy; entity.IsActive = request.IsActive; Touch(entity, context);
        AddAudit("DocumentNumberSequence", entity.Id, "Updated", context, before, Map(entity));
        return await SaveAsync(() => Map(entity), cancellationToken);
    }

    public async Task<AdministrationResult<ApprovalPolicyResponse[]>> GetApprovalsAsync(CancellationToken cancellationToken)
    {
        var policies = await dbContext.ApprovalPolicies.AsNoTracking().Include(entity => entity.Steps).OrderBy(entity => entity.ProcessCode).ThenBy(entity => entity.MinimumAmount).ToArrayAsync(cancellationToken);
        return AdministrationResult<ApprovalPolicyResponse[]>.Success(policies.Select(Map).ToArray());
    }

    public async Task<AdministrationResult<ApprovalPolicyResponse>> CreateApprovalAsync(CreateApprovalPolicyRequest request, AdministrationRequestContext context, CancellationToken cancellationToken)
    {
        var errors = ValidateApproval(request.ProcessCode, request.Name, request.MinimumAmount, request.MaximumAmount, request.ApprovalMode, request.RequiredRoles, out var mode);
        if (errors.Length > 0) return AdministrationResult<ApprovalPolicyResponse>.Failure(AdministrationResultStatus.Validation, errors);
        if (await HasApprovalOverlapAsync(null, request.ProcessCode.Trim(), request.MinimumAmount, request.MaximumAmount, cancellationToken)) return Conflict<ApprovalPolicyResponse>("This approval value range overlaps an active policy.");
        var entity = new ApprovalPolicy { Id = Guid.NewGuid(), ProcessCode = request.ProcessCode.Trim(), Name = request.Name.Trim(), MinimumAmount = request.MinimumAmount, MaximumAmount = request.MaximumAmount, ApprovalMode = mode, CreatedAt = Now, CreatedBy = context.Actor };
        AddSteps(entity, request.RequiredRoles, context); dbContext.ApprovalPolicies.Add(entity); AddAudit("ApprovalPolicy", entity.Id, "Created", context, null, Map(entity));
        return await SaveAsync(() => Map(entity), cancellationToken);
    }

    public async Task<AdministrationResult<ApprovalPolicyResponse>> UpdateApprovalAsync(Guid id, UpdateApprovalPolicyRequest request, AdministrationRequestContext context, CancellationToken cancellationToken)
    {
        var errors = ValidateApproval("existing", request.Name, request.MinimumAmount, request.MaximumAmount, request.ApprovalMode, request.RequiredRoles, out var mode);
        if (errors.Length > 0) return AdministrationResult<ApprovalPolicyResponse>.Failure(AdministrationResultStatus.Validation, errors);
        var entity = await dbContext.ApprovalPolicies.Include(policy => policy.Steps).SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (entity is null) return NotFound<ApprovalPolicyResponse>("The approval policy was not found.");
        if (!TryApplyVersion(entity, request.Version)) return VersionFailure<ApprovalPolicyResponse>();
        if (request.IsActive && await HasApprovalOverlapAsync(id, entity.ProcessCode, request.MinimumAmount, request.MaximumAmount, cancellationToken)) return Conflict<ApprovalPolicyResponse>("This approval value range overlaps an active policy.");
        var before = Map(entity); var now = Now;
        foreach (var step in entity.Steps.Where(step => step.RevokedAt is null)) { step.RevokedAt = now; step.RevokedBy = context.Actor; }
        entity.Name = request.Name.Trim(); entity.MinimumAmount = request.MinimumAmount; entity.MaximumAmount = request.MaximumAmount; entity.ApprovalMode = mode; entity.IsActive = request.IsActive; Touch(entity, context); AddSteps(entity, request.RequiredRoles, context);
        AddAudit("ApprovalPolicy", entity.Id, "Updated", context, before, Map(entity));
        return await SaveAsync(() => Map(entity), cancellationToken);
    }

    public async Task<AdministrationResult<AlertSettingResponse[]>> GetAlertsAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.AlertSettings.AsNoTracking().OrderByDescending(entity => entity.DaysBefore).ToArrayAsync(cancellationToken);
        return AdministrationResult<AlertSettingResponse[]>.Success(entities.Select(Map).ToArray());
    }

    public async Task<AdministrationResult<AlertSettingResponse>> UpdateAlertAsync(Guid id, UpdateAlertSettingRequest request, AdministrationRequestContext context, CancellationToken cancellationToken)
    {
        if (request.DaysBefore < 1 || request.DaysBefore > 365) return AdministrationResult<AlertSettingResponse>.Failure(AdministrationResultStatus.Validation, "Alert days must be between 1 and 365.");
        var entity = await dbContext.AlertSettings.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (entity is null) return NotFound<AlertSettingResponse>("The alert setting was not found.");
        if (!TryApplyVersion(entity, request.Version)) return VersionFailure<AlertSettingResponse>();
        var all = await dbContext.AlertSettings.ToArrayAsync(cancellationToken); var proposed = all.ToDictionary(setting => setting.Code, setting => setting.Id == id ? request.DaysBefore : setting.DaysBefore, StringComparer.Ordinal);
        if (proposed.GetValueOrDefault("ExpiryFirst") <= proposed.GetValueOrDefault("ExpiryEscalated") || proposed.GetValueOrDefault("ExpiryEscalated") <= proposed.GetValueOrDefault("ExpiryCritical")) return AdministrationResult<AlertSettingResponse>.Failure(AdministrationResultStatus.Validation, "Expiry alert days must remain First > Escalated > Critical.");
        var before = Map(entity); entity.DaysBefore = request.DaysBefore; entity.IsActive = request.IsActive; Touch(entity, context); AddAudit("AlertSetting", entity.Id, "Updated", context, before, Map(entity));
        return await SaveAsync(() => Map(entity), cancellationToken);
    }

    public async Task<AdministrationResult<ExpenseAllocationSettingResponse[]>> GetExpenseAllocationsAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.ExpenseAllocationSettings.AsNoTracking().OrderBy(entity => entity.Name).ToArrayAsync(cancellationToken);
        return AdministrationResult<ExpenseAllocationSettingResponse[]>.Success(entities.Select(Map).ToArray());
    }

    public async Task<AdministrationResult<ExpenseAllocationSettingResponse>> CreateExpenseAllocationAsync(CreateExpenseAllocationSettingRequest request, AdministrationRequestContext context, CancellationToken cancellationToken)
    {
        var errors = ValidateExpense(request.Name, request.Method, request.FixedPercentage, out var method);
        if (errors.Length > 0) return AdministrationResult<ExpenseAllocationSettingResponse>.Failure(AdministrationResultStatus.Validation, errors);
        if (request.BranchId is not null && !await dbContext.Branches.AnyAsync(entity => entity.Id == request.BranchId, cancellationToken)) return AdministrationResult<ExpenseAllocationSettingResponse>.Failure(AdministrationResultStatus.Validation, "The selected branch is invalid.");
        var entity = new ExpenseAllocationSetting { Id = Guid.NewGuid(), Name = request.Name.Trim(), Method = method, BranchId = request.BranchId, FixedPercentage = request.FixedPercentage, CreatedAt = Now, CreatedBy = context.Actor };
        dbContext.ExpenseAllocationSettings.Add(entity); AddAudit("ExpenseAllocationSetting", entity.Id, "Created", context, null, Map(entity));
        return await SaveAsync(() => Map(entity), cancellationToken);
    }

    public async Task<AdministrationResult<ExpenseAllocationSettingResponse>> UpdateExpenseAllocationAsync(Guid id, UpdateExpenseAllocationSettingRequest request, AdministrationRequestContext context, CancellationToken cancellationToken)
    {
        var errors = ValidateExpense(request.Name, request.Method, request.FixedPercentage, out var method);
        if (errors.Length > 0) return AdministrationResult<ExpenseAllocationSettingResponse>.Failure(AdministrationResultStatus.Validation, errors);
        var entity = await dbContext.ExpenseAllocationSettings.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (entity is null) return NotFound<ExpenseAllocationSettingResponse>("The expense-allocation setting was not found.");
        if (!TryApplyVersion(entity, request.Version)) return VersionFailure<ExpenseAllocationSettingResponse>();
        if (request.BranchId is not null && !await dbContext.Branches.AnyAsync(branch => branch.Id == request.BranchId, cancellationToken)) return AdministrationResult<ExpenseAllocationSettingResponse>.Failure(AdministrationResultStatus.Validation, "The selected branch is invalid.");
        var before = Map(entity); entity.Name = request.Name.Trim(); entity.Method = method; entity.BranchId = request.BranchId; entity.FixedPercentage = request.FixedPercentage; entity.IsActive = request.IsActive; Touch(entity, context); AddAudit("ExpenseAllocationSetting", entity.Id, "Updated", context, before, Map(entity));
        return await SaveAsync(() => Map(entity), cancellationToken);
    }

    public async Task<AdministrationResult<AdministrationAuditResponse[]>> GetAuditAsync(int take, CancellationToken cancellationToken)
    {
        take = Math.Clamp(take, 1, 500);
        return AdministrationResult<AdministrationAuditResponse[]>.Success(await dbContext.AuditLogs.AsNoTracking().OrderByDescending(entity => entity.OccurredAt).Take(take).Select(entity => new AdministrationAuditResponse(entity.Id, entity.EntityType, entity.EntityId, entity.Action, entity.Actor, entity.OccurredAt, entity.Reason, entity.BeforeJson, entity.AfterJson)).ToArrayAsync(cancellationToken));
    }

    private DateTimeOffset Now => timeProvider.GetUtcNow();
    private void Touch(AuditedConfiguration entity, AdministrationRequestContext context) { entity.ModifiedAt = Now; entity.ModifiedBy = context.Actor; }
    private void AddAudit(string type, Guid id, string action, AdministrationRequestContext context, object? before, object? after) => dbContext.AuditLogs.Add(new AdministrationAuditLog { Id = Guid.NewGuid(), EntityType = type, EntityId = id.ToString(), Action = action, Actor = context.Actor, OccurredAt = Now, BeforeJson = before is null ? null : JsonSerializer.Serialize(before, AuditJsonOptions), AfterJson = after is null ? null : JsonSerializer.Serialize(after, AuditJsonOptions) });
    private async Task<AdministrationResult<T>> SaveAsync<T>(Func<T> valueFactory, CancellationToken cancellationToken) { try { await dbContext.SaveChangesAsync(cancellationToken); return AdministrationResult<T>.Success(valueFactory()); } catch (DbUpdateConcurrencyException) { return VersionFailure<T>(); } }
    private bool TryApplyVersion(AuditedConfiguration entity, string version) { try { var bytes = Convert.FromBase64String(version); var property = dbContext.Entry(entity).Property(candidate => candidate.RowVersion); if (bytes.Length == 0 && property.CurrentValue.Length == 0) return true; property.OriginalValue = bytes; return bytes.Length > 0; } catch (FormatException) { return false; } }
    private void AddSteps(ApprovalPolicy policy, IEnumerable<string> roles, AdministrationRequestContext context) { var sequence = 1; foreach (var role in roles.Select(value => value.Trim()).Where(value => value.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase)) policy.Steps.Add(new ApprovalStep { Id = Guid.NewGuid(), Sequence = sequence++, RequiredRole = role, GrantedAt = Now, GrantedBy = context.Actor }); }
    private Task<bool> HasFiscalOverlapAsync(Guid? id, DateOnly start, DateOnly end, CancellationToken token) => dbContext.FiscalYears.AnyAsync(entity => entity.Id != id && start <= entity.EndsOn && end >= entity.StartsOn, token);
    private Task<bool> HasApprovalOverlapAsync(Guid? id, string process, decimal? min, decimal? max, CancellationToken token) { var low = min ?? decimal.MinValue; var high = max ?? decimal.MaxValue; return dbContext.ApprovalPolicies.AnyAsync(entity => entity.Id != id && entity.IsActive && entity.ProcessCode == process && low <= (entity.MaximumAmount ?? decimal.MaxValue) && high >= (entity.MinimumAmount ?? decimal.MinValue), token); }
    private static AdministrationResult<T> NotFound<T>(string error) => AdministrationResult<T>.Failure(AdministrationResultStatus.NotFound, error);
    private static AdministrationResult<T> Conflict<T>(string error) => AdministrationResult<T>.Failure(AdministrationResultStatus.Conflict, error);
    private static AdministrationResult<T> VersionFailure<T>() => Conflict<T>("The configuration changed. Reload it and try again.");
    private static string[] ValidateCompany(UpdateCompanySettingsRequest request) { var errors = new List<string>(); if (string.IsNullOrWhiteSpace(request.LegalName) || request.LegalName.Length > 200) errors.Add("A legal name of 200 characters or fewer is required."); if (request.CurrencyCode.Trim().Length != 3) errors.Add("A three-letter currency code is required."); if (string.IsNullOrWhiteSpace(request.TimeZoneId)) errors.Add("A time zone is required."); if (request.Email.Length > 0 && !IsEmail(request.Email)) errors.Add("The company email is invalid."); if (!string.IsNullOrWhiteSpace(request.Phone) && !Regex.IsMatch(request.Phone.Trim(), @"^\+[1-9][0-9]{7,14}$", RegexOptions.CultureInvariant)) errors.Add("Company phone must include a country code (+ and 8 to 15 digits)."); return errors.ToArray(); }
    private static string[] ValidateBranch(string code, string name, string timeZone) { var errors = new List<string>(); if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 20 || code.Any(character => !char.IsLetterOrDigit(character) && character != '-')) errors.Add("A branch code up to 20 letters, digits, or hyphens is required."); if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200) errors.Add("A branch name is required."); if (string.IsNullOrWhiteSpace(timeZone)) errors.Add("A time zone is required."); return errors.ToArray(); }
    private static string[] ValidateFiscalYear(string name, DateOnly start, DateOnly end) { var errors = new List<string>(); if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100) errors.Add("A fiscal-year name is required."); if (end < start) errors.Add("The fiscal-year end date must not precede its start date."); return errors.ToArray(); }
    private static string[] ValidateDocumentNumber(string type, long next, int padding, string reset, out DocumentResetPolicy policy) { var errors = new List<string>(); if (string.IsNullOrWhiteSpace(type) || type.Trim().Length > 80) errors.Add("A document type is required."); if (next < 1) errors.Add("The next number must be positive."); if (padding is < 1 or > 12) errors.Add("Padding must be between 1 and 12."); if (!Enum.TryParse(reset, true, out policy)) errors.Add("A valid reset policy is required."); return errors.ToArray(); }
    private static string[] ValidateApproval(string process, string name, decimal? minimum, decimal? maximum, string approvalMode, string[] roles, out ApprovalMode mode) { var errors = new List<string>(); if (string.IsNullOrWhiteSpace(process) || process.Trim().Length > 80) errors.Add("A process code is required."); if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200) errors.Add("A policy name is required."); if (minimum < 0 || maximum < 0 || minimum > maximum) errors.Add("The approval amount range is invalid."); if (!Enum.TryParse(approvalMode, true, out mode)) errors.Add("Approval mode must be All or Any."); if (roles.Length == 0 || roles.Any(string.IsNullOrWhiteSpace)) errors.Add("At least one required role is required."); return errors.ToArray(); }
    private static string[] ValidateExpense(string name, string methodText, decimal? fixedPercentage, out ExpenseAllocationMethod method) { var errors = new List<string>(); if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200) errors.Add("A setting name is required."); if (!Enum.TryParse(methodText, true, out method)) errors.Add("A valid allocation method is required."); if (method == ExpenseAllocationMethod.FixedPercentage && (fixedPercentage is null or < 0 or > 100)) errors.Add("Fixed-percentage allocation requires a percentage from 0 to 100."); if (method != ExpenseAllocationMethod.FixedPercentage && fixedPercentage is not null) errors.Add("A fixed percentage applies only to the FixedPercentage method."); return errors.ToArray(); }
    private static bool IsEmail(string email) { try { return new MailAddress(email).Address.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase); } catch (FormatException) { return false; } }
    private static CompanySettingsResponse Map(CompanySettings entity) => new(entity.Id, entity.LegalName, entity.TradingName, entity.TaxRegistrationNumber, entity.CurrencyCode, entity.TimeZoneId, entity.Address, entity.Phone, entity.Email, Version(entity));
    private static BranchResponse Map(Branch entity) => new(entity.Id, entity.Code, entity.Name, entity.Address, entity.TimeZoneId, entity.IsActive, Version(entity));
    private static FiscalYearResponse Map(FiscalYear entity) => new(entity.Id, entity.Name, entity.StartsOn, entity.EndsOn, entity.Status.ToString(), Version(entity));
    private static DocumentNumberSequenceResponse Map(DocumentNumberSequence entity) => new(entity.Id, entity.DocumentType, entity.BranchId, entity.Prefix, entity.NextNumber, entity.Padding, entity.Suffix, entity.ResetPolicy.ToString(), entity.IsActive, $"{entity.Prefix}{entity.NextNumber.ToString(CultureInfo.InvariantCulture).PadLeft(entity.Padding, '0')}{entity.Suffix}", Version(entity));
    private static ApprovalPolicyResponse Map(ApprovalPolicy entity) => new(entity.Id, entity.ProcessCode, entity.Name, entity.MinimumAmount, entity.MaximumAmount, entity.ApprovalMode.ToString(), entity.Steps.Where(step => step.RevokedAt is null).OrderBy(step => step.Sequence).Select(step => step.RequiredRole).ToArray(), entity.IsActive, Version(entity));
    private static AlertSettingResponse Map(AlertSetting entity) => new(entity.Id, entity.Code, entity.Name, entity.DaysBefore, entity.Severity.ToString(), entity.IsActive, Version(entity));
    private static ExpenseAllocationSettingResponse Map(ExpenseAllocationSetting entity) => new(entity.Id, entity.Name, entity.Method.ToString(), entity.BranchId, entity.FixedPercentage, entity.IsActive, Version(entity));
    private static string Version(AuditedConfiguration entity) => Convert.ToBase64String(entity.RowVersion);
}
