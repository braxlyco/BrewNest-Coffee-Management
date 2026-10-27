using BrewNest.BuildingBlocks.Authorization;
using BrewNest.BuildingBlocks.Modules;
using BrewNest.Contracts.Sales;
using BrewNest.Contracts.Kitchen;
using BrewNest.Contracts.Returns;
using BrewNest.Contracts.Payments;
using BrewNest.Modules.Sales.Application.Interfaces;
using BrewNest.Modules.Sales.Application.Services;
using BrewNest.Modules.Sales.Domain.BusinessRules;
using BrewNest.Modules.Sales.Endpoints;
using BrewNest.Modules.Sales.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrewNest.Modules.Sales.DependencyInjection;

/// <summary>Defines the Sales module composition root.</summary>
public sealed class SalesModule() : BrewNestModule("Sales")
{
    /// <inheritdoc />
    public override void RegisterServices(IServiceCollection services,IConfiguration configuration)
    {
        var connectionString=configuration.GetConnectionString("BrewNestDatabase")??string.Empty;services.TryAddSingleton(TimeProvider.System);
        services.AddDbContext<SalesDbContext>(options=>options.UseSqlServer(connectionString,sql=>{sql.MigrationsAssembly(typeof(SalesDbContext).Assembly.FullName);sql.MigrationsHistoryTable("__EFMigrationsHistory","sales");sql.EnableRetryOnFailure();}));
        services.AddScoped<SalesService>();services.AddScoped<IKitchenSalesOrderPort,KitchenSalesOrderPort>();services.AddScoped<IReturnsSalesPort,ReturnsSalesPort>();services.AddScoped<IPaymentsSalesPort,PaymentsSalesPort>();services.TryAddSingleton<ISalesDocumentNumberPort,ConfiguredSalesDocumentNumberProvider>();services.AddSingleton<IPermissionDefinitionProvider,SalesPermissionDefinitionProvider>();
        services.AddAuthorization(options=>{Add(options,SalesPermissions.OrdersRead);Add(options,SalesPermissions.OrdersManage);Add(options,SalesPermissions.DiscountsRequest);Add(options,SalesPermissions.DiscountsApprove);Add(options,SalesPermissions.BillingManage);Add(options,SalesPermissions.OrdersClose);});
    }
    /// <inheritdoc />
    public override void MapEndpoints(IEndpointRouteBuilder endpoints)=>endpoints.MapSalesEndpoints();
    private static void Add(AuthorizationOptions options,string permission)=>options.AddPolicy(permission,policy=>policy.RequireClaim("permission",permission));
}
