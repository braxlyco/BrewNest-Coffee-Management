namespace BrewNest.Contracts.Administration;

/// <summary>Company-wide configuration.</summary>
public sealed record CompanySettingsResponse(Guid Id, string LegalName, string TradingName, string TaxRegistrationNumber, string CurrencyCode, string TimeZoneId, string Address, string Phone, string Email, string Version);

/// <summary>Updates company-wide configuration.</summary>
public sealed record UpdateCompanySettingsRequest(string LegalName, string TradingName, string TaxRegistrationNumber, string CurrencyCode, string TimeZoneId, string Address, string Phone, string Email, string Version);

/// <summary>Branch configuration.</summary>
public sealed record BranchResponse(Guid Id, string Code, string Name, string Address, string TimeZoneId, bool IsActive, string Version);

/// <summary>Creates a branch.</summary>
public sealed record CreateBranchRequest(string Code, string Name, string Address, string TimeZoneId);

/// <summary>Updates a branch without deleting its history.</summary>
public sealed record UpdateBranchRequest(string Code, string Name, string Address, string TimeZoneId, bool IsActive, string Version);

/// <summary>Fiscal-year configuration.</summary>
public sealed record FiscalYearResponse(Guid Id, string Name, DateOnly StartsOn, DateOnly EndsOn, string Status, string Version);

/// <summary>Creates a fiscal year.</summary>
public sealed record CreateFiscalYearRequest(string Name, DateOnly StartsOn, DateOnly EndsOn);

/// <summary>Updates a fiscal year.</summary>
public sealed record UpdateFiscalYearRequest(string Name, DateOnly StartsOn, DateOnly EndsOn, string Status, string Version);

/// <summary>Human-readable document-number configuration.</summary>
public sealed record DocumentNumberSequenceResponse(Guid Id, string DocumentType, Guid? BranchId, string Prefix, long NextNumber, int Padding, string Suffix, string ResetPolicy, bool IsActive, string Preview, string Version);

/// <summary>Creates a document-number sequence.</summary>
public sealed record CreateDocumentNumberSequenceRequest(string DocumentType, Guid? BranchId, string Prefix, long NextNumber, int Padding, string Suffix, string ResetPolicy);

/// <summary>Updates a document-number sequence.</summary>
public sealed record UpdateDocumentNumberSequenceRequest(string Prefix, long NextNumber, int Padding, string Suffix, string ResetPolicy, bool IsActive, string Version);

/// <summary>Configurable approval route and value band.</summary>
public sealed record ApprovalPolicyResponse(Guid Id, string ProcessCode, string Name, decimal? MinimumAmount, decimal? MaximumAmount, string ApprovalMode, string[] RequiredRoles, bool IsActive, string Version);

/// <summary>Creates a configurable approval route.</summary>
public sealed record CreateApprovalPolicyRequest(string ProcessCode, string Name, decimal? MinimumAmount, decimal? MaximumAmount, string ApprovalMode, string[] RequiredRoles);

/// <summary>Updates a configurable approval route.</summary>
public sealed record UpdateApprovalPolicyRequest(string Name, decimal? MinimumAmount, decimal? MaximumAmount, string ApprovalMode, string[] RequiredRoles, bool IsActive, string Version);

/// <summary>Configurable alert threshold.</summary>
public sealed record AlertSettingResponse(Guid Id, string Code, string Name, int DaysBefore, string Severity, bool IsActive, string Version);

/// <summary>Updates an alert threshold.</summary>
public sealed record UpdateAlertSettingRequest(int DaysBefore, bool IsActive, string Version);

/// <summary>Operating-expense allocation configuration.</summary>
public sealed record ExpenseAllocationSettingResponse(Guid Id, string Name, string Method, Guid? BranchId, decimal? FixedPercentage, bool IsActive, string Version);

/// <summary>Creates an expense-allocation setting.</summary>
public sealed record CreateExpenseAllocationSettingRequest(string Name, string Method, Guid? BranchId, decimal? FixedPercentage);

/// <summary>Updates an expense-allocation setting.</summary>
public sealed record UpdateExpenseAllocationSettingRequest(string Name, string Method, Guid? BranchId, decimal? FixedPercentage, bool IsActive, string Version);

/// <summary>Append-only Administration audit entry.</summary>
public sealed record AdministrationAuditResponse(Guid Id, string EntityType, string EntityId, string Action, string Actor, DateTimeOffset OccurredAt, string? Reason, string? BeforeJson, string? AfterJson);
