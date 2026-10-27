namespace BrewNest.Contracts.Payments;

/// <summary>Supported customer payment methods.</summary>
public enum PaymentMethod
{
    /// <summary>Cash received at the café.</summary>
    Cash,
    /// <summary>Cash collected on delivery.</summary>
    Cod,
    /// <summary>Bank transfer awaiting reference verification.</summary>
    BankTransfer,
    /// <summary>Easypaisa wallet payment.</summary>
    Easypaisa,
    /// <summary>JazzCash wallet payment.</summary>
    JazzCash
}
/// <summary>Payment lifecycle state.</summary>
public enum PaymentStatus
{
    /// <summary>Recorded but not yet verified.</summary>
    PendingVerification,
    /// <summary>Verified as received.</summary>
    Verified,
    /// <summary>Verification or gateway processing failed.</summary>
    Failed,
    /// <summary>Reconciled into a settlement.</summary>
    Settled,
    /// <summary>Cancelled without physical deletion.</summary>
    Cancelled
}
/// <summary>Refund lifecycle state.</summary>
public enum RefundStatus
{
    /// <summary>Awaiting refund processing.</summary>
    Requested,
    /// <summary>Being processed by an adapter.</summary>
    Processing,
    /// <summary>Refund execution completed.</summary>
    Completed,
    /// <summary>Refund execution failed.</summary>
    Failed,
    /// <summary>Refund request was rejected or cancelled.</summary>
    Cancelled
}

/// <summary>Creates a payment against a verified Sales bill.</summary>
public sealed record CreatePaymentRequest(Guid SalesOrderId, string BillNumber, decimal Amount, PaymentMethod Method, string PayerReference, string Notes);
/// <summary>Verifies or rejects a pending payment.</summary>
public sealed record VerifyPaymentRequest(bool Approve, string Remarks, string Version);
/// <summary>Settles a verified payment.</summary>
public sealed record SettlePaymentRequest(string SettlementReference, string Remarks, string Version);
/// <summary>Processes or rejects a requested refund.</summary>
public sealed record ProcessRefundRequest(bool Approve, string Remarks, string Version);
/// <summary>Payment projection safe for API clients.</summary>
public sealed record PaymentResponse(Guid Id, string PaymentNumber, Guid SalesOrderId, string OrderNumber, string BillNumber, decimal Amount, PaymentMethod Method, PaymentStatus Status, string PayerReference, string? GatewayReference, string Notes, DateTimeOffset CreatedAt, string CreatedBy, DateTimeOffset? VerifiedAt, string? VerifiedBy, DateTimeOffset? SettledAt, string? SettledBy, string? SettlementReference, string Version);
/// <summary>Refund projection safe for API clients.</summary>
public sealed record RefundResponse(Guid Id, string RefundNumber, string SourceReference, Guid? PaymentId, decimal Amount, RefundStatus Status, string Reason, string? GatewayReference, DateTimeOffset RequestedAt, string RequestedBy, DateTimeOffset? ProcessedAt, string? ProcessedBy, string? Remarks, string Version);
/// <summary>Append-only payment or refund transition.</summary>
public sealed record PaymentStatusHistoryResponse(Guid Id, string DocumentType, Guid DocumentId, string PreviousStatus, string NewStatus, string Reason, string ChangedBy, DateTimeOffset ChangedAt);
/// <summary>Sales-owned bill snapshot consumed by Payments.</summary>
public sealed record PaymentsBillSnapshot(Guid SalesOrderId, string OrderNumber, string BillNumber, decimal NetAmount, string Status);

/// <summary>Sales query boundary used by Payments without accessing Sales persistence.</summary>
public interface IPaymentsSalesPort
{
    /// <summary>Gets a billed order matching both identifiers.</summary>
    Task<PaymentsBillSnapshot?> GetBillAsync(Guid salesOrderId, string billNumber, CancellationToken cancellationToken);
}

/// <summary>COD handoff boundary reserved for the Delivery module.</summary>
public interface IPaymentsCodPort
{
    /// <summary>Records collected COD against a Sales bill.</summary>
    Task<Guid> RecordCodAsync(Guid salesOrderId, string billNumber, decimal amount, string riderReference, string actor, CancellationToken cancellationToken);
}
