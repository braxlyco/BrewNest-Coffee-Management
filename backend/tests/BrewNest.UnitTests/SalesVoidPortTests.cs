using BrewNest.Modules.Sales.Application.Interfaces;
using BrewNest.Modules.Sales.Domain.Entities;
using BrewNest.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.UnitTests;

/// <summary>Checks Sales state transitions requested by approved Returns voids.</summary>
public sealed class SalesVoidPortTests
{
    /// <summary>A paid void keeps the bill, records history, and can be retried safely.</summary>
    [Fact]
    public async Task PaidVoidPreservesBillAndIsIdempotent()
    {
        await using var database = Database();
        var order = Seed(database, OrderStatus.Closed, true);
        await database.SaveChangesAsync();
        var port = new ReturnsSalesPort(database, TimeProvider.System);

        Assert.True((await port.VoidPaidAsync(order.Id, "Approved refund", "finance", default)).Succeeded);
        Assert.True((await port.VoidPaidAsync(order.Id, "Approved refund", "finance", default)).Succeeded);

        var saved = await database.Orders.Include(x => x.Bill).SingleAsync();
        Assert.Equal(OrderStatus.Cancelled, saved.Status);
        Assert.NotNull(saved.Bill);
        Assert.Single(await database.StatusHistory.ToArrayAsync());
        Assert.Equal("Closed", (await database.StatusHistory.SingleAsync()).PreviousStatus);
    }

    /// <summary>An unpaid void can cancel a billed order after Returns verifies no payment.</summary>
    [Fact]
    public async Task UnpaidVoidCanCancelBilledOrderWithoutDeletingBill()
    {
        await using var database = Database();
        var order = Seed(database, OrderStatus.Billed, true);
        await database.SaveChangesAsync();
        var port = new ReturnsSalesPort(database, TimeProvider.System);

        Assert.True((await port.VoidUnpaidAsync(order.Id, "Unpaid bill void", "sales manager", default)).Succeeded);
        Assert.Equal(OrderStatus.Cancelled, (await database.Orders.SingleAsync()).Status);
        Assert.Single(await database.Bills.ToArrayAsync());
    }

    /// <summary>The paid path cannot be used before a bill exists.</summary>
    [Fact]
    public async Task PaidVoidRejectsUnbilledOrder()
    {
        await using var database = Database();
        var order = Seed(database, OrderStatus.Confirmed, false);
        await database.SaveChangesAsync();
        var result = await new ReturnsSalesPort(database, TimeProvider.System).VoidPaidAsync(order.Id, "Invalid paid void", "finance", default);
        Assert.False(result.Succeeded);
        Assert.Equal(OrderStatus.Confirmed, (await database.Orders.SingleAsync()).Status);
    }

    private static SalesDbContext Database() => new(new DbContextOptionsBuilder<SalesDbContext>()
        .UseInMemoryDatabase($"sales-void-{Guid.NewGuid():N}").Options);

    private static SalesOrder Seed(SalesDbContext database, OrderStatus status, bool billed)
    {
        var order = new SalesOrder
        {
            Id = Guid.NewGuid(), OrderNumber = "SO-VOID-TEST", OrderType = OrderType.Takeaway,
            Status = status, CustomerName = "Guest", CreatedAt = DateTimeOffset.UtcNow, CreatedBy = "tester"
        };
        if (billed) order.Bill = new SalesBill
        {
            Id = Guid.NewGuid(), OrderId = order.Id, Order = order, BillNumber = "BILL-VOID-TEST",
            GrossAmount = 100m, NetAmount = 100m, IssuedAt = DateTimeOffset.UtcNow, IssuedBy = "tester"
        };
        database.Orders.Add(order);
        return order;
    }
}
