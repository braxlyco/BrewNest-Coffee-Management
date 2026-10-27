using BrewNest.Contracts.Returns;
using BrewNest.Modules.Returns.Application.Interfaces;
using BrewNest.Modules.Returns.Application.Services;
using BrewNest.Modules.Returns.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BrewNest.UnitTests;

/// <summary>Verifies Returns, inspection, refund, and void rules.</summary>
public sealed class ReturnsRulesTests
{
    private static readonly Guid LineId = Guid.NewGuid();
    private static readonly Guid ItemId = Guid.NewGuid();

    /// <summary>Original bill is mandatory.</summary>
    [Fact]
    public async Task UnknownBillIsRejected()
    {
        await using var db = Database();
        var result = await Service(db, new SalesPort(false)).CreateReturn(
            new("missing", "reason", [new(LineId, 1)]), Context(), default);

        Assert.Equal(ReturnsResultStatus.Validation, result.Status);
    }

    /// <summary>Returned quantity cannot exceed billed quantity.</summary>
    [Fact]
    public async Task QuantityCannotExceedBill()
    {
        await using var db = Database();
        var result = await Service(db).CreateReturn(
            new("INV-1", "reason", [new(LineId, 3)]), Context(), default);

        Assert.Equal(ReturnsResultStatus.Validation, result.Status);
    }

    /// <summary>Approval role comes from Administration.</summary>
    [Fact]
    public async Task ReturnApprovalEnforcesRole()
    {
        await using var db = Database();
        var service = Service(db);
        var created = (await service.CreateReturn(
            new("INV-1", "reason", [new(LineId, 1)]), Context(), default)).Value!;

        var denied = await service.DecideReturn(
            created.Id, new(true, "approve", created.Version), new("cashier", ["Cashier"]), default);
        Assert.Equal(ReturnsResultStatus.Forbidden, denied.Status);

        var approved = await service.DecideReturn(
            created.Id, new(true, "approve", created.Version), Context(), default);
        Assert.Equal(ReturnStatus.InspectionPending, approved.Value!.Status);
    }

    /// <summary>Opened or unverified food cannot be restocked.</summary>
    [Fact]
    public async Task InvalidFoodEvidenceBlocksRestock()
    {
        await using var db = Database();
        var service = Service(db);
        var value = await Approved(service);
        var line = value.Lines[0];

        var result = await service.Inspect(
            value.Id,
            line.Id,
            new(false, true, true, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), "LOT", Guid.NewGuid(), true,
                true, true, "seal", "fail", "opened", "ref", line.Version),
            Context(),
            default);

        Assert.Equal(ReturnsResultStatus.Validation, result.Status);
    }

    /// <summary>Failed food goes to disposal and creates credit/refund records.</summary>
    [Fact]
    public async Task DisposalCompletesCreditAndRefund()
    {
        await using var db = Database();
        var payment = new PaymentPort();
        var service = Service(db, payment: payment);
        var value = await Approved(service);
        var line = value.Lines[0];

        var completed = await service.Inspect(
            value.Id,
            line.Id,
            new(false, false, false, null, null, string.Empty, Guid.Empty, false, false, false,
                "condition", "failed", "dispose", "photo", line.Version),
            Context(),
            default);

        Assert.Equal(ReturnStatus.Completed, completed.Value!.Status);
        Assert.NotNull(completed.Value.CreditNoteNumber);
        Assert.Equal(1, payment.Refunds);
    }

    /// <summary>Paid orders use after-payment void and refund flow.</summary>
    [Fact]
    public async Task PaidVoidCreatesRefundAfterApproval()
    {
        await using var db = Database();
        var payment = new PaymentPort();
        var service = Service(db, payment: payment);
        var created = (await service.CreateVoid(
            new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "duplicate payment"),
            Context(),
            default)).Value!;

        Assert.True(created.IsAfterPayment);
        var completed = await service.DecideVoid(
            created.Id, new(true, "approved", created.Version), Context(), default);
        Assert.Equal(VoidStatus.Completed, completed.Value!.Status);
        Assert.NotNull(completed.Value.RefundReference);
    }

    /// <summary>Only the paid balance is refunded and Sales is cancelled after approval.</summary>
    [Fact]
    public async Task PaidVoidRefundsOnlyCollectedBalanceAndCallsSalesCancellation()
    {
        await using var db = Database();
        var payment = new PaymentPort { PaidAmount = 75m, RefundedAmount = 25m };
        var sales = new SalesPort(true);
        var service = Service(db, sales, payment);
        var orderId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var created = (await service.CreateVoid(new(orderId, "Customer cancellation"), Context(), default)).Value!;
        Assert.Equal(50m, created.Amount);
        var approved = await service.DecideVoid(created.Id, new(true, "Approved", created.Version), Context(), default);
        Assert.Equal(VoidStatus.Completed, approved.Value!.Status);
        Assert.Equal(50m, payment.LastRefundAmount);
        Assert.Equal(1, sales.PaidVoids);
    }

    /// <summary>One order cannot have overlapping open void requests.</summary>
    [Fact]
    public async Task SecondOpenVoidForSameOrderIsRejected()
    {
        await using var db = Database();
        var service = Service(db);
        var orderId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        Assert.True((await service.CreateVoid(new(orderId, "First request"), Context(), default)).Succeeded);
        var duplicate = await service.CreateVoid(new(orderId, "Duplicate request"), Context(), default);
        Assert.Equal(ReturnsResultStatus.Conflict, duplicate.Status);
    }

    private static async Task<ReturnResponse> Approved(ReturnsService service)
    {
        var created = (await service.CreateReturn(
            new("INV-1", "reason", [new(LineId, 1)]), Context(), default)).Value!;
        return (await service.DecideReturn(
            created.Id, new(true, "approve", created.Version), Context(), default)).Value!;
    }

    private static ReturnsDbContext Database() => new(
        new DbContextOptionsBuilder<ReturnsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ReturnsService Service(
        ReturnsDbContext db,
        SalesPort? sales = null,
        PaymentPort? payment = null) =>
        new(
            db,
            sales ?? new SalesPort(true),
            new ApprovalPort(),
            new InventoryPort(),
            payment ?? new PaymentPort(),
            new ReturnsDocumentNumbers(new ConfigurationBuilder().Build()),
            TimeProvider.System);

    private static ReturnsContext Context() =>
        new("manager", ["Sales Manager", "Finance", "Store/QC Manager"]);

    private sealed class SalesPort(bool found = true) : IReturnsSalesPort
    {
        public int PaidVoids { get; private set; }
        public Task<ReturnsBillSnapshot?> FindBillAsync(string number, CancellationToken token) =>
            Task.FromResult<ReturnsBillSnapshot?>(found
                ? new(
                    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    "SO-1",
                    "Billed",
                    Guid.NewGuid(),
                    "INV-1",
                    200m,
                    [new(LineId, ItemId, "FOOD", "Sealed food", 2m, 100m)])
                : null);

        public Task<ReturnsOrderSnapshot?> GetOrderAsync(Guid id, CancellationToken token) =>
            Task.FromResult<ReturnsOrderSnapshot?>(new(id, "SO-1", "Billed", "INV-1", 200m));

        public Task<ReturnsPortResult> VoidUnpaidAsync(
            Guid id,
            string reason,
            string actor,
            CancellationToken token) => Task.FromResult(new ReturnsPortResult(true, []));

        public Task<ReturnsPortResult> VoidPaidAsync(
            Guid id, string reason, string actor, CancellationToken token)
        {
            PaidVoids++;
            return Task.FromResult(new ReturnsPortResult(true, []));
        }
    }

    private sealed class ApprovalPort : IReturnsApprovalPolicyPort
    {
        public Task<ReturnsApprovalRoute?> ResolveAsync(string process, CancellationToken token) =>
            Task.FromResult<ReturnsApprovalRoute?>(new(
                process,
                [new(1, process == "ReturnedFoodRestocking" ? "Store/QC Manager" : "Sales Manager")]));
    }

    private sealed class InventoryPort : IReturnsInventoryPort
    {
        public Task<ReturnsPortResult> RestockAsync(
            string reference,
            ReturnsInventoryRestock item,
            string actor,
            CancellationToken token) => Task.FromResult(new ReturnsPortResult(true, []));

        public Task<ReturnsPortResult> DisposeAsync(
            string reference,
            ReturnsInventoryDisposal item,
            string actor,
            CancellationToken token) => Task.FromResult(new ReturnsPortResult(true, []));
    }

    private sealed class PaymentPort : IReturnsPaymentPort
    {
        public int Refunds { get; private set; }
        public decimal? PaidAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal? LastRefundAmount { get; private set; }

        public Task<ReturnsPaymentState> GetStateAsync(
            Guid id,
            string? bill,
            decimal amount,
            CancellationToken token) => Task.FromResult(new ReturnsPaymentState(true, PaidAmount ?? amount, RefundedAmount, "PAY"));

        public Task<string> RequestRefundAsync(
            Guid salesOrderId,
            string source,
            decimal amount,
            string reason,
            string actor,
            CancellationToken token)
        {
            Refunds++;
            LastRefundAmount = amount;
            return Task.FromResult("REFUND-1");
        }
    }
}
