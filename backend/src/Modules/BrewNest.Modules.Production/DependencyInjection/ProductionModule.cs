using BrewNest.BuildingBlocks.Authorization;
using BrewNest.BuildingBlocks.Modules;
using BrewNest.Modules.Production.Application.Interfaces;
using BrewNest.Modules.Production.Application.Services;
using BrewNest.Modules.Production.Domain.BusinessRules;
using BrewNest.Modules.Production.Endpoints;
using BrewNest.Modules.Production.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrewNest.Modules.Production.DependencyInjection;

/// <summary>Defines the Production module composition root.</summary>
public sealed class ProductionModule() : BrewNestModule("Production")
{
    /// <inheritdoc />
    public override void RegisterServices(IServiceCollection services,IConfiguration configuration)
    {
        var connection=configuration.GetConnectionString("BrewNestDatabase")??string.Empty;services.TryAddSingleton(TimeProvider.System);services.AddDbContext<ProductionDbContext>(options=>options.UseSqlServer(connection,sql=>{sql.MigrationsAssembly(typeof(ProductionDbContext).Assembly.FullName);sql.MigrationsHistoryTable("__EFMigrationsHistory","production");sql.EnableRetryOnFailure();}));services.AddScoped<ProductionService>();services.TryAddSingleton<IProductionDocumentNumberPort,ConfiguredProductionDocumentNumberProvider>();services.AddSingleton<IPermissionDefinitionProvider,ProductionPermissionDefinitionProvider>();services.AddAuthorization(options=>{Add(options,ProductionPermissions.Read);Add(options,ProductionPermissions.PlansManage);Add(options,ProductionPermissions.OrdersRelease);Add(options,ProductionPermissions.MaterialsIssue);Add(options,ProductionPermissions.CompletionManage);});
    }
    /// <inheritdoc />
    public override void MapEndpoints(IEndpointRouteBuilder endpoints)=>endpoints.MapProductionEndpoints();
    private static void Add(AuthorizationOptions options,string permission)=>options.AddPolicy(permission,policy=>policy.RequireClaim("permission",permission));
}
