using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BrewNest.Contracts.Payments;

namespace BrewNest.Web.Features.Payments.Models;

internal static class PaymentsPermissionNames
{
    public const string Read = "payments.read";
    public const string Create = "payments.create";
    public const string Verify = "payments.verify";
    public const string Settle = "payments.settle";
    public const string RefundsRead = "payments.refunds.read";
    public const string RefundsProcess = "payments.refunds.process";
}

internal sealed class PaymentForm : IValidatableObject
{
    [Required, RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "Select a billed order.")]
    public string SalesOrderId { get; set; } = string.Empty;
    [Required, StringLength(40)] public string BillNumber { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.01", "999999999999999.99")] public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    [StringLength(150)] public string PayerReference { get; set; } = string.Empty;
    [StringLength(500)] public string Notes { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Amount != decimal.Round(Amount, 2))
            yield return new ValidationResult("Amount can have at most two decimal places.", [nameof(Amount)]);
        if (Method is not PaymentMethod.Cash && string.IsNullOrWhiteSpace(PayerReference))
            yield return new ValidationResult("A payer or transaction reference is required for this method.", [nameof(PayerReference)]);
        if (Method is PaymentMethod.Easypaisa or PaymentMethod.JazzCash &&
            !Regex.IsMatch(PayerReference, @"^\+[1-9]\d{7,14}$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)))
            yield return new ValidationResult("Wallet phone must include a country code and 8–15 digits, for example +923001234567.", [nameof(PayerReference)]);
    }
}

internal sealed class SettlementForm
{
    [Required, StringLength(150)] public string Reference { get; set; } = string.Empty;
    [Required, StringLength(500)] public string Remarks { get; set; } = "Reconciled and settled";
}
