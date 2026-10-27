using BrewNest.Contracts.Procurement;
using BrewNest.Modules.Inventory.Domain.Entities;
using BrewNest.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace BrewNest.Modules.Inventory.Application.Interfaces;
internal sealed class ProcurementInventoryPort(InventoryDbContext database,TimeProvider clock):IProcurementInventoryPort
{
    public async Task<ProcurementInventoryResult> ReceiveAsync(string reference,ProcurementInventoryReceipt[] lines,string actor,CancellationToken token)
    {
        if(lines.Length==0||lines.Any(x=>x.ItemId==Guid.Empty||x.WarehouseId==Guid.Empty||string.IsNullOrWhiteSpace(x.LotNumber)||x.Quantity<=0||x.ManufacturingDate>x.ExpiryDate))return Failure("Every receipt line needs valid item, warehouse, lot, dates, and positive quantity.");
        var previous = await PreviouslyPostedAsync(reference, StockMovementType.GoodsReceipt,
            lines.Select(x => (x.ItemId, x.WarehouseId, x.LotNumber.Trim(), x.Quantity)).ToArray(), token);
        if (previous is not null) return previous;
        foreach(var line in lines){var lot=await database.StockLots.SingleOrDefaultAsync(x=>x.ItemId==line.ItemId&&x.WarehouseId==line.WarehouseId&&x.LotNumber==line.LotNumber,token);if(lot is null){lot=new(){Id=Guid.NewGuid(),ItemId=line.ItemId,WarehouseId=line.WarehouseId,LotNumber=line.LotNumber.Trim(),ManufacturingDate=line.ManufacturingDate,ExpiryDate=line.ExpiryDate,CreatedAt=clock.GetUtcNow(),CreatedBy=actor};database.StockLots.Add(lot);}else if(lot.IsBlocked)return Failure($"Lot {line.LotNumber} is blocked.");lot.OnHandQuantity+=line.Quantity;lot.ModifiedAt=clock.GetUtcNow();lot.ModifiedBy=actor;database.Movements.Add(Movement(lot,StockMovementType.GoodsReceipt,line.Quantity,reference,actor,"Accepted procurement receipt."));}
        try{await database.SaveChangesAsync(token);return new(true,[]);}catch(DbUpdateException){return Failure("The Inventory receipt conflicts with current stock data.");}
    }
    public async Task<ProcurementInventoryResult> ReturnAsync(string reference,ProcurementInventoryReturn[] lines,string actor,CancellationToken token)
    {
        if(lines.Length==0||lines.Any(x=>x.Quantity<=0))return Failure("Vendor-return quantities must be positive.");
        var previous = await PreviouslyPostedAsync(reference, StockMovementType.VendorReturn,
            lines.Select(x => (x.ItemId, x.WarehouseId, x.LotNumber.Trim(), x.Quantity)).ToArray(), token);
        if (previous is not null) return previous;
        foreach(var line in lines){var lot=await database.StockLots.SingleOrDefaultAsync(x=>x.ItemId==line.ItemId&&x.WarehouseId==line.WarehouseId&&x.LotNumber==line.LotNumber,token);if(lot is null)return Failure($"Lot {line.LotNumber} was not found.");if(lot.OnHandQuantity-lot.ReservedQuantity<line.Quantity)return Failure($"Lot {line.LotNumber} has insufficient available stock.");lot.OnHandQuantity-=line.Quantity;lot.ModifiedAt=clock.GetUtcNow();lot.ModifiedBy=actor;database.Movements.Add(Movement(lot,StockMovementType.VendorReturn,-line.Quantity,reference,actor,"Returned to vendor."));}
        try{await database.SaveChangesAsync(token);return new(true,[]);}catch(DbUpdateException){return Failure("The vendor return conflicts with current stock data.");}
    }
    private StockMovement Movement(StockLot lot,StockMovementType type,decimal quantity,string reference,string actor,string remarks)=>new(){Id=Guid.NewGuid(),StockLotId=lot.Id,ItemId=lot.ItemId,WarehouseId=lot.WarehouseId,MovementType=type,Quantity=quantity,OnHandAfter=lot.OnHandQuantity,ReservedAfter=lot.ReservedQuantity,ReferenceType="Procurement",ReferenceNumber=reference,Remarks=remarks,PerformedBy=actor,PerformedAt=clock.GetUtcNow()};
    private async Task<ProcurementInventoryResult?> PreviouslyPostedAsync(
        string reference, StockMovementType type,
        (Guid ItemId, Guid WarehouseId, string LotNumber, decimal Quantity)[] expectedLines,
        CancellationToken token)
    {
        var posted = await database.Movements.AsNoTracking()
            .Where(x => x.ReferenceType == "Procurement" && x.ReferenceNumber == reference && x.MovementType == type)
            .ToArrayAsync(token);
        if (posted.Length == 0) return null;

        var lotIds = posted.Select(x => x.StockLotId).Distinct().ToArray();
        var lots = await database.StockLots.AsNoTracking().Where(x => lotIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, token);
        if (lots.Count != lotIds.Length) return Failure("The prior Inventory posting cannot be verified.");

        var expected = expectedLines.GroupBy(x => (x.ItemId, x.WarehouseId, x.LotNumber))
            .ToDictionary(x => x.Key, x => x.Sum(line => line.Quantity));
        var actual = posted.GroupBy(x => (x.ItemId, x.WarehouseId, lots[x.StockLotId].LotNumber))
            .ToDictionary(x => x.Key, x => x.Sum(movement => decimal.Abs(movement.Quantity)));
        return expected.Count == actual.Count && expected.All(x =>
            actual.TryGetValue(x.Key, out var quantity) && quantity == x.Value)
            ? new ProcurementInventoryResult(true, [])
            : Failure("The procurement reference was already posted with different Inventory quantities.");
    }
    private static ProcurementInventoryResult Failure(params string[] errors)=>new(false,errors);
}
