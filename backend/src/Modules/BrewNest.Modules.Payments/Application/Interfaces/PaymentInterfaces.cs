using BrewNest.Contracts.Payments;

namespace BrewNest.Modules.Payments.Application.Interfaces;

/// <summary>Result returned by a replaceable payment gateway adapter.</summary>
public sealed record PaymentGatewayResult(bool Succeeded, string? Reference, string? Error);

/// <summary>Replaceable external wallet or payment processor.</summary>
public interface IPaymentGateway
{
    /// <summary>Gets the payment method supported by this adapter.</summary>
    PaymentMethod Method { get; }
    /// <summary>Simulates or performs payment authorization.</summary>
    Task<PaymentGatewayResult> AuthorizeAsync(string paymentNumber, decimal amount, string payerReference, CancellationToken cancellationToken);
    /// <summary>Simulates or performs a refund.</summary>
    Task<PaymentGatewayResult> RefundAsync(string refundNumber, decimal amount, string sourceReference, CancellationToken cancellationToken);
}

internal interface IPaymentDocumentNumbers
{
    string Payment();
    string Refund();
}
