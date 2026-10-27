using BrewNest.Contracts.Catalog;
using BrewNest.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BrewNest.Modules.Catalog.Infrastructure.Persistence.Seed;

internal sealed class CatalogDevelopmentSeeder(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    IConfiguration configuration,
    TimeProvider timeProvider) : IHostedService
{
    private const string Actor = "development-demo-seed";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment() || !configuration.GetValue("DemoData:Enabled", false))
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        if (!database.Database.IsRelational()) return;
        await database.Database.MigrateAsync(cancellationToken);
        var now = timeProvider.GetUtcNow();

        await AddIfMissing(database.UnitsOfMeasure, new UnitOfMeasure { Id=DemoCatalogIds.EachUnit,Code="DEMO-EA",Name="Each",Symbol="ea",Category="Count",DecimalPlaces=0,CreatedAt=now,CreatedBy=Actor }, cancellationToken);
        await AddIfMissing(database.UnitsOfMeasure, new UnitOfMeasure { Id=DemoCatalogIds.KilogramUnit,Code="DEMO-KG",Name="Kilogram",Symbol="kg",Category="Mass",DecimalPlaces=3,CreatedAt=now,CreatedBy=Actor }, cancellationToken);
        await AddIfMissing(database.UnitsOfMeasure, new UnitOfMeasure { Id=DemoCatalogIds.LitreUnit,Code="DEMO-L",Name="Litre",Symbol="L",Category="Volume",DecimalPlaces=3,CreatedAt=now,CreatedBy=Actor }, cancellationToken);

        await AddIfMissing(database.ItemGroups, new ItemGroup { Id=DemoCatalogIds.MenuGroup,Code="DEMO-MENU",Name="Coffee Menu",AppliesTo=CatalogItemType.MenuItem,CreatedAt=now,CreatedBy=Actor }, cancellationToken);
        await AddIfMissing(database.ItemGroups, new ItemGroup { Id=DemoCatalogIds.RawMaterialGroup,Code="DEMO-RAW",Name="Beverage Ingredients",AppliesTo=CatalogItemType.RawMaterial,RequiresInspection=true,CreatedAt=now,CreatedBy=Actor }, cancellationToken);
        await AddIfMissing(database.ItemGroups, new ItemGroup { Id=DemoCatalogIds.PackagingGroup,Code="DEMO-PACK",Name="Takeaway Packaging",AppliesTo=CatalogItemType.PackagingMaterial,CreatedAt=now,CreatedBy=Actor }, cancellationToken);

        await AddIfMissing(database.Items, Item(DemoCatalogIds.EspressoItem,"DEMO-ESP","Espresso",CatalogItemType.MenuItem,DemoCatalogIds.EachUnit,DemoCatalogIds.MenuGroup,350m,95m,null,false,now), cancellationToken);
        await AddIfMissing(database.Items, Item(DemoCatalogIds.CappuccinoItem,"DEMO-CAP","Cappuccino",CatalogItemType.MenuItem,DemoCatalogIds.EachUnit,DemoCatalogIds.MenuGroup,450m,145m,null,false,now), cancellationToken);
        await AddIfMissing(database.Items, Item(DemoCatalogIds.CoffeeBeansItem,"DEMO-BEAN","Arabica Coffee Beans",CatalogItemType.RawMaterial,DemoCatalogIds.KilogramUnit,DemoCatalogIds.RawMaterialGroup,null,4200m,180,true,now), cancellationToken);
        await AddIfMissing(database.Items, Item(DemoCatalogIds.MilkItem,"DEMO-MILK","Fresh Milk",CatalogItemType.RawMaterial,DemoCatalogIds.LitreUnit,DemoCatalogIds.RawMaterialGroup,null,310m,7,true,now), cancellationToken);
        await AddIfMissing(database.Items, Item(DemoCatalogIds.CupItem,"DEMO-CUP","12 oz Paper Cup",CatalogItemType.PackagingMaterial,DemoCatalogIds.EachUnit,DemoCatalogIds.PackagingGroup,null,18m,null,false,now), cancellationToken);
        await AddIfMissing(database.Items, Item(DemoCatalogIds.LidItem,"DEMO-LID","12 oz Cup Lid",CatalogItemType.PackagingMaterial,DemoCatalogIds.EachUnit,DemoCatalogIds.PackagingGroup,null,8m,null,false,now), cancellationToken);

        await AddIfMissing(database.Customers, new Customer { Id=Guid.Parse("d5000000-0000-0000-0000-000000000001"),Code="DEMO-WALKIN",Name="Walk-in Customer",Phone="+923000000000",Email="walkin@demo.local",Address="BrewNest café",CreatedAt=now,CreatedBy=Actor }, cancellationToken);
        await AddIfMissing(database.Vendors, new Vendor { Id=Guid.Parse("d5000000-0000-0000-0000-000000000002"),Code="DEMO-SUPPLIER",Name="Pakistan Café Supplies",ContactPerson="Demo Supplier",Phone="+923010000000",Email="supplier@demo.local",Address="Lahore",TaxRegistrationNumber="DEMO-NTN-001",CreatedAt=now,CreatedBy=Actor }, cancellationToken);
        await AddIfMissing(database.DiningTables, new DiningTable { Id=Guid.Parse("d6000000-0000-0000-0000-000000000001"),Code="DEMO-T01",Name="Window Table",Capacity=4,Area="Main Hall",CreatedAt=now,CreatedBy=Actor }, cancellationToken);
        await AddIfMissing(database.DiningTables, new DiningTable { Id=Guid.Parse("d6000000-0000-0000-0000-000000000002"),Code="DEMO-T02",Name="Garden Table",Capacity=2,Area="Patio",CreatedAt=now,CreatedBy=Actor }, cancellationToken);
        await AddIfMissing(database.Warehouses, new Warehouse { Id=DemoCatalogIds.MainWarehouse,Code="DEMO-MAIN",Name="Main Store",Address="BrewNest main branch",CreatedAt=now,CreatedBy=Actor }, cancellationToken);
        await AddIfMissing(database.Riders, new Rider { Id=Guid.Parse("d7000000-0000-0000-0000-000000000001"),Code="DEMO-R01",Name="Ali Rider",Phone="+923020000001",VehicleRegistration="LEA-DEMO-1",IsAvailable=true,CreatedAt=now,CreatedBy=Actor }, cancellationToken);
        await AddIfMissing(database.Riders, new Rider { Id=Guid.Parse("d7000000-0000-0000-0000-000000000002"),Code="DEMO-R02",Name="Usman Rider",Phone="+923020000002",VehicleRegistration="LEA-DEMO-2",IsAvailable=true,CreatedAt=now,CreatedBy=Actor }, cancellationToken);

        if (!await database.Recipes.AnyAsync(x=>x.Id==Guid.Parse("d8000000-0000-0000-0000-000000000001"),cancellationToken))
        {
            database.Recipes.Add(new Recipe { Id=Guid.Parse("d8000000-0000-0000-0000-000000000001"),Code="DEMO-REC-ESP",Name="Espresso BOM",MenuItemId=DemoCatalogIds.EspressoItem,YieldQuantity=1m,YieldUnitOfMeasureId=DemoCatalogIds.EachUnit,CreatedAt=now,CreatedBy=Actor,Lines=[new RecipeLine { Id=Guid.Parse("d8100000-0000-0000-0000-000000000001"),IngredientItemId=DemoCatalogIds.CoffeeBeansItem,Quantity=0.018m,WastagePercentage=2m,Sequence=1,AddedAt=now,AddedBy=Actor }] });
        }
        if (!await database.Recipes.AnyAsync(x=>x.Id==Guid.Parse("d8000000-0000-0000-0000-000000000002"),cancellationToken))
        {
            database.Recipes.Add(new Recipe { Id=Guid.Parse("d8000000-0000-0000-0000-000000000002"),Code="DEMO-REC-CAP",Name="Cappuccino BOM",MenuItemId=DemoCatalogIds.CappuccinoItem,YieldQuantity=1m,YieldUnitOfMeasureId=DemoCatalogIds.EachUnit,CreatedAt=now,CreatedBy=Actor,Lines=[new RecipeLine { Id=Guid.Parse("d8100000-0000-0000-0000-000000000002"),IngredientItemId=DemoCatalogIds.CoffeeBeansItem,Quantity=0.018m,WastagePercentage=2m,Sequence=1,AddedAt=now,AddedBy=Actor },new RecipeLine { Id=Guid.Parse("d8100000-0000-0000-0000-000000000003"),IngredientItemId=DemoCatalogIds.MilkItem,Quantity=0.180m,WastagePercentage=3m,Sequence=2,AddedAt=now,AddedBy=Actor }] });
        }
        if (!await database.InspectionRequirements.AnyAsync(x=>x.Id==Guid.Parse("d9000000-0000-0000-0000-000000000001"),cancellationToken))
        {
            database.InspectionRequirements.Add(new InspectionRequirement { Id=Guid.Parse("d9000000-0000-0000-0000-000000000001"),ItemId=DemoCatalogIds.CoffeeBeansItem,Name="Coffee bean receiving inspection",IsRequired=true,CreatedAt=now,CreatedBy=Actor,Criteria=[new InspectionCriterion { Id=Guid.Parse("d9100000-0000-0000-0000-000000000001"),Name="Packaging sealed",ResultType=InspectionResultType.PassFail,IsMandatory=true,Sequence=1,AddedAt=now,AddedBy=Actor },new InspectionCriterion { Id=Guid.Parse("d9100000-0000-0000-0000-000000000002"),Name="Aroma and appearance",ResultType=InspectionResultType.Text,IsMandatory=true,Sequence=2,AddedAt=now,AddedBy=Actor }] });
        }

        await database.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)=>Task.CompletedTask;

    private static CatalogItem Item(Guid id,string code,string name,CatalogItemType type,Guid unitId,Guid groupId,decimal? price,decimal cost,int? shelfLife,bool inspection,DateTimeOffset now)=>new() { Id=id,Code=code,Name=name,Description=$"Development demo {name.ToLowerInvariant()}.",ItemType=type,UnitOfMeasureId=unitId,ItemGroupId=groupId,SalesPrice=price,StandardCost=cost,ShelfLifeDays=shelfLife,RequiresInspection=inspection,CreatedAt=now,CreatedBy=Actor };

    private static async Task AddIfMissing<T>(DbSet<T> set,T value,CancellationToken cancellationToken) where T:AuditedMasterData
    {
        if (!await set.AnyAsync(x=>x.Id==value.Id,cancellationToken)) set.Add(value);
    }
}
