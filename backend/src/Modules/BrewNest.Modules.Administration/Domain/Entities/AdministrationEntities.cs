namespace BrewNest.Modules.Administration.Domain.Entities;

internal abstract class AuditedConfiguration
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

internal sealed class CompanySettings : AuditedConfiguration
{
    public string LegalName { get; set; } = string.Empty;
    public string TradingName { get; set; } = string.Empty;
    public string TaxRegistrationNumber { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "PKR";
    public string TimeZoneId { get; set; } = "Asia/Karachi";
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

internal sealed class Branch : AuditedConfiguration
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "Asia/Karachi";
    public bool IsActive { get; set; } = true;
}

internal sealed class FiscalYear : AuditedConfiguration
{
    public string Name { get; set; } = string.Empty;
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }
    public FiscalYearStatus Status { get; set; } = FiscalYearStatus.Draft;
}

internal enum FiscalYearStatus
{
    Draft,
    Open,
    Closed
}

internal sealed class DocumentNumberSequence : AuditedConfiguration
{
    public string DocumentType { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public long NextNumber { get; set; } = 1;
    public int Padding { get; set; } = 6;
    public string Suffix { get; set; } = string.Empty;
    public DocumentResetPolicy ResetPolicy { get; set; }
    public bool IsActive { get; set; } = true;
}

internal enum DocumentResetPolicy
{
    Never,
    Yearly,
    Monthly
}

internal sealed class ApprovalPolicy : AuditedConfiguration
{
    public string ProcessCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public ApprovalMode ApprovalMode { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ApprovalStep> Steps { get; set; } = [];
}

internal enum ApprovalMode
{
    All,
    Any
}

internal sealed class ApprovalStep
{
    public Guid Id { get; set; }
    public Guid ApprovalPolicyId { get; set; }
    public ApprovalPolicy ApprovalPolicy { get; set; } = null!;
    public int Sequence { get; set; }
    public string RequiredRole { get; set; } = string.Empty;
    public DateTimeOffset GrantedAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

internal sealed class AlertSetting : AuditedConfiguration
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DaysBefore { get; set; }
    public AlertSeverity Severity { get; set; }
    public bool IsActive { get; set; } = true;
}

internal enum AlertSeverity
{
    Information,
    Escalated,
    Critical
}

internal sealed class ExpenseAllocationSetting : AuditedConfiguration
{
    public string Name { get; set; } = string.Empty;
    public ExpenseAllocationMethod Method { get; set; }
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public decimal? FixedPercentage { get; set; }
    public bool IsActive { get; set; } = true;
}

internal enum ExpenseAllocationMethod
{
    Revenue,
    Headcount,
    FixedPercentage
}

internal sealed class AdministrationAuditLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public string? Reason { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
}
