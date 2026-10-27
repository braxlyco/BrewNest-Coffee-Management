namespace BrewNest.Modules.Procurement.Domain.Entities;

internal abstract class ProcurementDocument
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
internal enum PrStatus { Draft, Submitted, Approved, Rejected, Converted, Cancelled }
internal enum QuoteStatus { Received, Selected, Rejected }
internal enum PoStatus { PendingApproval, Approved, PartiallyReceived, Received, Rejected, Cancelled }
internal enum ReceiptStatus { PendingInspection, Accepted, Rejected, Returned }
internal enum InspectionState { Pending, Passed, Failed, NotRequired }

internal sealed class PurchaseRequisition : ProcurementDocument
{
    public string Department { get; set; } = string.Empty; public DateOnly RequiredOn { get; set; } public string Reason { get; set; } = string.Empty; public PrStatus Status { get; set; }
    public List<PurchaseRequisitionLine> Lines { get; set; } = [];
}
internal sealed class PurchaseRequisitionLine { public Guid Id { get; set; } public Guid RequisitionId { get; set; } public PurchaseRequisition Requisition { get; set; } = null!; public Guid ItemId { get; set; } public decimal Quantity { get; set; } public decimal EstimatedUnitPrice { get; set; } public string Notes { get; set; } = string.Empty; }
internal sealed class RequestForQuotation : ProcurementDocument
{
    public Guid RequisitionId { get; set; } public PurchaseRequisition Requisition { get; set; } = null!; public DateOnly ResponseDeadline { get; set; } public string Status { get; set; } = "Issued"; public List<RfqVendor> Vendors { get; set; } = []; public List<VendorQuotation> Quotations { get; set; } = [];
}
internal sealed class RfqVendor { public Guid Id { get; set; } public Guid RfqId { get; set; } public RequestForQuotation Rfq { get; set; } = null!; public Guid VendorId { get; set; } }
internal sealed class VendorQuotation : ProcurementDocument
{
    public Guid RfqId { get; set; } public RequestForQuotation Rfq { get; set; } = null!; public Guid VendorId { get; set; } public string VendorName { get; set; } = string.Empty; public DateOnly ValidUntil { get; set; } public string Reference { get; set; } = string.Empty; public QuoteStatus Status { get; set; } public List<VendorQuotationLine> Lines { get; set; } = [];
}
internal sealed class VendorQuotationLine { public Guid Id { get; set; } public Guid QuotationId { get; set; } public VendorQuotation Quotation { get; set; } = null!; public Guid ItemId { get; set; } public decimal Quantity { get; set; } public decimal UnitPrice { get; set; } public int LeadTimeDays { get; set; } }
internal sealed class PurchaseOrder : ProcurementDocument
{
    public Guid RequisitionId { get; set; } public Guid QuotationId { get; set; } public Guid VendorId { get; set; } public string VendorName { get; set; } = string.Empty; public PoStatus Status { get; set; } public List<PurchaseOrderLine> Lines { get; set; } = []; public List<ProcurementApprovalDecision> Approvals { get; set; } = [];
}
internal sealed class PurchaseOrderLine { public Guid Id { get; set; } public Guid PurchaseOrderId { get; set; } public PurchaseOrder PurchaseOrder { get; set; } = null!; public Guid ItemId { get; set; } public decimal Quantity { get; set; } public decimal UnitPrice { get; set; } public decimal ReceivedQuantity { get; set; } }
internal sealed class ProcurementApprovalDecision { public Guid Id { get; set; } public Guid PurchaseOrderId { get; set; } public PurchaseOrder PurchaseOrder { get; set; } = null!; public int Sequence { get; set; } public string RequiredRole { get; set; } = string.Empty; public string Decision { get; set; } = "Pending"; public string? DecidedBy { get; set; } public DateTimeOffset? DecidedAt { get; set; } public string? Remarks { get; set; } }
internal sealed class GoodsReceipt : ProcurementDocument
{
    public Guid PurchaseOrderId { get; set; } public PurchaseOrder PurchaseOrder { get; set; } = null!; public string SupplierReference { get; set; } = string.Empty; public ReceiptStatus Status { get; set; } public List<GoodsReceiptLine> Lines { get; set; } = [];
}
internal sealed class GoodsReceiptLine { public Guid Id { get; set; } public Guid GoodsReceiptId { get; set; } public GoodsReceipt GoodsReceipt { get; set; } = null!; public Guid PurchaseOrderLineId { get; set; } public Guid ItemId { get; set; } public Guid WarehouseId { get; set; } public string LotNumber { get; set; } = string.Empty; public DateOnly? ManufacturingDate { get; set; } public DateOnly? ExpiryDate { get; set; } public decimal Quantity { get; set; } public InspectionState Inspection { get; set; } public List<QualityInspectionResult> InspectionResults { get; set; } = []; }
internal sealed class QualityInspectionResult { public Guid Id { get; set; } public Guid GoodsReceiptLineId { get; set; } public GoodsReceiptLine GoodsReceiptLine { get; set; } = null!; public Guid CriterionId { get; set; } public string Criterion { get; set; } = string.Empty; public string Result { get; set; } = string.Empty; public bool Passed { get; set; } public string Remarks { get; set; } = string.Empty; public string SupportingReference { get; set; } = string.Empty; public string Inspector { get; set; } = string.Empty; public DateTimeOffset InspectedAt { get; set; } }
internal sealed class VendorReturn : ProcurementDocument { public Guid GoodsReceiptId { get; set; } public string Reason { get; set; } = string.Empty; public List<VendorReturnLine> Lines { get; set; } = []; }
internal sealed class VendorReturnLine { public Guid Id { get; set; } public Guid VendorReturnId { get; set; } public VendorReturn VendorReturn { get; set; } = null!; public Guid GoodsReceiptLineId { get; set; } public decimal Quantity { get; set; } }
internal sealed class ProcurementStatusHistory { public Guid Id { get; set; } public string DocumentType { get; set; } = string.Empty; public Guid DocumentId { get; set; } public string PreviousStatus { get; set; } = string.Empty; public string NewStatus { get; set; } = string.Empty; public string Reason { get; set; } = string.Empty; public string ChangedBy { get; set; } = string.Empty; public DateTimeOffset ChangedAt { get; set; } }
