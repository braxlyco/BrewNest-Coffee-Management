using BrewNest.BuildingBlocks.Authorization;
using BrewNest.BuildingBlocks.Modules;
using BrewNest.Contracts.Inventory;
using BrewNest.Contracts.Kitchen;
using BrewNest.Contracts.Production;
using BrewNest.Contracts.Procurement;
using BrewNest.Contracts.Returns;
using BrewNest.Modules.Inventory.Application.Interfaces;
using BrewNest.Modules.Inventory.Application.Services;
using BrewNest.Modules.Inventory.Domain.BusinessRules;
using BrewNest.Modules.Inventory.Endpoints;
using BrewNest.Modules.Inventory.Infrastructure.Persistence;
using BrewNest.Modules.Inventory.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrewNest.Modules.Inventory.DependencyInjection;

/// <summary>Defines the Inventory module composition root.</summary>
public sealed class InventoryModule() : BrewNestModule("Inventory")
{
    /// <inheritdoc />
    public override void RegisterServices(IServiceCollection services,IConfiguration configuration)
    {
        var connectionString=configuration.GetConnectionString("BrewNestDatabase")??string.Empty;services.TryAddSingleton(TimeProvider.System);
        services.AddDbContext<InventoryDbContext>(options=>options.UseSqlServer(connectionString,sql=>{sql.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName);sql.MigrationsHistoryTable("__EFMigrationsHistory","inventory");sql.EnableRetryOnFailure();}));
        services.TryAddSingleton<IInventoryCatalogReferenceValidator,PermissiveCatalogReferenceValidator>();services.TryAddSingleton<IInventoryExpiryPolicyProvider,ConfiguredExpiryPolicyProvider>();services.AddScoped<InventoryService>();services.AddScoped<IInventoryReservationPort>(p=>p.GetRequiredService<InventoryService>());services.AddScoped<IKitchenInventoryPort>(p=>p.GetRequiredService<InventoryService>());services.AddScoped<IProductionInventoryPort,ProductionInventoryPort>();services.AddScoped<IProcurementInventoryPort,ProcurementInventoryPort>();services.AddScoped<IReturnsInventoryPort,ReturnsInventoryPort>();services.AddSingleton<IPermissionDefinitionProvider,InventoryPermissionDefinitionProvider>();services.AddHostedService<InventoryDevelopmentSeeder>();
        services.AddAuthorization(options=>{Add(options,InventoryPermissions.StockRead);Add(options,InventoryPermissions.StockManage);Add(options,InventoryPermissions.ReservationsManage);Add(options,InventoryPermissions.AdjustmentsManage);Add(options,InventoryPermissions.WastageManage);Add(options,InventoryPermissions.ExpiryRead);});
    }
    /// <inheritdoc />
    public override void MapEndpoints(IEndpointRouteBuilder endpoints)=>endpoints.MapInventoryEndpoints();
    private static void Add(AuthorizationOptions options,string permission)=>options.AddPolicy(permission,policy=>policy.RequireClaim("permission",permission));
}
