using BrewNest.Contracts.Kitchen;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Catalog.Infrastructure.Integrations;

internal sealed class CatalogKitchenRecipePort(CatalogDbContext database) : IKitchenRecipePort
{
    public Task<KitchenRecipeSnapshot?> GetRecipeAsync(Guid menuItemId, CancellationToken cancellationToken) =>
        database.Recipes.AsNoTracking()
            .Where(recipe => recipe.MenuItemId == menuItemId && recipe.IsActive)
            .Select(recipe => new KitchenRecipeSnapshot(
                recipe.MenuItemId,
                recipe.YieldQuantity,
                recipe.Lines
                    .Where(line => line.RevokedAt == null)
                    .OrderBy(line => line.Sequence)
                    .Select(line => new KitchenRecipeIngredient(line.IngredientItemId, line.Quantity, line.WastagePercentage, line.IngredientItem.Name, line.IngredientItem.UnitOfMeasure.Symbol))
                    .ToArray()))
            .SingleOrDefaultAsync(cancellationToken);
}
