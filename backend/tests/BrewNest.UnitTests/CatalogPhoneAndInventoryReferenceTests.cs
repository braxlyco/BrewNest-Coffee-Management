using BrewNest.Contracts.Catalog;
using BrewNest.Modules.Catalog.Application.Services;
using BrewNest.Modules.Catalog.Infrastructure.Integrations;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.UnitTests;

/// <summary>Verifies contact phone rules and Catalog-owned Inventory references.</summary>
public sealed class CatalogPhoneAndInventoryReferenceTests
{
    private static readonly CatalogRequestContext Actor = new("validation-test");

    /// <summary>Rejects numbers without a valid international prefix and digit count.</summary>
    [Theory]
    [InlineData("03001234567")]
    [InlineData("+023001234567")]
    [InlineData("+92300-1234567")]
    [InlineData("+1234567")]
    [InlineData("+1234567890123456")]
    public async Task NewContactsRejectPhonesWithoutValidCountryCodeAndDigitCount(string phone)
    {
        await using var database = CreateContext();
        await database.Database.EnsureCreatedAsync();
        var service = new CatalogService(database, TimeProvider.System);

        var customer = await service.CreateCustomerAsync(
            new CreateCustomerRequest("CUST", "Customer", phone, "", ""), Actor, CancellationToken.None);
        var vendor = await service.CreateVendorAsync(
            new CreateVendorRequest("VEND", "Vendor", "Contact", phone, "", "", ""), Actor, CancellationToken.None);
        var rider = await service.CreateRiderAsync(
            new CreateRiderRequest("RIDE", "Rider", phone, "CAR-1", true), Actor, CancellationToken.None);

        Assert.Equal(CatalogResultStatus.Validation, customer.Status);
        Assert.Equal(CatalogResultStatus.Validation, vendor.Status);
        Assert.Equal(CatalogResultStatus.Validation, rider.Status);
        Assert.Contains(customer.Errors, error => error.Contains("country code", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Accepts valid phones and rejects empty unit metadata or unknown item types.</summary>
    [Fact]
    public async Task ValidInternationalPhoneAndUnitFieldsAreAccepted()
    {
        await using var database = CreateContext();
        await database.Database.EnsureCreatedAsync();
        var service = new CatalogService(database, TimeProvider.System);

        var invalidUnit = await service.CreateUnitAsync(new("EA", "Each", "", "", 0), Actor, CancellationToken.None);
        var validCustomer = await service.CreateCustomerAsync(
            new CreateCustomerRequest("CUST", "Customer", "+923001234567", "", ""), Actor, CancellationToken.None);
        var invalidItemType = await service.CreateItemAsync(
            new("ITEM", "Item", "", "42", Guid.NewGuid(), null, null, 0m, null, false),
            Actor, CancellationToken.None);

        Assert.Equal(CatalogResultStatus.Validation, invalidUnit.Status);
        Assert.True(validCustomer.Succeeded);
        Assert.Equal(CatalogResultStatus.Validation, invalidItemType.Status);
    }

    /// <summary>Only active Catalog items in active warehouses may be received into Inventory.</summary>
    [Fact]
    public async Task InventoryReferencesRequireActiveMaterialAndWarehouse()
    {
        await using var database = CreateContext();
        await database.Database.EnsureCreatedAsync();
        var service = new CatalogService(database, TimeProvider.System);
        var unit = await service.CreateUnitAsync(new("EA", "Each", "ea", "Count", 0), Actor, CancellationToken.None);
        var warehouse = await service.CreateWarehouseAsync(new("WH", "Warehouse", ""), Actor, CancellationToken.None);
        var material = await service.CreateItemAsync(
            new("BEANS", "Beans", "", "RawMaterial", unit.Value!.Id, null, null, 50m, null, false),
            Actor, CancellationToken.None);
        var menuItem = await service.CreateItemAsync(
            new("COFFEE", "Coffee", "", "MenuItem", unit.Value.Id, null, 200m, 50m, null, false),
            Actor, CancellationToken.None);
        var validator = new CatalogInventoryReferenceValidator(database);

        Assert.True(await validator.AreValidAsync(material.Value!.Id, warehouse.Value!.Id, CancellationToken.None));
        Assert.True(await validator.AreValidAsync(menuItem.Value!.Id, warehouse.Value.Id, CancellationToken.None));
        Assert.False(await validator.AreValidAsync(Guid.NewGuid(), warehouse.Value.Id, CancellationToken.None));
        Assert.False(await validator.AreValidAsync(material.Value.Id, Guid.NewGuid(), CancellationToken.None));

        await service.UpdateWarehouseAsync(warehouse.Value.Id,
            new(warehouse.Value.Code, warehouse.Value.Name, warehouse.Value.Address, false, warehouse.Value.Version, "closed"),
            Actor, CancellationToken.None);
        Assert.False(await validator.AreValidAsync(material.Value.Id, warehouse.Value.Id, CancellationToken.None));
    }

    private static CatalogDbContext CreateContext() => new(
        new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"catalog-reference-{Guid.NewGuid():N}").Options);
}
