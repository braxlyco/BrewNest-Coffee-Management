using BrewNest.Modules.Catalog.Application.Services;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.UnitTests;

/// <summary>Verifies Catalog application rules without HTTP or SQL Server.</summary>
public sealed class CatalogValidationTests
{
    private static readonly CatalogRequestContext Context=new("unit-test");

    /// <summary>Confirms codes are normalized and duplicate unit codes are rejected.</summary>
    [Fact]
    public async Task UnitCodesAreNormalizedAndUnique()
    {
        await using var db=CreateContext();await db.Database.EnsureCreatedAsync();var service=new CatalogService(db,TimeProvider.System);
        var first=await service.CreateUnitAsync(new(" kg ","Kilogram","kg","Mass",3),Context,CancellationToken.None);
        var duplicate=await service.CreateUnitAsync(new("KG","Other","x","Mass",2),Context,CancellationToken.None);
        Assert.True(first.Succeeded);Assert.Equal("KG",first.Value?.Code);Assert.Equal(CatalogResultStatus.Conflict,duplicate.Status);
    }

    /// <summary>Confirms money and type rules distinguish sellable menu items from materials.</summary>
    [Theory]
    [InlineData("MenuItem",-1.0)]
    [InlineData("RawMaterial",100.0)]
    [InlineData("PackagingMaterial",10.0)]
    public async Task ItemTypeControlsSalesPrice(string itemType,double price)
    {
        await using var db=CreateContext();await db.Database.EnsureCreatedAsync();var service=new CatalogService(db,TimeProvider.System);
        var result=await service.CreateItemAsync(new("ITEM","Item","",itemType,Guid.NewGuid(),null,price<0?null:(decimal)price,1,null,false),Context,CancellationToken.None);
        Assert.Equal(CatalogResultStatus.Validation,result.Status);
    }

    /// <summary>Confirms an inspection rule targets exactly one item or group and has criteria.</summary>
    [Fact]
    public async Task InspectionRequirementNeedsExactlyOneTargetAndCriterion()
    {
        await using var db=CreateContext();await db.Database.EnsureCreatedAsync();var service=new CatalogService(db,TimeProvider.System);
        var result=await service.CreateInspectionRequirementAsync(new(null,null,"Receiving",false,[]),Context,CancellationToken.None);
        Assert.Equal(CatalogResultStatus.Validation,result.Status);Assert.Contains(result.Errors,x=>x.Contains("exactly one",StringComparison.Ordinal));Assert.Contains(result.Errors,x=>x.Contains("criterion",StringComparison.Ordinal));
    }

    private static CatalogDbContext CreateContext()=>new(new DbContextOptionsBuilder<CatalogDbContext>().UseInMemoryDatabase($"catalog-unit-{Guid.NewGuid():N}").Options);
}
