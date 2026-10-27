using BrewNest.BuildingBlocks.Authorization;
using BrewNest.BuildingBlocks.Modules;
using BrewNest.Contracts.Kitchen;
using BrewNest.Modules.Kitchen.Application.Services;
using BrewNest.Modules.Kitchen.Application.Interfaces;
using BrewNest.Modules.Kitchen.Domain.BusinessRules;
using BrewNest.Modules.Kitchen.Endpoints;
using BrewNest.Modules.Kitchen.Infrastructure.Integrations;
using BrewNest.Modules.Kitchen.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrewNest.Modules.Kitchen.DependencyInjection;

/// <summary>Defines the Kitchen module composition root.</summary>
public sealed class KitchenModule() : BrewNestModule("Kitchen")
{
    /// <inheritdoc />
    public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BrewNestDatabase") ?? string.Empty;
        services.TryAddSingleton(TimeProvider.System);
        services.AddDbContext<KitchenDbContext>(options => options.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsAssembly(typeof(KitchenDbContext).Assembly.FullName);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "kitchen");
            sql.EnableRetryOnFailure();
        }));
        services.AddScoped<KitchenService>();
        services.AddScoped<IKitchenRealtimePublisher, SignalRKitchenRealtimePublisher>();
        services.TryAddSingleton<IKitchenDocumentNumberPort, ConfiguredKitchenDocumentNumberProvider>();
        services.AddSingleton<IPermissionDefinitionProvider, KitchenPermissionDefinitionProvider>();
        services.AddAuthorization(options =>
        {
            Add(options, KitchenPermissions.QueueRead);
            Add(options, KitchenPermissions.TicketsGenerate);
            Add(options, KitchenPermissions.PreparationManage);
            Add(options, KitchenPermissions.TicketsCancel);
        });
    }

    /// <inheritdoc />
    public override void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapKitchenEndpoints();

    private static void Add(AuthorizationOptions options, string permission) =>
        options.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
}
