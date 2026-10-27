using BrewNest.Contracts.Payments;

namespace BrewNest.Modules.Payments.Domain.Entities;

internal abstract class PaymentDocument
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

internal sealed class Payment : PaymentDocument
{
    public Guid SalesOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string BillNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string PayerReference { get; set; } = string.Empty;
    public string? GatewayReference { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTimeOffset? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    public DateTimeOffset? SettledAt { get; set; }
    public string? SettledBy { get; set; }
    public string? SettlementReference { get; set; }
}

internal sealed class Refund : PaymentDocument
{
    public Guid? SalesOrderId { get; set; }
    public string SourceReference { get; set; } = string.Empty;
    public Guid? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public RefundStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? GatewayReference { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? Remarks { get; set; }
}

internal sealed class PaymentStatusHistory
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public Guid DocumentId { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; set; }
}
