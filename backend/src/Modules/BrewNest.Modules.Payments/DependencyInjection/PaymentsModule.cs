using BrewNest.BuildingBlocks.Authorization;
using BrewNest.BuildingBlocks.Modules;
using BrewNest.Contracts.Payments;
using BrewNest.Contracts.Returns;
using BrewNest.Modules.Payments.Application.Interfaces;
using BrewNest.Modules.Payments.Application.Services;
using BrewNest.Modules.Payments.Domain.BusinessRules;
using BrewNest.Modules.Payments.Endpoints;
using BrewNest.Modules.Payments.Infrastructure.Integrations;
using BrewNest.Modules.Payments.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrewNest.Modules.Payments.DependencyInjection;

/// <summary>Defines the Payments module composition root.</summary>
public sealed class PaymentsModule() : BrewNestModule("Payments")
{
    /// <inheritdoc />
    public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("BrewNestDatabase") ?? string.Empty;
        services.TryAddSingleton(TimeProvider.System);
        services.AddDbContext<PaymentsDbContext>(options => options.UseSqlServer(connection, sql =>
        {
            sql.MigrationsAssembly(typeof(PaymentsDbContext).Assembly.FullName);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "payments");
            sql.EnableRetryOnFailure();
        }));
        services.AddScoped<PaymentsService>();
        services.AddScoped<IReturnsPaymentPort>(provider => provider.GetRequiredService<PaymentsService>());
        services.AddScoped<IPaymentsCodPort>(provider => provider.GetRequiredService<PaymentsService>());
        services.AddSingleton<IPaymentGateway, EasypaisaSimulatorGateway>();
        services.AddSingleton<IPaymentGateway, JazzCashSimulatorGateway>();
        services.AddSingleton<IPaymentDocumentNumbers, PaymentDocumentNumbers>();
        services.AddSingleton<IPermissionDefinitionProvider, PaymentsPermissionProvider>();
        services.AddAuthorization(options =>
        {
            foreach (var permission in new[] { PaymentsPermissions.Read, PaymentsPermissions.Create, PaymentsPermissions.Verify,
                PaymentsPermissions.Settle, PaymentsPermissions.RefundsRead, PaymentsPermissions.RefundsProcess })
                options.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
        });
    }

    /// <inheritdoc />
    public override void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapPayments();
}
