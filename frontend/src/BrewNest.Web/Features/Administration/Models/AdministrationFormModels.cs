using System.ComponentModel.DataAnnotations;

namespace BrewNest.Web.Features.Administration.Models;

internal sealed class CompanySettingsFormModel
{
    [Required, StringLength(200)] public string LegalName { get; set; } = string.Empty;
    [StringLength(200)] public string TradingName { get; set; } = string.Empty;
    [StringLength(100)] public string TaxRegistrationNumber { get; set; } = string.Empty;
    [Required, StringLength(3, MinimumLength = 3)] public string CurrencyCode { get; set; } = "PKR";
    [Required] public string TimeZoneId { get; set; } = "Asia/Karachi";
    [StringLength(500)] public string Address { get; set; } = string.Empty;
    [RegularExpression(@"^\+[1-9][0-9]{7,14}$", ErrorMessage = "Use country code and 8–15 digits, e.g. +923001234567.")] public string Phone { get; set; } = string.Empty;
    [EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

internal sealed class BranchFormModel
{
    [Required, StringLength(20)] public string Code { get; set; } = string.Empty;
    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [StringLength(500)] public string Address { get; set; } = string.Empty;
    [Required] public string TimeZoneId { get; set; } = "Asia/Karachi";
}

internal sealed class FiscalYearFormModel
{
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    public DateOnly StartsOn { get; set; } = new(DateTime.Today.Year, 1, 1);
    public DateOnly EndsOn { get; set; } = new(DateTime.Today.Year, 12, 31);
}

internal sealed class DocumentNumberFormModel
{
    [Required, StringLength(80)] public string DocumentType { get; set; } = string.Empty;
    [StringLength(30)] public string Prefix { get; set; } = string.Empty;
    [Range(1, long.MaxValue)] public long NextNumber { get; set; } = 1;
    [Range(1, 12)] public int Padding { get; set; } = 6;
    [StringLength(30)] public string Suffix { get; set; } = string.Empty;
    [Required] public string ResetPolicy { get; set; } = "Never";
}

internal sealed class ExpenseAllocationFormModel
{
    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [Required] public string Method { get; set; } = "RevenueShare";
    [Range(0, 100)] public decimal? FixedPercentage { get; set; }
}

internal sealed class ApprovalPolicyFormModel
{
    [Required, StringLength(80)] public string ProcessCode { get; set; } = string.Empty;
    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal? MinimumAmount { get; set; }
    [Range(0, double.MaxValue)] public decimal? MaximumAmount { get; set; }
    [Required] public string ApprovalMode { get; set; } = "All";
    [Required] public string RequiredRoles { get; set; } = string.Empty;
}
