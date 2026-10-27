namespace BrewNest.Contracts.Returns;

/// <summary>Return/RMA lifecycle.</summary>
public enum ReturnStatus { /// <summary>Awaiting approval.</summary>
 PendingApproval, /// <summary>Approved for inspection.</summary>
 InspectionPending, /// <summary>Completed with disposition.</summary>
 Completed, /// <summary>Rejected.</summary>
 Rejected, /// <summary>Cancelled.</summary>
 Cancelled }
/// <summary>Returned-item disposition.</summary>
public enum ReturnDisposition { /// <summary>Not inspected.</summary>
 Pending, /// <summary>Restocked.</summary>
 Restocked, /// <summary>Disposed as wastage.</summary>
 Wastage }
/// <summary>Void request lifecycle.</summary>
public enum VoidStatus { /// <summary>Awaiting approval.</summary>
 PendingApproval, /// <summary>Approved and applied.</summary>
 Completed, /// <summary>Rejected.</summary>
 Rejected }

/// <summary>Creates a bill-backed return request.</summary>
public sealed record CreateReturnRequest(string BillNumber,string Reason,CreateReturnLineRequest[] Lines);
/// <summary>Quantity from an original bill line.</summary>
public sealed record CreateReturnLineRequest(Guid SalesLineId,decimal Quantity);
/// <summary>Optimistic status decision.</summary>
public sealed record ReturnDecisionRequest(bool Approve,string Reason,string Version);
/// <summary>Captures returned-food evidence and disposition.</summary>
public sealed record InspectReturnLineRequest(bool IsSealed,bool IsPackagingIntact,bool ManufacturingDateChecked,DateOnly? ManufacturingDate,DateOnly? ExpiryDate,string LotNumber,Guid WarehouseId,bool BatchVerified,bool QualityPassed,bool RequestRestock,string Criterion,string Result,string Remarks,string SupportingReference,string Version);
/// <summary>Original line and return disposition.</summary>
public sealed record ReturnLineResponse(Guid Id,Guid SalesLineId,Guid ItemId,string ItemCode,string ItemName,decimal Quantity,decimal UnitPrice,decimal Amount,bool IsSealed,bool IsPackagingIntact,bool ManufacturingDateChecked,DateOnly? ManufacturingDate,DateOnly? ExpiryDate,string? LotNumber,Guid? WarehouseId,bool BatchVerified,bool QualityPassed,ReturnDisposition Disposition,string? Inspector,DateTimeOffset? InspectedAt,string Version);
/// <summary>Return request, RMA, credit, and refund state.</summary>
public sealed record ReturnResponse(Guid Id,string ReturnNumber,string RmaNumber,Guid SalesOrderId,string OrderNumber,Guid BillId,string BillNumber,string Reason,ReturnStatus Status,decimal TotalAmount,ReturnLineResponse[] Lines,string? CreditNoteNumber,string? RefundReference,DateTimeOffset CreatedAt,string CreatedBy,string Version);
/// <summary>Creates a void request for an order.</summary>
public sealed record CreateVoidRequest(Guid SalesOrderId,string Reason);
/// <summary>Void transaction and approval state.</summary>
public sealed record VoidResponse(Guid Id,string VoidNumber,Guid SalesOrderId,string OrderNumber,string? BillNumber,bool IsAfterPayment,decimal Amount,VoidStatus Status,string Reason,string? RefundReference,DateTimeOffset CreatedAt,string CreatedBy,string Version);
/// <summary>Append-only Returns status history.</summary>
public sealed record ReturnsStatusHistoryResponse(Guid Id,string DocumentType,Guid DocumentId,string PreviousStatus,string NewStatus,string Reason,string ChangedBy,DateTimeOffset ChangedAt);

/// <summary>Immutable billed line supplied by Sales.</summary>
public sealed record ReturnsSalesLine(Guid SalesLineId,Guid ItemId,string ItemCode,string ItemName,decimal Quantity,decimal UnitPrice);
/// <summary>Verified original bill snapshot supplied by Sales.</summary>
public sealed record ReturnsBillSnapshot(Guid SalesOrderId,string OrderNumber,string OrderStatus,Guid BillId,string BillNumber,decimal NetAmount,ReturnsSalesLine[] Lines);
/// <summary>Sales order snapshot used by void processing.</summary>
public sealed record ReturnsOrderSnapshot(Guid SalesOrderId,string OrderNumber,string Status,string? BillNumber,decimal Amount);
/// <summary>Sales boundary result.</summary>
public sealed record ReturnsPortResult(bool Succeeded,string[] Errors);
/// <summary>Sales operations needed by Returns.</summary>
public interface IReturnsSalesPort
{
    /// <summary>Finds and verifies an original bill.</summary>
    Task<ReturnsBillSnapshot?> FindBillAsync(string billNumber,CancellationToken cancellationToken);
    /// <summary>Gets an order for void classification.</summary>
    Task<ReturnsOrderSnapshot?> GetOrderAsync(Guid salesOrderId,CancellationToken cancellationToken);
    /// <summary>Cancels an approved unpaid order.</summary>
    Task<ReturnsPortResult> VoidUnpaidAsync(Guid salesOrderId,string reason,string actor,CancellationToken cancellationToken);
    /// <summary>Voids a billed or closed order after the paid-void workflow is approved.</summary>
    Task<ReturnsPortResult> VoidPaidAsync(Guid salesOrderId,string reason,string actor,CancellationToken cancellationToken);
}

/// <summary>Configured role in an approval route.</summary>
public sealed record ReturnsApprovalStep(int Sequence,string RequiredRole);
/// <summary>Administration-owned approval route.</summary>
public sealed record ReturnsApprovalRoute(string ProcessCode,ReturnsApprovalStep[] Steps);
/// <summary>Resolves Returns approval rules from Administration.</summary>
public interface IReturnsApprovalPolicyPort
{
    /// <summary>Gets the active role route for a process.</summary>
    Task<ReturnsApprovalRoute?> ResolveAsync(string processCode,CancellationToken cancellationToken);
}

/// <summary>Returned lot accepted into Inventory.</summary>
public sealed record ReturnsInventoryRestock(Guid ItemId,Guid WarehouseId,string LotNumber,DateOnly? ManufacturingDate,DateOnly ExpiryDate,decimal Quantity);
/// <summary>Returned item routed to disposal without increasing stock.</summary>
public sealed record ReturnsInventoryDisposal(Guid ItemId,string LotNumber,decimal Quantity,string Reason);
/// <summary>Inventory disposition operations for Returns.</summary>
public interface IReturnsInventoryPort
{
    /// <summary>Restocks an approved, passed, sealed return.</summary>
    Task<ReturnsPortResult> RestockAsync(string reference,ReturnsInventoryRestock item,string actor,CancellationToken cancellationToken);
    /// <summary>Records disposal without increasing available stock.</summary>
    Task<ReturnsPortResult> DisposeAsync(string reference,ReturnsInventoryDisposal item,string actor,CancellationToken cancellationToken);
}

/// <summary>Payment status used to classify a void.</summary>
public sealed record ReturnsPaymentState(bool IsPaid,decimal PaidAmount,decimal RefundedAmount,string? PaymentReference);
/// <summary>Payments operations needed by Returns.</summary>
public interface IReturnsPaymentPort
{
    /// <summary>Gets payment state for an order/bill.</summary>
    Task<ReturnsPaymentState> GetStateAsync(Guid salesOrderId,string? billNumber,decimal amount,CancellationToken cancellationToken);
    /// <summary>Submits an approved refund request to the replaceable payment boundary.</summary>
    Task<string> RequestRefundAsync(Guid salesOrderId,string sourceReference,decimal amount,string reason,string actor,CancellationToken cancellationToken);
}
