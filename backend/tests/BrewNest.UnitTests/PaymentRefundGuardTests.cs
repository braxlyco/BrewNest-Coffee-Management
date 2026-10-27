using BrewNest.Contracts.Payments;
using BrewNest.Modules.Payments.Application.Interfaces;
using BrewNest.Modules.Payments.Application.Services;
using BrewNest.Modules.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.UnitTests;

/// <summary>Refund allocation and wallet input safeguards.</summary>
public sealed class PaymentRefundGuardTests
{
    private static readonly Guid OrderId = Guid.Parse("fa4e2162-1123-4a6f-8a8c-5598675b4930");

    /// <summary>Wallet numbers must include an international dialing code.</summary>
    [Theory]
    [InlineData("03001234567")]
    [InlineData("+123")]
    [InlineData("+923001234567890123")]
    public async Task WalletPaymentRequiresCountryCodeAndValidDigitCount(string phone)
    {
        await using var database = Database();
        var result = await Service(database).Create(
            new(OrderId, "INV-1", 100m, PaymentMethod.Easypaisa, phone, ""), new("cashier"), default);
        Assert.Equal(PaymentsResultStatus.Validation, result.Status);
    }

    /// <summary>Unknown numeric enum values must not enter persistence.</summary>
    [Fact]
    public async Task UndefinedPaymentMethodIsRejected()
    {
        await using var database = Database();
        var result = await Service(database).Create(
            new(OrderId, "INV-1", 100m, (PaymentMethod)999, "ref", ""), new("cashier"), default);
        Assert.Equal(PaymentsResultStatus.Validation, result.Status);
    }

    /// <summary>Refunds cannot exceed received money or mutate an idempotency source.</summary>
    [Fact]
    public async Task RefundCannotExceedVerifiedAmountOrChangeAnExistingSource()
    {
        await using var database = Database();
        var service = Service(database);
        var payment = (await service.Create(
            new(OrderId, "INV-1", 100m, PaymentMethod.Cash, "", ""), new("cashier"), default)).Value!;
        await service.Verify(payment.Id, new(true, "Received", payment.Version), new("manager"), default);
        await service.RequestRefundAsync(OrderId, "CN-1", 80m, "Return", "manager", default);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RequestRefundAsync(OrderId, "CN-2", 30m, "Return", "manager", default));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RequestRefundAsync(OrderId, "CN-1", 70m, "Changed amount", "manager", default));
        var state = await service.GetStateAsync(OrderId, "INV-1", 100m, default);
        Assert.Equal(80m, state.RefundedAmount);
    }

    /// <summary>A single wallet payment selects the same simulator for refund.</summary>
    [Fact]
    public async Task SingleWalletPaymentRoutesRefundToOriginalSandboxAdapter()
    {
        await using var database = Database();
        var gateway = new Gateway(PaymentMethod.Easypaisa);
        var service = Service(database, gateway);
        var payment = await service.Create(
            new(OrderId, "INV-1", 100m, PaymentMethod.Easypaisa, "+923001234567", ""), new("cashier"), default);
        Assert.True(payment.Succeeded);
        await service.RequestRefundAsync(OrderId, "CN-1", 50m, "Return", "manager", default);
        var refund = Assert.Single((await service.GetRefunds(default)).Value!);
        var processed = await service.ProcessRefund(refund.Id, new(true, "Approved", refund.Version), new("manager"), default);
        Assert.Equal(RefundStatus.Completed, processed.Value!.Status);
        Assert.Equal(1, gateway.Refunds);
        Assert.StartsWith("SIM-WALLET-REF", processed.Value.GatewayReference);
    }

    /// <summary>Mixed tenders stay on the explicit manual simulator path.</summary>
    [Fact]
    public async Task MultipleTendersUseExplicitManualSimulator()
    {
        await using var database = Database();
        var gateway = new Gateway(PaymentMethod.Easypaisa);
        var service = Service(database, gateway);
        await service.Create(new(OrderId, "INV-1", 50m, PaymentMethod.Easypaisa, "+923001234567", ""), new("cashier"), default);
        var cash = (await service.Create(new(OrderId, "INV-1", 50m, PaymentMethod.Cash, "", ""), new("cashier"), default)).Value!;
        await service.Verify(cash.Id, new(true, "Received", cash.Version), new("manager"), default);
        await service.RequestRefundAsync(OrderId, "CN-2", 75m, "Return", "manager", default);
        var refund = Assert.Single((await service.GetRefunds(default)).Value!);
        var processed = await service.ProcessRefund(refund.Id, new(true, "Approved", refund.Version), new("manager"), default);
        Assert.Equal(RefundStatus.Completed, processed.Value!.Status);
        Assert.Equal(0, gateway.Refunds);
        Assert.StartsWith("SIM-MANUAL-REF", processed.Value.GatewayReference);
    }

    private static PaymentsDbContext Database() => new(new DbContextOptionsBuilder<PaymentsDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static PaymentsService Service(PaymentsDbContext database, Gateway? gateway = null) =>
        new(database, new SalesPort(), [gateway ?? new Gateway(PaymentMethod.Easypaisa)],
            new Numbers(), TimeProvider.System);
    private sealed class SalesPort : IPaymentsSalesPort
    {
        public Task<PaymentsBillSnapshot?> GetBillAsync(Guid id, string bill, CancellationToken token) =>
            Task.FromResult<PaymentsBillSnapshot?>(new(OrderId, "SO-1", "INV-1", 100m, "Billed"));
    }
    private sealed class Numbers : IPaymentDocumentNumbers
    {
        public string Payment() => $"PAY-{Guid.NewGuid():N}";
        public string Refund() => $"REF-{Guid.NewGuid():N}";
    }
    private sealed class Gateway(PaymentMethod method) : IPaymentGateway
    {
        public PaymentMethod Method => method;
        public int Refunds { get; private set; }
        public Task<PaymentGatewayResult> AuthorizeAsync(string number, decimal amount, string reference, CancellationToken token) =>
            Task.FromResult(new PaymentGatewayResult(true, $"SIM-WALLET-PAY-{Guid.NewGuid():N}", null));
        public Task<PaymentGatewayResult> RefundAsync(string number, decimal amount, string reference, CancellationToken token)
        {
            Refunds++;
            return Task.FromResult(new PaymentGatewayResult(true, $"SIM-WALLET-REF-{Guid.NewGuid():N}", null));
        }
    }
}
