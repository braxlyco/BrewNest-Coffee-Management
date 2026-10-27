using BrewNest.Contracts.Inventory;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Catalog.Infrastructure.Integrations;

internal sealed class CatalogInventoryReferenceValidator(CatalogDbContext database) : IInventoryCatalogReferenceValidator
{
    public async ValueTask<bool> AreValidAsync(Guid itemId, Guid warehouseId, CancellationToken cancellationToken)
    {
        if (itemId == Guid.Empty || warehouseId == Guid.Empty) return false;

        var itemExists = await database.Items.AsNoTracking().AnyAsync(
            item => item.Id == itemId && item.IsActive,
            cancellationToken);
        if (!itemExists) return false;

        return await database.Warehouses.AsNoTracking().AnyAsync(
            warehouse => warehouse.Id == warehouseId && warehouse.IsActive,
            cancellationToken);
    }
}

internal sealed class CatalogInventoryReferenceLookup(CatalogDbContext database) : IInventoryCatalogReferenceLookup
{
    public async Task<InventoryCatalogReferenceData> GetAsync(CancellationToken cancellationToken)
    {
        var items = await database.Items.AsNoTracking()
            .OrderBy(item => item.Name)
            .Select(item => new InventoryCatalogItemReference(item.Id, item.Code, item.Name, item.ItemType.ToString(), item.IsActive))
            .ToArrayAsync(cancellationToken);
        var warehouses = await database.Warehouses.AsNoTracking()
            .OrderBy(warehouse => warehouse.Name)
            .Select(warehouse => new InventoryCatalogWarehouseReference(warehouse.Id, warehouse.Code, warehouse.Name, warehouse.IsActive))
            .ToArrayAsync(cancellationToken);
        return new(items, warehouses);
    }
}
