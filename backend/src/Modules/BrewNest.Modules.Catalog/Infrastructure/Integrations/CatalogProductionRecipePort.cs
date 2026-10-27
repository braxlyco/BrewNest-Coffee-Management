using BrewNest.Contracts.Production;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Catalog.Infrastructure.Integrations;

internal sealed class CatalogProductionRecipePort(CatalogDbContext database) : IProductionRecipePort
{
    public Task<ProductionMenuItemReference[]> GetMenuItemsAsync(CancellationToken cancellationToken) =>
        database.Recipes.AsNoTracking().Where(x => x.IsActive && x.MenuItem.IsActive)
            .OrderBy(x => x.MenuItem.Code).Select(x => new ProductionMenuItemReference(x.MenuItemId, x.MenuItem.Code, x.MenuItem.Name)).ToArrayAsync(cancellationToken);

    public Task<ProductionCatalogItemReference[]> GetItemNamesAsync(CancellationToken cancellationToken) =>
        database.Items.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new ProductionCatalogItemReference(x.Id, x.Name)).ToArrayAsync(cancellationToken);

    public Task<ProductionRecipeSnapshot?> GetRecipeAsync(Guid menuItemId, CancellationToken cancellationToken) =>
        database.Recipes.AsNoTracking().Where(x => x.MenuItemId == menuItemId && x.IsActive)
            .Select(x => new ProductionRecipeSnapshot(x.MenuItemId, x.YieldQuantity,
                x.Lines.Where(line => line.RevokedAt == null).OrderBy(line => line.Sequence)
                    .Select(line => new ProductionRecipeIngredient(line.IngredientItemId, line.Quantity, line.WastagePercentage)).ToArray()))
            .SingleOrDefaultAsync(cancellationToken);
}
