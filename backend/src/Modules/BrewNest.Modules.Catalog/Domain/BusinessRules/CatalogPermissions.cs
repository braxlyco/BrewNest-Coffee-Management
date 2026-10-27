using BrewNest.BuildingBlocks.Authorization;

namespace BrewNest.Modules.Catalog.Domain.BusinessRules;

internal static class CatalogPermissions
{
    public const string MasterDataRead = "catalog.masterdata.read";
    public const string MasterDataManage = "catalog.masterdata.manage";
    public const string RecipesRead = "catalog.recipes.read";
    public const string RecipesManage = "catalog.recipes.manage";
    public const string QualityRead = "catalog.quality.read";
    public const string QualityManage = "catalog.quality.manage";
}

internal sealed class CatalogPermissionDefinitionProvider : IPermissionDefinitionProvider
{
    public IReadOnlyCollection<PermissionDefinition> Permissions { get; } =
    [
        new(CatalogPermissions.MasterDataRead, "View Catalog master data.", "Catalog"),
        new(CatalogPermissions.MasterDataManage, "Create and update Catalog master data.", "Catalog"),
        new(CatalogPermissions.RecipesRead, "View recipes and bills of materials.", "Recipes"),
        new(CatalogPermissions.RecipesManage, "Create and revise recipes and bills of materials.", "Recipes"),
        new(CatalogPermissions.QualityRead, "View inspection requirements and criteria.", "Quality"),
        new(CatalogPermissions.QualityManage, "Create and revise inspection requirements and criteria.", "Quality")
    ];
}
