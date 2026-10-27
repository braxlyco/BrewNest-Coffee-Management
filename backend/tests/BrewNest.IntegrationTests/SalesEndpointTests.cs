using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrewNest.Contracts.Catalog;
using BrewNest.Contracts.Identity;
using BrewNest.Contracts.Sales;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using BrewNest.Modules.Sales.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.IntegrationTests;

/// <summary>Exercises Sales authorization, fulfillment modes, discounts, billing, and history.</summary>
public sealed class SalesEndpointTests:IClassFixture<IdentityWebApplicationFactory>,IAsyncLifetime
{
    private readonly IdentityWebApplicationFactory factory;
    /// <summary>Creates the Sales HTTP test suite.</summary>
    public SalesEndpointTests(IdentityWebApplicationFactory factory)=>this.factory=factory;
    /// <inheritdoc />
    public async Task InitializeAsync(){await using var scope=factory.Services.CreateAsyncScope();await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.EnsureCreatedAsync();await scope.ServiceProvider.GetRequiredService<SalesDbContext>().Database.EnsureCreatedAsync();}
    /// <inheritdoc />
    public Task DisposeAsync()=>Task.CompletedTask;

    /// <summary>Sales APIs reject anonymous users and authenticated users without Sales permissions.</summary>
    [Fact]public async Task SalesRequiresExplicitPermission(){using var client=Client();using var anonymous=await client.GetAsync("/api/sales/orders");Assert.Equal(HttpStatusCode.Unauthorized,anonymous.StatusCode);var admin=await LoginAdmin(client);Bearer(client,admin.AccessToken);var email=$"sales-denied-{Guid.NewGuid():N}@test.local";var password=IdentityWebApplicationFactory.CreateValidPassword();using var created=await client.PostAsJsonAsync("/api/identity/users",new CreateUserRequest(email,"Sales Denied",password,[]));Assert.Equal(HttpStatusCode.OK,created.StatusCode);client.DefaultRequestHeaders.Authorization=null;using var login=await client.PostAsJsonAsync("/api/identity/auth/login",new LoginRequest(email,password));var token=Assert.IsType<TokenResponse>(await login.Content.ReadFromJsonAsync<TokenResponse>());Bearer(client,token.AccessToken);using var forbidden=await client.GetAsync("/api/sales/orders");Assert.Equal(HttpStatusCode.Forbidden,forbidden.StatusCode);}

    /// <summary>A dining table cannot have two simultaneous active orders.</summary>
    [Fact]public async Task DineInTableCannotBeDoubleAssigned(){using var client=await AdminClient();var suffix=Guid.NewGuid().ToString("N")[..6];var table=await Post<DiningTableResponse>(client,"/api/catalog/dining-tables",new CreateDiningTableRequest($"s-{suffix}","Sales table",4,"Main"));await Post<SalesOrderResponse>(client,"/api/sales/orders",new CreateSalesOrderRequest(SalesOrderType.DineIn,null,table.Id,"","","",""));using var duplicate=await client.PostAsJsonAsync("/api/sales/orders",new CreateSalesOrderRequest(SalesOrderType.DineIn,null,table.Id,"","","",""));Assert.Equal(HttpStatusCode.BadRequest,duplicate.StatusCode);}

    /// <summary>A pending discount blocks billing until a manager approves it, then bill and closure succeed.</summary>
    [Fact]public async Task DiscountApprovalGatesFinalBilling(){using var client=await AdminClient();var menu=await CreateMenuItem(client);var order=await Post<SalesOrderResponse>(client,"/api/sales/orders",new CreateSalesOrderRequest(SalesOrderType.Takeaway,null,null,"Walk-in","","","test"));order=await Post<SalesOrderResponse>(client,$"/api/sales/orders/{order.Id}/lines",new AddSalesOrderLineRequest(menu.Id,2m,"extra hot",order.Version));Assert.Equal(menu.SalesPrice!.Value*2m,order.GrossAmount);order=await Post<SalesOrderResponse>(client,$"/api/sales/orders/{order.Id}/confirm",new ChangeSalesOrderStatusRequest("confirmed",order.Version));order=await Post<SalesOrderResponse>(client,$"/api/sales/orders/{order.Id}/discounts",new RequestSalesDiscountRequest(10m,"manager promotion",order.Version));using var blocked=await client.PostAsJsonAsync($"/api/sales/orders/{order.Id}/bill",new ChangeSalesOrderStatusRequest("bill",order.Version));Assert.Equal(HttpStatusCode.Conflict,blocked.StatusCode);var discount=Assert.Single(order.Discounts);order=await Post<SalesOrderResponse>(client,$"/api/sales/orders/{order.Id}/discounts/{discount.Id}/decision",new DecideSalesDiscountRequest(true,"approved",order.Version,discount.Version));order=await Post<SalesOrderResponse>(client,$"/api/sales/orders/{order.Id}/bill",new ChangeSalesOrderStatusRequest("bill",order.Version));Assert.NotNull(order.Bill);Assert.Equal(order.GrossAmount-order.DiscountAmount,order.Bill.NetAmount);order=await Post<SalesOrderResponse>(client,$"/api/sales/orders/{order.Id}/close",new ChangeSalesOrderStatusRequest("complete",order.Version));Assert.Equal(SalesOrderStatus.Closed,order.Status);var history=await client.GetFromJsonAsync<SalesStatusHistoryResponse[]>($"/api/sales/status-history?orderId={order.Id}");Assert.Contains(history!,x=>x.NewStatus=="Billed");Assert.Contains(history!,x=>x.NewStatus=="Closed");}

    /// <summary>Delivery validation and stale concurrency tokens return consistent client errors.</summary>
    [Fact]public async Task DeliveryAndConcurrencyValidationAreEnforced(){using var client=await AdminClient();using var invalid=await client.PostAsJsonAsync("/api/sales/orders",new CreateSalesOrderRequest(SalesOrderType.Delivery,null,null,"Customer","","",""));Assert.Equal(HttpStatusCode.BadRequest,invalid.StatusCode);var order=await Post<SalesOrderResponse>(client,"/api/sales/orders",new CreateSalesOrderRequest(SalesOrderType.Delivery,null,null,"Customer","+923001234567","Lahore",""));var menu=await CreateMenuItem(client);using var stale=await client.PostAsJsonAsync($"/api/sales/orders/{order.Id}/lines",new AddSalesOrderLineRequest(menu.Id,1m,"","not-base64"));Assert.Equal(HttpStatusCode.Conflict,stale.StatusCode);}

    private static async Task<CatalogItemResponse> CreateMenuItem(HttpClient client){var suffix=Guid.NewGuid().ToString("N")[..6];var unit=await Post<UnitOfMeasureResponse>(client,"/api/catalog/units",new CreateUnitOfMeasureRequest($"su-{suffix}","Sales unit","ea","Count",0));return await Post<CatalogItemResponse>(client,"/api/catalog/items",new CreateCatalogItemRequest($"sm-{suffix}","Test coffee","","MenuItem",unit.Id,null,500m,100m,null,false));}
    private HttpClient Client()=>factory.CreateClient(new(){BaseAddress=new Uri("https://localhost")});private async Task<HttpClient> AdminClient(){var client=Client();var token=await LoginAdmin(client);Bearer(client,token.AccessToken);return client;}private async Task<TokenResponse> LoginAdmin(HttpClient client){client.DefaultRequestHeaders.Authorization=null;using var response=await client.PostAsJsonAsync("/api/identity/auth/login",new LoginRequest(factory.AdministratorEmail,factory.AdministratorPassword));Assert.Equal(HttpStatusCode.OK,response.StatusCode);return Assert.IsType<TokenResponse>(await response.Content.ReadFromJsonAsync<TokenResponse>());}private static void Bearer(HttpClient client,string token)=>client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",token);private static async Task<T> Post<T>(HttpClient client,string uri,object request){using var response=await client.PostAsJsonAsync(uri,request);Assert.Equal(HttpStatusCode.OK,response.StatusCode);return Assert.IsType<T>(await response.Content.ReadFromJsonAsync<T>());}
}
