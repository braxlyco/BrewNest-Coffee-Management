using System.Reflection;
using BrewNest.Api.Endpoints;
using BrewNest.Api.Errors;
using BrewNest.BuildingBlocks.Modules;
using BrewNest.BuildingBlocks.RealTime;
using BrewNest.Modules.Administration.DependencyInjection;
using BrewNest.Modules.Catalog.DependencyInjection;
using BrewNest.Modules.Delivery.DependencyInjection;
using BrewNest.Modules.Finance.DependencyInjection;
using BrewNest.Modules.Identity.DependencyInjection;
using BrewNest.Modules.Inventory.DependencyInjection;
using BrewNest.Modules.Kitchen.DependencyInjection;
using BrewNest.Modules.Payments.DependencyInjection;
using BrewNest.Modules.Procurement.DependencyInjection;
using BrewNest.Modules.Production.DependencyInjection;
using BrewNest.Modules.Reporting.DependencyInjection;
using BrewNest.Modules.Returns.DependencyInjection;
using BrewNest.Modules.Sales.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddSignalR();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    });
});

Assembly[] moduleAssemblies =
[
    typeof(IdentityModule).Assembly,
    typeof(AdministrationModule).Assembly,
    typeof(CatalogModule).Assembly,
    typeof(SalesModule).Assembly,
    typeof(KitchenModule).Assembly,
    typeof(ProductionModule).Assembly,
    typeof(InventoryModule).Assembly,
    typeof(ProcurementModule).Assembly,
    typeof(ReturnsModule).Assembly,
    typeof(PaymentsModule).Assembly,
    typeof(DeliveryModule).Assembly,
    typeof(FinanceModule).Assembly,
    typeof(ReportingModule).Assembly
];
builder.Services.AddBrewNestModules(builder.Configuration, moduleAssemblies);

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapFoundationEndpoints();
app.MapHub<BrewNestHub>("/hubs/notifications");
app.MapBrewNestModules();

app.Run();

/// <summary>Provides a public entry-point marker for integration-test hosting.</summary>
public partial class Program;
