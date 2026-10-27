using BrewNest.Contracts.Payments;
using BrewNest.Modules.Payments.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BrewNest.Modules.Payments.Infrastructure.Integrations;

internal abstract class SimulatorPaymentGateway(
    PaymentMethod method,
    string prefix,
    IConfiguration configuration,
    IHostEnvironment environment) : IPaymentGateway
{
    public PaymentMethod Method => method;

    public Task<PaymentGatewayResult> AuthorizeAsync(string paymentNumber, decimal amount, string payerReference, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!environment.IsDevelopment())
            return Task.FromResult(new PaymentGatewayResult(false, null, "Sandbox wallet payments are available only in Development."));
        if (amount <= 0 || string.IsNullOrWhiteSpace(payerReference))
        {
            return Task.FromResult(new PaymentGatewayResult(false, null, "A positive amount and sandbox payer reference are required."));
        }
        if (string.Equals(payerReference, configuration["Payments:Simulation:DeclinePayerReference"], StringComparison.Ordinal))
            return Task.FromResult(new PaymentGatewayResult(false, null, "The sandbox wallet declined this test payment."));

        return Task.FromResult(new PaymentGatewayResult(true, $"SIM-{prefix}-PAY-{Guid.NewGuid():N}", null));
    }

    public Task<PaymentGatewayResult> RefundAsync(string refundNumber, decimal amount, string sourceReference, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!environment.IsDevelopment())
            return Task.FromResult(new PaymentGatewayResult(false, null, "Sandbox wallet refunds are available only in Development."));
        return Task.FromResult(amount <= 0
            ? new PaymentGatewayResult(false, null, "A positive refund amount is required.")
            : new PaymentGatewayResult(true, $"SIM-{prefix}-REF-{Guid.NewGuid():N}", null));
    }
}

internal sealed class EasypaisaSimulatorGateway(IConfiguration configuration, IHostEnvironment environment)
    : SimulatorPaymentGateway(PaymentMethod.Easypaisa, "EP", configuration, environment);
internal sealed class JazzCashSimulatorGateway(IConfiguration configuration, IHostEnvironment environment)
    : SimulatorPaymentGateway(PaymentMethod.JazzCash, "JC", configuration, environment);
