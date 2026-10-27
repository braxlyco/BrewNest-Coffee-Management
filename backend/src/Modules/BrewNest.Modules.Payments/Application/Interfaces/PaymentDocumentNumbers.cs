using Microsoft.Extensions.Configuration;

namespace BrewNest.Modules.Payments.Application.Interfaces;

internal sealed class PaymentDocumentNumbers(IConfiguration configuration) : IPaymentDocumentNumbers
{
    public string Payment() => Next(configuration["Payments:DocumentNumbers:PaymentPrefix"] ?? "PAY");
    public string Refund() => Next(configuration["Payments:DocumentNumbers:RefundPrefix"] ?? "RFND");
    private static string Next(string prefix) => $"{prefix}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 1 + 14 + 1 + 8, prefix.Length + 1 + 14 + 1 + 32)];
}
