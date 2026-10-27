using BrewNest.Contracts.Payments;
using BrewNest.Modules.Payments.Application.Interfaces;
using BrewNest.Modules.Payments.Application.Services;
using BrewNest.Modules.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.UnitTests;

/// <summary>Verifies payment, gateway, settlement, and refund rules.</summary>
public sealed class PaymentsServiceTests
{
    private static readonly Guid OrderId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");

    /// <summary>An unknown Sales bill cannot receive payment.</summary>
    [Fact]
    public async Task UnknownBillIsRejected()
    {
        await using var database = Database();
        var result = await Service(database, sales: new SalesPort(false)).Create(Request(), Context(), default);
        Assert.Equal(PaymentsResultStatus.Validation, result.Status);
    }

    /// <summary>Committed payments cannot exceed the verified bill total.</summary>
    [Fact]
    public async Task OverpaymentIsRejected()
    {
        await using var database = Database();
        var service = Service(database);
        Assert.True((await service.Create(Request(800m), Context(), default)).Succeeded);
        var result = await service.Create(Request(300m), Context(), default);
        Assert.Equal(PaymentsResultStatus.Validation, result.Status);
    }

    /// <summary>Wallet methods use their replaceable simulator and become verified.</summary>
    [Fact]
    public async Task WalletPaymentUsesGateway()
    {
        await using var database = Database();
        var gateway = new Gateway(PaymentMethod.Easypaisa);
        var result = await Service(database, gateways: [gateway]).Create(
            Request(500m, PaymentMethod.Easypaisa, "+923001234567"), Context(), default);
        Assert.Equal(PaymentStatus.Verified, result.Value!.Status);
        Assert.StartsWith("SIM-", result.Value.GatewayReference);
        Assert.Equal(1, gateway.Authorizations);
    }

    /// <summary>Manual payments require verification before settlement.</summary>
    [Fact]
    public async Task BankPaymentCanBeVerifiedAndSettled()
    {
        await using var database = Database();
        var service = Service(database);
        var created = (await service.Create(Request(500m, PaymentMethod.BankTransfer, "BANK-1"), Context(), default)).Value!;
        var verified = (await service.Verify(created.Id, new(true, "Bank receipt checked", created.Version), Context(), default)).Value!;
        var settled = await service.Settle(verified.Id, new("SET-1", "Bank reconciliation", verified.Version), Context(), default);
        Assert.Equal(PaymentStatus.Settled, settled.Value!.Status);
    }

    /// <summary>Return refund requests are idempotent and explicitly processed.</summary>
    [Fact]
    public async Task RefundRequestIsIdempotentAndProcessed()
    {
        await using var database = Database();
        var service = Service(database);
        await service.Create(Request(), Context(), default);
        var created = Assert.Single((await service.GetPayments(default)).Value!);
        await service.Verify(created.Id, new(true, "Received", created.Version), Context(), default);
        var first = await service.RequestRefundAsync(OrderId, "CN-1", 100m, "Return", "manager", default);
        var second = await service.RequestRefundAsync(OrderId, "CN-1", 100m, "Return", "manager", default);
        Assert.Equal(first, second);
        var refund = Assert.Single((await service.GetRefunds(default)).Value!);
        var processed = await service.ProcessRefund(refund.Id, new(true, "Cash returned", refund.Version), Context(), default);
        Assert.Equal(RefundStatus.Completed, processed.Value!.Status);
    }

    private static CreatePaymentRequest Request(decimal amount = 500m, PaymentMethod method = PaymentMethod.Cash, string reference = "") =>
        new(OrderId, "INV-1", amount, method, reference, "test");
    private static PaymentsContext Context() => new("cashier");
    private static PaymentsDbContext Database() => new(new DbContextOptionsBuilder<PaymentsDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static PaymentsService Service(PaymentsDbContext database, SalesPort? sales = null, IPaymentGateway[]? gateways = null) =>
        new(database, sales ?? new SalesPort(true), gateways ?? [new Gateway(PaymentMethod.Easypaisa), new Gateway(PaymentMethod.JazzCash)], new Numbers(), TimeProvider.System);

    private sealed class SalesPort(bool found) : IPaymentsSalesPort
    {
        public Task<PaymentsBillSnapshot?> GetBillAsync(Guid salesOrderId, string billNumber, CancellationToken token) =>
            Task.FromResult<PaymentsBillSnapshot?>(found ? new(OrderId, "SO-1", "INV-1", 1000m, "Billed") : null);
    }
    private sealed class Numbers : IPaymentDocumentNumbers
    {
        public string Payment() => $"PAY-{Guid.NewGuid():N}";
        public string Refund() => $"RFND-{Guid.NewGuid():N}";
    }
    private sealed class Gateway(PaymentMethod method) : IPaymentGateway
    {
        public PaymentMethod Method => method;
        public int Authorizations { get; private set; }
        public Task<PaymentGatewayResult> AuthorizeAsync(string number, decimal amount, string reference, CancellationToken token) { Authorizations++; return Task.FromResult(new PaymentGatewayResult(true, $"SIM-{Guid.NewGuid():N}", null)); }
        public Task<PaymentGatewayResult> RefundAsync(string number, decimal amount, string reference, CancellationToken token) => Task.FromResult(new PaymentGatewayResult(true, $"SIM-REF-{Guid.NewGuid():N}", null));
    }
}
