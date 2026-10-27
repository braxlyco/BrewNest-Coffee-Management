using BrewNest.Contracts.Procurement;
using BrewNest.Modules.Inventory.Application.Interfaces;
using BrewNest.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.UnitTests;

/// <summary>Protects cross-module retry paths from posting stock twice.</summary>
public sealed class ProcurementInventoryIdempotencyTests
{
    /// <summary>Identical Procurement references are acknowledged without a second movement.</summary>
    [Fact]
    public async Task ReceiptAndVendorReturnRetriesDoNotDuplicateStockMovement()
    {
        await using var database = new InventoryDbContext(new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var port = new ProcurementInventoryPort(database, TimeProvider.System);
        var itemId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var received = new ProcurementInventoryReceipt(itemId, warehouseId, "QA-LOT", null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)), 2m);
        Assert.True((await port.ReceiveAsync("GRN-QA-1", [received], "qa", default)).Succeeded);
        Assert.True((await port.ReceiveAsync("GRN-QA-1", [received], "qa", default)).Succeeded);
        Assert.Equal(2m, Assert.Single(await database.StockLots.ToArrayAsync()).OnHandQuantity);
        Assert.Single(await database.Movements.Where(x => x.ReferenceNumber == "GRN-QA-1").ToArrayAsync());

        var returned = new ProcurementInventoryReturn(itemId, warehouseId, "QA-LOT", 0.5m);
        Assert.True((await port.ReturnAsync("VRN-QA-1", [returned], "qa", default)).Succeeded);
        Assert.True((await port.ReturnAsync("VRN-QA-1", [returned], "qa", default)).Succeeded);
        Assert.Equal(1.5m, Assert.Single(await database.StockLots.ToArrayAsync()).OnHandQuantity);
        Assert.Single(await database.Movements.Where(x => x.ReferenceNumber == "VRN-QA-1").ToArrayAsync());
    }

    /// <summary>A reused reference with a different quantity is rejected, not silently accepted.</summary>
    [Fact]
    public async Task DifferentQuantitiesForSameReferenceConflict()
    {
        await using var database = new InventoryDbContext(new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var port = new ProcurementInventoryPort(database, TimeProvider.System);
        var itemId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var expiry = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6));
        Assert.True((await port.ReceiveAsync("GRN-QA-2",
            [new(itemId, warehouseId, "QA-LOT", null, expiry, 2m)], "qa", default)).Succeeded);
        Assert.False((await port.ReceiveAsync("GRN-QA-2",
            [new(itemId, warehouseId, "QA-LOT", null, expiry, 3m)], "qa", default)).Succeeded);
        Assert.Equal(2m, Assert.Single(await database.StockLots.ToArrayAsync()).OnHandQuantity);
    }
}
