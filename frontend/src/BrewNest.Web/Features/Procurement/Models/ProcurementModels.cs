using System.ComponentModel.DataAnnotations;

namespace BrewNest.Web.Features.Procurement.Models;
internal static class ProcurementPermissionNames { public const string Read="procurement.read";public const string RequisitionsManage="procurement.requisitions.manage";public const string RequisitionsApprove="procurement.requisitions.approve";public const string SourcingManage="procurement.sourcing.manage";public const string OrdersManage="procurement.orders.manage";public const string OrdersApprove="procurement.orders.approve";public const string InspectionsManage="procurement.inspections.manage";public const string ReceiptsManage="procurement.receipts.manage";public const string ReturnsManage="procurement.returns.manage"; }
internal sealed class RequisitionForm
{
    [Required, StringLength(100)] public string Department { get; set; } = "Kitchen";
    public DateTime RequiredOn { get; set; } = DateTime.Today.AddDays(3);
    [Required, StringLength(500)] public string Reason { get; set; } = "Replenish operating stock";
    [Required] public Guid? ItemId { get; set; }
    [Range(typeof(decimal), "0.001", "999999999999999.999")] public decimal Quantity { get; set; } = 1;
    [Range(typeof(decimal), "0", "999999999999999.99")] public decimal EstimatedUnitPrice { get; set; }
}

internal sealed class RfqForm
{
    [Required] public Guid? RequisitionId { get; set; }
    public DateTime ResponseDeadline { get; set; } = DateTime.Today.AddDays(7);
    public HashSet<Guid> VendorIds { get; } = [];
}

internal sealed class QuotationForm
{
    [Required] public Guid? RfqId { get; set; }
    [Required] public Guid? VendorId { get; set; }
    [Required, StringLength(100)] public string Reference { get; set; } = string.Empty;
    public DateTime ValidUntil { get; set; } = DateTime.Today.AddDays(14);
    public List<QuotationLineForm> Lines { get; set; } = [];
}

internal sealed class QuotationLineForm
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int LeadTimeDays { get; set; }
}

internal sealed class ProcurementReceiptForm
{
    [Required] public Guid? OrderId { get; set; }
    [Required] public Guid? OrderLineId { get; set; }
    [Required] public Guid? WarehouseId { get; set; }
    [Required, StringLength(100)] public string LotNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateOnly? ManufacturingDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    [Required, StringLength(100)] public string SupplierReference { get; set; } = string.Empty;
}

internal sealed class ProcurementInspectionForm
{
    public Guid ReceiptId { get; set; }
    public Guid LineId { get; set; }
    public string Version { get; set; } = string.Empty;
    [Required, StringLength(500)] public string Remarks { get; set; } = string.Empty;
    public List<ProcurementCriterionForm> Criteria { get; set; } = [];
}

internal sealed class ProcurementCriterionForm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public bool? Passed { get; set; }
    public string Result { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string SupportingReference { get; set; } = string.Empty;
}

internal sealed class ProcurementVendorReturnForm
{
    [Required] public Guid? ReceiptId { get; set; }
    [Required] public Guid? LineId { get; set; }
    [Range(typeof(decimal), "0.001", "999999999999999.999")] public decimal Quantity { get; set; } = 1;
    [Required, StringLength(500)] public string Reason { get; set; } = string.Empty;
}
