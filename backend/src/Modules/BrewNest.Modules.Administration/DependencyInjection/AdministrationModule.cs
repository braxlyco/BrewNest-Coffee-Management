using BrewNest.BuildingBlocks.Authorization;
using BrewNest.BuildingBlocks.Modules;
using BrewNest.Modules.Administration.Application.Services;
using BrewNest.Modules.Administration.Domain.BusinessRules;
using BrewNest.Modules.Administration.Endpoints;
using BrewNest.Modules.Administration.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BrewNest.Contracts.Procurement;
using BrewNest.Modules.Administration.Infrastructure.Integrations;
using BrewNest.Contracts.Returns;

namespace BrewNest.Modules.Administration.DependencyInjection;

/// <summary>Defines the Administration module composition root.</summary>
public sealed class AdministrationModule() : BrewNestModule("Administration")
{
    /// <inheritdoc />
    public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BrewNestDatabase") ?? string.Empty;
        services.TryAddSingleton(TimeProvider.System);
        services.AddDbContext<AdministrationDbContext>(options => options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(AdministrationDbContext).Assembly.FullName);
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "administration");
            sqlOptions.EnableRetryOnFailure();
        }));
        services.AddScoped<AdministrationService>();
        services.AddScoped<IProcurementApprovalPolicyPort, AdministrationProcurementApprovalPolicyPort>();
        services.AddScoped<IReturnsApprovalPolicyPort, AdministrationReturnsApprovalPolicyPort>();
        services.AddSingleton<IPermissionDefinitionProvider, AdministrationPermissionDefinitionProvider>();
        services.AddAuthorization(options =>
        {
            AddPolicy(options, AdministrationPermissions.SettingsRead);
            AddPolicy(options, AdministrationPermissions.SettingsManage);
            AddPolicy(options, AdministrationPermissions.ApprovalsRead);
            AddPolicy(options, AdministrationPermissions.ApprovalsManage);
            AddPolicy(options, AdministrationPermissions.AuditRead);
        });
    }

    /// <inheritdoc />
    public override void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapAdministrationEndpoints();

    private static void AddPolicy(AuthorizationOptions options, string permission) =>
        options.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
}
