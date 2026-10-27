namespace BrewNest.Contracts.Procurement;

/// <summary>Purchase requisition lifecycle.</summary>
public enum RequisitionStatus { /// <summary>Editable.</summary>
 Draft, /// <summary>Submitted.</summary>
 Submitted, /// <summary>Approved.</summary>
 Approved, /// <summary>Rejected.</summary>
 Rejected, /// <summary>Converted.</summary>
 Converted, /// <summary>Cancelled.</summary>
 Cancelled }
/// <summary>Request-for-quotation lifecycle.</summary>
public enum RfqStatus { /// <summary>Editable.</summary>
 Draft, /// <summary>Issued.</summary>
 Issued, /// <summary>Closed.</summary>
 Closed, /// <summary>Cancelled.</summary>
 Cancelled }
/// <summary>Vendor quotation lifecycle.</summary>
public enum QuotationStatus { /// <summary>Received.</summary>
 Received, /// <summary>Selected.</summary>
 Selected, /// <summary>Rejected.</summary>
 Rejected }
/// <summary>Purchase-order lifecycle.</summary>
public enum PurchaseOrderStatus { /// <summary>Awaiting approval.</summary>
 PendingApproval, /// <summary>Approved.</summary>
 Approved, /// <summary>Partially received.</summary>
 PartiallyReceived, /// <summary>Received.</summary>
 Received, /// <summary>Rejected.</summary>
 Rejected, /// <summary>Cancelled.</summary>
 Cancelled }
/// <summary>Goods-receipt lifecycle.</summary>
public enum GoodsReceiptStatus { /// <summary>Awaiting inspection.</summary>
 PendingInspection, /// <summary>Accepted.</summary>
 Accepted, /// <summary>Rejected.</summary>
 Rejected, /// <summary>Returned.</summary>
 Returned }
/// <summary>Inspection decision.</summary>
public enum InspectionDecision { /// <summary>Pending.</summary>
 Pending, /// <summary>Passed.</summary>
 Passed, /// <summary>Failed.</summary>
 Failed, /// <summary>Not required.</summary>
 NotRequired }

/// <summary>Requested material and estimated cost.</summary>
public sealed record ProcurementLineRequest(Guid ItemId, decimal Quantity, decimal EstimatedUnitPrice, string Notes);
/// <summary>Creates a purchase requisition.</summary>
public sealed record CreateRequisitionRequest(string Department, DateOnly RequiredOn, string Reason, ProcurementLineRequest[] Lines);
/// <summary>Changes a procurement document status with optimistic concurrency.</summary>
public sealed record ChangeProcurementStatusRequest(string Reason, string Version);
/// <summary>Purchase requisition line.</summary>
public sealed record RequisitionLineResponse(Guid Id, Guid ItemId, decimal Quantity, decimal EstimatedUnitPrice, string Notes);
/// <summary>Purchase requisition.</summary>
public sealed record RequisitionResponse(Guid Id, string Number, string Department, DateOnly RequiredOn, string Reason, RequisitionStatus Status, decimal EstimatedTotal, RequisitionLineResponse[] Lines, DateTimeOffset CreatedAt, string CreatedBy, string Version);

/// <summary>Creates and issues an RFQ.</summary>
public sealed record CreateRfqRequest(Guid RequisitionId, DateOnly ResponseDeadline, Guid[] VendorIds);
/// <summary>Request for quotation.</summary>
public sealed record RfqResponse(Guid Id, string Number, Guid RequisitionId, DateOnly ResponseDeadline, RfqStatus Status, Guid[] VendorIds, DateTimeOffset CreatedAt, string Version);
/// <summary>Quoted item and terms.</summary>
public sealed record QuotationLineRequest(Guid ItemId, decimal Quantity, decimal UnitPrice, int LeadTimeDays);
/// <summary>Captures a vendor quotation.</summary>
public sealed record SubmitQuotationRequest(Guid VendorId, DateOnly ValidUntil, string Reference, QuotationLineRequest[] Lines);
/// <summary>Vendor quotation line.</summary>
public sealed record QuotationLineResponse(Guid Id, Guid ItemId, decimal Quantity, decimal UnitPrice, int LeadTimeDays);
/// <summary>Comparable vendor quotation.</summary>
public sealed record QuotationResponse(Guid Id, Guid RfqId, Guid VendorId, string VendorName, DateOnly ValidUntil, string Reference, QuotationStatus Status, decimal Total, QuotationLineResponse[] Lines, DateTimeOffset CreatedAt, string Version);

/// <summary>Purchase-order line and receipt progress.</summary>
public sealed record PurchaseOrderLineResponse(Guid Id, Guid ItemId, decimal Quantity, decimal UnitPrice, decimal ReceivedQuantity);
/// <summary>One configured approval decision.</summary>
public sealed record ApprovalDecisionResponse(Guid Id, int Sequence, string RequiredRole, string Decision, string? DecidedBy, DateTimeOffset? DecidedAt, string? Remarks);
/// <summary>Purchase order with its approval route.</summary>
public sealed record PurchaseOrderResponse(Guid Id, string Number, Guid RequisitionId, Guid QuotationId, Guid VendorId, string VendorName, PurchaseOrderStatus Status, decimal Total, PurchaseOrderLineResponse[] Lines, ApprovalDecisionResponse[] Approvals, DateTimeOffset CreatedAt, string CreatedBy, string Version);

/// <summary>Lot-level receipt request.</summary>
public sealed record ReceiptLineRequest(Guid PurchaseOrderLineId, Guid WarehouseId, string LotNumber, DateOnly? ManufacturingDate, DateOnly? ExpiryDate, decimal Quantity);
/// <summary>Creates a goods receipt.</summary>
public sealed record CreateGoodsReceiptRequest(string SupplierReference, ReceiptLineRequest[] Lines);
/// <summary>Captured criterion result.</summary>
public sealed record InspectionResultRequest(Guid CriterionId, string Result, bool Passed, string Remarks, string SupportingReference);
/// <summary>Completes a receipt-line inspection.</summary>
public sealed record CompleteInspectionRequest(string Remarks, InspectionResultRequest[] Results, string Version);
/// <summary>Goods receipt line.</summary>
public sealed record ReceiptLineResponse(Guid Id, Guid PurchaseOrderLineId, Guid ItemId, Guid WarehouseId, string LotNumber, DateOnly? ManufacturingDate, DateOnly? ExpiryDate, decimal Quantity, InspectionDecision Inspection, decimal ReturnedQuantity = 0m);
/// <summary>Goods receipt and inspection state.</summary>
public sealed record GoodsReceiptResponse(Guid Id, string Number, Guid PurchaseOrderId, string SupplierReference, GoodsReceiptStatus Status, ReceiptLineResponse[] Lines, DateTimeOffset CreatedAt, string CreatedBy, string Version);
/// <summary>Creates a vendor return.</summary>
public sealed record CreateVendorReturnRequest(Guid GoodsReceiptId, string Reason, VendorReturnLineRequest[] Lines);
/// <summary>Vendor-return quantity.</summary>
public sealed record VendorReturnLineRequest(Guid GoodsReceiptLineId, decimal Quantity);
/// <summary>Completed vendor return.</summary>
public sealed record VendorReturnResponse(Guid Id, string Number, Guid GoodsReceiptId, string Reason, decimal TotalQuantity, DateTimeOffset CreatedAt, string CreatedBy);
/// <summary>Append-only procurement status history.</summary>
public sealed record ProcurementStatusHistoryResponse(Guid Id, string DocumentType, Guid DocumentId, string PreviousStatus, string NewStatus, string Reason, string ChangedBy, DateTimeOffset ChangedAt);

/// <summary>Catalog item snapshot.</summary>
public sealed record ProcurementItemReference(Guid Id, string Code, string Name, bool RequiresInspection, ProcurementInspectionCriterion[] Criteria);
/// <summary>Catalog vendor snapshot.</summary>
public sealed record ProcurementVendorReference(Guid Id, string Code, string Name);
/// <summary>Catalog warehouse snapshot.</summary>
public sealed record ProcurementWarehouseReference(Guid Id, string Code, string Name);
/// <summary>Configurable inspection criterion snapshot.</summary>
public sealed record ProcurementInspectionCriterion(Guid Id, string Name, string ResultType, string? MinimumValue, string? MaximumValue, string? Unit, bool IsMandatory);
/// <summary>Active procurement reference data.</summary>
public sealed record ProcurementReferenceData(ProcurementItemReference[] Items, ProcurementVendorReference[] Vendors, ProcurementWarehouseReference[] Warehouses);
/// <summary>Supplies Catalog-owned procurement reference data.</summary>
public interface IProcurementCatalogPort
{
    /// <summary>Gets active purchasable items, vendors, and warehouses.</summary>
    Task<ProcurementReferenceData> GetReferencesAsync(CancellationToken cancellationToken);
    /// <summary>Gets one active purchasable item.</summary>
    Task<ProcurementItemReference?> GetItemAsync(Guid itemId, CancellationToken cancellationToken);
}

/// <summary>Required approval role at a sequence.</summary>
public sealed record ProcurementApprovalStep(int Sequence, string RequiredRole);
/// <summary>Administration-owned approval route snapshot.</summary>
public sealed record ProcurementApprovalRoute(Guid PolicyId, string PolicyName, string Mode, ProcurementApprovalStep[] Steps);
/// <summary>Resolves configurable Administration approval policies.</summary>
public interface IProcurementApprovalPolicyPort
{
    /// <summary>Resolves the active policy matching a process and amount.</summary>
    Task<ProcurementApprovalRoute?> ResolveAsync(string processCode, decimal? amount, CancellationToken cancellationToken);
}

/// <summary>Accepted material passed to Inventory.</summary>
public sealed record ProcurementInventoryReceipt(Guid ItemId, Guid WarehouseId, string LotNumber, DateOnly? ManufacturingDate, DateOnly? ExpiryDate, decimal Quantity);
/// <summary>Stock returned to its vendor.</summary>
public sealed record ProcurementInventoryReturn(Guid ItemId, Guid WarehouseId, string LotNumber, decimal Quantity);
/// <summary>Inventory boundary result.</summary>
public sealed record ProcurementInventoryResult(bool Succeeded, string[] Errors);
/// <summary>Inventory operations required by Procurement.</summary>
public interface IProcurementInventoryPort
{
    /// <summary>Receives inspected or inspection-exempt material.</summary>
    Task<ProcurementInventoryResult> ReceiveAsync(string reference, ProcurementInventoryReceipt[] lines, string actor, CancellationToken cancellationToken);
    /// <summary>Removes available stock for a vendor return.</summary>
    Task<ProcurementInventoryResult> ReturnAsync(string reference, ProcurementInventoryReturn[] lines, string actor, CancellationToken cancellationToken);
}
