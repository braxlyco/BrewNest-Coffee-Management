using BrewNest.Contracts.Inventory;
using BrewNest.Contracts.Kitchen;
using BrewNest.Modules.Inventory.Application.Services;
using BrewNest.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.UnitTests;

/// <summary>Verifies Inventory invariants independently of HTTP and SQL Server.</summary>
public sealed class InventoryServiceTests
{
    /// <summary>Available stock is on-hand less reserved, and release restores availability.</summary>
    [Fact]
    public async Task ReservationAndReleaseMaintainAvailableQuantity()
    {
        await using var db=Database();var service=Service(db);var lot=Value(await service.ReceiveAsync(Receipt(10m),new("tester"),default));
        var reservation=Value(await service.ReserveForApiAsync(new(lot.Id,4m,"Kitchen","KOT-1",null,"test"),new("tester"),default));
        var reserved=Assert.Single(Value(await service.GetLotsAsync(null,null,default)));Assert.Equal(6m,reserved.AvailableQuantity);Assert.Equal(4m,reserved.ReservedQuantity);
        Value(await service.ReleaseForApiAsync(reservation.Id,new(4m,"cancelled",reservation.Version),new("tester"),default));
        var released=Assert.Single(Value(await service.GetLotsAsync(null,null,default)));Assert.Equal(10m,released.AvailableQuantity);Assert.Equal(0m,released.ReservedQuantity);
    }

    /// <summary>Issues cannot consume reserved stock unless the matching reservation is supplied.</summary>
    [Fact]
    public async Task DirectIssueCannotCreateNegativeAvailableStock()
    {
        await using var db=Database();var service=Service(db);var lot=Value(await service.ReceiveAsync(Receipt(5m),new("tester"),default));
        Value(await service.ReserveForApiAsync(new(lot.Id,4m,"Production","PLAN-1",null,""),new("tester"),default));
        lot=Assert.Single(Value(await service.GetLotsAsync(null,null,default)));var result=await service.IssueAsync(new(lot.Id,null,2m,"Manual","GI-1","",lot.Version,null),new("tester"),default);
        Assert.False(result.Succeeded);Assert.Contains("negative inventory",string.Join(' ',result.Errors),StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Adjustments may not reduce on-hand below the reserved quantity.</summary>
    [Fact]
    public async Task AdjustmentCannotReduceBelowReservations()
    {
        await using var db=Database();var service=Service(db);var lot=Value(await service.ReceiveAsync(Receipt(8m),new("tester"),default));
        Value(await service.ReserveForApiAsync(new(lot.Id,6m,"Sales","ORDER-1",null,""),new("tester"),default));lot=Assert.Single(Value(await service.GetLotsAsync(null,null,default)));
        var result=await service.AdjustAsync(new(lot.Id,-3m,"count correction","ADJ-1",lot.Version),new("tester"),default);Assert.False(result.Succeeded);
    }

    /// <summary>Expired stock is automatically blocked and cannot be reserved.</summary>
    [Fact]
    public async Task ExpiredStockIsBlockedFromReservation()
    {
        await using var db=Database();var service=Service(db);var request=Receipt(5m) with { ExpiryDate=new DateOnly(2026,9,22) };var lot=Value(await service.ReceiveAsync(request,new("tester"),default));
        var result=await service.ReserveForApiAsync(new(lot.Id,1m,"Kitchen","KOT-2",null,""),new("tester"),default);Assert.False(result.Succeeded);
        Assert.True(Assert.Single(Value(await service.GetLotsAsync(null,null,default))).IsBlocked);
    }

    /// <summary>Wastage cannot exceed unreserved physical stock.</summary>
    [Fact]
    public async Task WastageCannotConsumeReservedStock()
    {
        await using var db=Database();var service=Service(db);var lot=Value(await service.ReceiveAsync(Receipt(5m),new("tester"),default));Value(await service.ReserveForApiAsync(new(lot.Id,4m,"Sales","ORDER-2",null,""),new("tester"),default));lot=Assert.Single(Value(await service.GetLotsAsync(null,null,default)));
        var result=await service.WasteAsync(new(lot.Id,2m,"damage","W-1",lot.Version),new("tester"),default);Assert.False(result.Succeeded);
    }

    /// <summary>Kitchen staff see the ingredient and corrective action, never its database identifier.</summary>
    [Fact]
    public async Task KitchenShortageNamesIngredientAndDoesNotExposeIdentifier()
    {
        await using var db=Database();
        var service=Service(db);
        var ingredientId=Guid.NewGuid();
        var result=await service.ReserveAsync("KOT-LOW-STOCK",[new KitchenIngredientRequirement(ingredientId,2m,"Coffee beans","kg")],"chef",default);
        Assert.False(result.Succeeded);
        var error=Assert.Single(result.Errors);
        Assert.Contains("Coffee beans",error,StringComparison.Ordinal);
        Assert.Contains("2.000 kg",error,StringComparison.Ordinal);
        Assert.Contains("Inventory",error,StringComparison.Ordinal);
        Assert.DoesNotContain(ingredientId.ToString(),error,StringComparison.OrdinalIgnoreCase);
    }

    private static InventoryDbContext Database()=>new(new DbContextOptionsBuilder<InventoryDbContext>().UseInMemoryDatabase($"inventory-{Guid.NewGuid():N}").Options);
    private static InventoryService Service(InventoryDbContext db)=>new(db,new ValidCatalogReferences(),new FixedExpiryPolicy(),new FixedTimeProvider());
    private static GoodsReceiptRequest Receipt(decimal quantity)=>new(Guid.NewGuid(),Guid.NewGuid(),"LOT-001",new(2026,9,1),new(2026,10,1),quantity,"ManualReceipt","GR-1","");
    private static T Value<T>(InventoryResult<T> result){Assert.True(result.Succeeded,string.Join(' ',result.Errors));return Assert.IsType<T>(result.Value);}
    private sealed class ValidCatalogReferences:IInventoryCatalogReferenceValidator { public ValueTask<bool> AreValidAsync(Guid itemId,Guid warehouseId,CancellationToken cancellationToken)=>ValueTask.FromResult(itemId!=Guid.Empty&&warehouseId!=Guid.Empty); }
    private sealed class FixedExpiryPolicy:IInventoryExpiryPolicyProvider { public ValueTask<InventoryExpiryThresholds> GetAsync(CancellationToken cancellationToken)=>ValueTask.FromResult(new InventoryExpiryThresholds(15,7,3)); }
    private sealed class FixedTimeProvider:TimeProvider { public override DateTimeOffset GetUtcNow()=>new(2026,9,23,12,0,0,TimeSpan.Zero); }
}
