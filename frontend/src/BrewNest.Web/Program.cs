using BrewNest.Web;
using BrewNest.Web.Features.Identity.Models;
using BrewNest.Web.Features.Identity.Services;
using BrewNest.Web.Features.Identity.State;
using BrewNest.Web.Features.Administration.Models;
using BrewNest.Web.Features.Administration.Services;
using BrewNest.Web.Features.Catalog.Models;
using BrewNest.Web.Features.Catalog.Services;
using BrewNest.Web.Features.Inventory.Models;
using BrewNest.Web.Features.Inventory.Services;
using BrewNest.Web.Features.Sales.Models;
using BrewNest.Web.Features.Sales.Services;
using BrewNest.Web.Features.Kitchen.Models;
using BrewNest.Web.Features.Kitchen.Services;
using BrewNest.Web.Features.Kitchen.State;
using BrewNest.Web.Features.Production.Models;
using BrewNest.Web.Features.Production.Services;
using BrewNest.Web.Features.Procurement.Models;
using BrewNest.Web.Features.Procurement.Services;
using BrewNest.Web.Features.Returns.Models;
using BrewNest.Web.Features.Returns.Services;
using BrewNest.Web.Features.Payments.Models;
using BrewNest.Web.Features.Payments.Services;
using BrewNest.Web.Services.Api;
using BrewNest.Web.Services.RealTime;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var configuredApiUrl = builder.Configuration["ApiBaseUrl"];
var apiBaseUrl = string.IsNullOrWhiteSpace(configuredApiUrl)
    ? builder.HostEnvironment.BaseAddress
    : configuredApiUrl;

builder.Services.AddAuthorizationCore(options =>
{
    string[] permissions =
    [
        IdentityPermissionNames.UsersRead,
        IdentityPermissionNames.UsersManage,
        IdentityPermissionNames.RolesRead,
        IdentityPermissionNames.RolesManage,
        IdentityPermissionNames.PermissionsRead,
        IdentityPermissionNames.PermissionsManage,
        AdministrationPermissionNames.SettingsRead,
        AdministrationPermissionNames.SettingsManage,
        AdministrationPermissionNames.ApprovalsRead,
        AdministrationPermissionNames.ApprovalsManage,
        AdministrationPermissionNames.AuditRead,
        CatalogPermissionNames.MasterDataRead,
        CatalogPermissionNames.MasterDataManage,
        CatalogPermissionNames.RecipesRead,
        CatalogPermissionNames.RecipesManage,
        CatalogPermissionNames.QualityRead,
        CatalogPermissionNames.QualityManage,
        InventoryPermissionNames.StockRead,
        InventoryPermissionNames.StockManage,
        InventoryPermissionNames.ReservationsManage,
        InventoryPermissionNames.AdjustmentsManage,
        InventoryPermissionNames.WastageManage,
        InventoryPermissionNames.ExpiryRead,
        SalesPermissionNames.OrdersRead,
        SalesPermissionNames.OrdersManage,
        SalesPermissionNames.DiscountsRequest,
        SalesPermissionNames.DiscountsApprove,
        SalesPermissionNames.BillingManage,
        SalesPermissionNames.OrdersClose,
        KitchenPermissionNames.QueueRead,
        KitchenPermissionNames.TicketsGenerate,
        KitchenPermissionNames.PreparationManage,
        KitchenPermissionNames.TicketsCancel,
        ProductionPermissionNames.Read,
        ProductionPermissionNames.PlansManage,
        ProductionPermissionNames.OrdersRelease,
        ProductionPermissionNames.MaterialsIssue,
        ProductionPermissionNames.CompletionManage
        ,ProcurementPermissionNames.Read,ProcurementPermissionNames.RequisitionsManage,ProcurementPermissionNames.RequisitionsApprove,ProcurementPermissionNames.SourcingManage,ProcurementPermissionNames.OrdersManage,ProcurementPermissionNames.OrdersApprove,ProcurementPermissionNames.InspectionsManage,ProcurementPermissionNames.ReceiptsManage,ProcurementPermissionNames.ReturnsManage
        ,ReturnsPermissionNames.Read,ReturnsPermissionNames.Manage,ReturnsPermissionNames.Approve,ReturnsPermissionNames.Inspect,ReturnsPermissionNames.Voids,ReturnsPermissionNames.VoidsApprove,ReturnsPermissionNames.Refunds
        ,PaymentsPermissionNames.Read,PaymentsPermissionNames.Create,PaymentsPermissionNames.Verify,PaymentsPermissionNames.Settle,PaymentsPermissionNames.RefundsRead,PaymentsPermissionNames.RefundsProcess
    ];
    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
    }
});
builder.Services.AddScoped<IdentityTokenStore>();
builder.Services.AddScoped<IdentityApiClient>();
builder.Services.AddScoped<AdministrationApiClient>();
builder.Services.AddScoped<CatalogApiClient>();
builder.Services.AddScoped<InventoryApiClient>();
builder.Services.AddScoped<SalesApiClient>();
builder.Services.AddScoped<KitchenApiClient>();
builder.Services.AddScoped<KitchenQueueState>();
builder.Services.AddScoped<ProductionApiClient>();
builder.Services.AddScoped<ProcurementApiClient>();
builder.Services.AddScoped<ReturnsApiClient>();
builder.Services.AddScoped<PaymentsApiClient>();
builder.Services.AddScoped<IdentitySessionState>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<IdentitySessionState>());
builder.Services.AddScoped<JwtAuthorizationHandler>();
builder.Services.AddScoped(provider =>
{
    var handler = provider.GetRequiredService<JwtAuthorizationHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl) };
});
builder.Services.AddScoped<IBrewNestApiClient, BrewNestApiClient>();
builder.Services.AddScoped<BrewNestRealtimeClient>();

await builder.Build().RunAsync();
