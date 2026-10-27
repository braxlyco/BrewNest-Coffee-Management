using BrewNest.Contracts.Catalog;
using BrewNest.Modules.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BrewNest.Modules.Inventory.Infrastructure.Persistence.Seed;

internal sealed class InventoryDevelopmentSeeder(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    IConfiguration configuration,
    TimeProvider timeProvider) : IHostedService
{
    private const string Actor = "development-demo-seed";
    private static readonly Guid BeansLotId=Guid.Parse("e1000000-0000-0000-0000-000000000001");
    private static readonly Guid MilkLotId=Guid.Parse("e1000000-0000-0000-0000-000000000002");
    private static readonly Guid CupsLotId=Guid.Parse("e1000000-0000-0000-0000-000000000003");
    private static readonly Guid LidsLotId=Guid.Parse("e1000000-0000-0000-0000-000000000004");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment() || !configuration.GetValue("DemoData:Enabled", false))
        {
            return;
        }

        await using var scope=scopeFactory.CreateAsyncScope();
        var database=scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        if (!database.Database.IsRelational()) return;
        await database.Database.MigrateAsync(cancellationToken);
        var now=timeProvider.GetUtcNow();
        var today=DateOnly.FromDateTime(now.UtcDateTime);

        if (!await database.StockLots.AnyAsync(x=>x.Id==BeansLotId,cancellationToken))
        {
            database.StockLots.Add(Lot(BeansLotId,DemoCatalogIds.CoffeeBeansItem,"DEMO-BEANS-001",today.AddDays(-30),today.AddDays(150),25m,2m,now));
            database.Reservations.Add(new StockReservation { Id=Guid.Parse("e2000000-0000-0000-0000-000000000001"),StockLotId=BeansLotId,ItemId=DemoCatalogIds.CoffeeBeansItem,WarehouseId=DemoCatalogIds.MainWarehouse,Quantity=2m,ReferenceType="DemoKitchen",ReferenceId="DEMO-KOT-001",Status=ReservationStatus.Active,Remarks="Sample reservation showing available stock calculation.",CreatedAt=now,CreatedBy=Actor });
            database.Movements.Add(Movement(Guid.Parse("e3000000-0000-0000-0000-000000000001"),BeansLotId,DemoCatalogIds.CoffeeBeansItem,StockMovementType.GoodsReceipt,25m,25m,0m,"DemoReceipt","DEMO-GR-001","Initial coffee bean stock.",now));
            database.Movements.Add(Movement(Guid.Parse("e3000000-0000-0000-0000-000000000002"),BeansLotId,DemoCatalogIds.CoffeeBeansItem,StockMovementType.Reservation,2m,25m,2m,"DemoKitchen","DEMO-KOT-001","Sample kitchen reservation.",now.AddMilliseconds(1)));
        }
        await AddLotAsync(database,MilkLotId,DemoCatalogIds.MilkItem,"DEMO-MILK-001",today.AddDays(-1),today.AddDays(3),40m,Guid.Parse("e3000000-0000-0000-0000-000000000003"),"DEMO-GR-002","Critical-expiry milk stock.",now,cancellationToken);
        await AddLotAsync(database,CupsLotId,DemoCatalogIds.CupItem,"DEMO-CUPS-001",null,null,500m,Guid.Parse("e3000000-0000-0000-0000-000000000004"),"DEMO-GR-003","Initial paper cup stock.",now,cancellationToken);
        await AddLotAsync(database,LidsLotId,DemoCatalogIds.LidItem,"DEMO-LIDS-001",null,null,500m,Guid.Parse("e3000000-0000-0000-0000-000000000005"),"DEMO-GR-004","Initial cup lid stock.",now,cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)=>Task.CompletedTask;

    private static async Task AddLotAsync(InventoryDbContext database,Guid lotId,Guid itemId,string lotNumber,DateOnly? manufactured,DateOnly? expiry,decimal quantity,Guid movementId,string reference,string remarks,DateTimeOffset now,CancellationToken cancellationToken)
    {
        if (await database.StockLots.AnyAsync(x=>x.Id==lotId,cancellationToken)) return;
        database.StockLots.Add(Lot(lotId,itemId,lotNumber,manufactured,expiry,quantity,0m,now));
        database.Movements.Add(Movement(movementId,lotId,itemId,StockMovementType.GoodsReceipt,quantity,quantity,0m,"DemoReceipt",reference,remarks,now));
    }

    private static StockLot Lot(Guid id,Guid itemId,string number,DateOnly? manufactured,DateOnly? expiry,decimal onHand,decimal reserved,DateTimeOffset now)=>new() { Id=id,ItemId=itemId,WarehouseId=DemoCatalogIds.MainWarehouse,LotNumber=number,ManufacturingDate=manufactured,ExpiryDate=expiry,OnHandQuantity=onHand,ReservedQuantity=reserved,CreatedAt=now,CreatedBy=Actor };
    private static StockMovement Movement(Guid id,Guid lotId,Guid itemId,StockMovementType type,decimal quantity,decimal onHand,decimal reserved,string referenceType,string referenceNumber,string remarks,DateTimeOffset at)=>new() { Id=id,StockLotId=lotId,ItemId=itemId,WarehouseId=DemoCatalogIds.MainWarehouse,MovementType=type,Quantity=quantity,OnHandAfter=onHand,ReservedAfter=reserved,ReferenceType=referenceType,ReferenceNumber=referenceNumber,Remarks=remarks,PerformedBy=Actor,PerformedAt=at };
}
