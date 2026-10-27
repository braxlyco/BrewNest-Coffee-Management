using System.ComponentModel.DataAnnotations;

namespace BrewNest.Web.Features.Returns.Models;

internal static class ReturnsPermissionNames
{
    public const string Read = "returns.read";
    public const string Manage = "returns.manage";
    public const string Approve = "returns.approve";
    public const string Inspect = "returns.inspect";
    public const string Voids = "returns.voids.manage";
    public const string VoidsApprove = "returns.voids.approve";
    public const string Refunds = "returns.refunds.manage";
}

internal sealed class ReturnForm : IValidatableObject
{
    [Required, StringLength(40)]
    public string BillNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select an item from the original bill."), RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "Select an item from the original bill.")]
    public string SalesLineId { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.001", "999999999999999.999")]
    public decimal Quantity { get; set; } = 1;

    [Required, StringLength(500, MinimumLength = 3)]
    public string Reason { get; set; } = "Customer return";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Quantity != decimal.Round(Quantity, 3))
            yield return new ValidationResult("Quantity can have at most three decimal places.", [nameof(Quantity)]);
    }
}

internal sealed class VoidForm
{
    [Required(ErrorMessage = "Select the order to void."), RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "Select the order to void.")]
    public string SalesOrderId { get; set; } = string.Empty;

    [Required, StringLength(500, MinimumLength = 3)]
    public string Reason { get; set; } = "Transaction void requested";
}

internal sealed class ReturnInspectionForm : IValidatableObject
{
    public bool IsSealed { get; set; }
    public bool IsPackagingIntact { get; set; }
    public bool ManufacturingDateChecked { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    [StringLength(100)]
    public string LotNumber { get; set; } = string.Empty;

    [RegularExpression("^$|^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "Select a valid warehouse for restocking.")]
    public string WarehouseId { get; set; } = string.Empty;
    public bool BatchVerified { get; set; }
    public bool QualityPassed { get; set; }
    public bool RestockRequested { get; set; }

    [Required, StringLength(200)]
    public string Criterion { get; set; } = "Returned food condition";

    [Required, StringLength(500)]
    public string Result { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Remarks { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string SupportingReference { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!RestockRequested) yield break;
        if (!IsSealed || !IsPackagingIntact || !ManufacturingDateChecked || !BatchVerified || !QualityPassed)
            yield return new ValidationResult("All safety and quality checks must pass before restocking.");
        if (ManufacturingDate is null || ManufacturingDate > DateTime.UtcNow.Date ||
            ExpiryDate is null || ExpiryDate <= DateTime.UtcNow.Date ||
            ManufacturingDate > ExpiryDate)
            yield return new ValidationResult("Enter checked manufacturing and future expiry dates.", [nameof(ManufacturingDate), nameof(ExpiryDate)]);
        if (string.IsNullOrWhiteSpace(LotNumber))
            yield return new ValidationResult("A verified lot is required for restocking.", [nameof(LotNumber)]);
        if (!Guid.TryParse(WarehouseId, out var warehouseId) || warehouseId == Guid.Empty)
            yield return new ValidationResult("Select an active warehouse for restocking.", [nameof(WarehouseId)]);
    }
}
