using BrewNest.BuildingBlocks.Authorization;
using BrewNest.BuildingBlocks.Modules;
using BrewNest.Modules.Catalog.Application.Services;
using BrewNest.Modules.Catalog.Domain.BusinessRules;
using BrewNest.Modules.Catalog.Endpoints;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using BrewNest.Modules.Catalog.Infrastructure.Persistence.Seed;
using BrewNest.Modules.Catalog.Infrastructure.Integrations;
using BrewNest.Contracts.Sales;
using BrewNest.Contracts.Kitchen;
using BrewNest.Contracts.Production;
using BrewNest.Contracts.Procurement;
using BrewNest.Contracts.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrewNest.Modules.Catalog.DependencyInjection;

/// <summary>Defines the Catalog module composition root.</summary>
public sealed class CatalogModule() : BrewNestModule("Catalog")
{
    /// <inheritdoc />
    public override void RegisterServices(IServiceCollection services,IConfiguration configuration)
    {
        var connectionString=configuration.GetConnectionString("BrewNestDatabase")??string.Empty;
        services.TryAddSingleton(TimeProvider.System);
        services.AddDbContext<CatalogDbContext>(options=>options.UseSqlServer(connectionString,sql=>{sql.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName);sql.MigrationsHistoryTable("__EFMigrationsHistory","catalog");sql.EnableRetryOnFailure();}));
        services.AddScoped<CatalogService>();services.AddScoped<ISalesCatalogPort,CatalogSalesReferencePort>();services.AddScoped<IKitchenRecipePort,CatalogKitchenRecipePort>();services.AddScoped<IProductionRecipePort,CatalogProductionRecipePort>();services.AddScoped<IProcurementCatalogPort,CatalogProcurementPort>();services.AddScoped<IInventoryCatalogReferenceValidator,CatalogInventoryReferenceValidator>();services.AddScoped<IInventoryCatalogReferenceLookup,CatalogInventoryReferenceLookup>();services.AddSingleton<IPermissionDefinitionProvider,CatalogPermissionDefinitionProvider>();services.AddHostedService<CatalogDevelopmentSeeder>();
        services.AddAuthorization(options=>{Add(options,CatalogPermissions.MasterDataRead);Add(options,CatalogPermissions.MasterDataManage);Add(options,CatalogPermissions.RecipesRead);Add(options,CatalogPermissions.RecipesManage);Add(options,CatalogPermissions.QualityRead);Add(options,CatalogPermissions.QualityManage);});
    }
    /// <inheritdoc />
    public override void MapEndpoints(IEndpointRouteBuilder endpoints)=>endpoints.MapCatalogEndpoints();
    private static void Add(AuthorizationOptions options,string permission)=>options.AddPolicy(permission,policy=>policy.RequireClaim("permission",permission));
}
