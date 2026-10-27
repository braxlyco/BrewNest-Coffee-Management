using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrewNest.Contracts.Identity;
using BrewNest.Contracts.Inventory;
using BrewNest.Contracts.Catalog;
using BrewNest.Modules.Inventory.Infrastructure.Persistence;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.IntegrationTests;

/// <summary>Exercises Inventory authorization, ledger operations, expiry, concurrency, and quantity safeguards.</summary>
public sealed class InventoryEndpointTests:IClassFixture<IdentityWebApplicationFactory>,IAsyncLifetime
{
    private readonly IdentityWebApplicationFactory factory;
    /// <summary>Creates the Inventory HTTP test suite.</summary>
    public InventoryEndpointTests(IdentityWebApplicationFactory factory)=>this.factory=factory;
    /// <inheritdoc />
    public async Task InitializeAsync(){await using var scope=factory.Services.CreateAsyncScope();await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.EnsureCreatedAsync();await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.EnsureCreatedAsync();}
    /// <inheritdoc />
    public Task DisposeAsync()=>Task.CompletedTask;

    /// <summary>Inventory endpoints require authentication and an explicit Inventory permission.</summary>
    [Fact]
    public async Task InventoryRequiresExplicitPermission()
    {
        using var client=Client();using var anonymous=await client.GetAsync("/api/inventory/lots");Assert.Equal(HttpStatusCode.Unauthorized,anonymous.StatusCode);
        var admin=await LoginAdmin(client);Bearer(client,admin.AccessToken);var email=$"inventory-denied-{Guid.NewGuid():N}@test.local";var password=IdentityWebApplicationFactory.CreateValidPassword();using var created=await client.PostAsJsonAsync("/api/identity/users",new CreateUserRequest(email,"Inventory Denied",password,[]));Assert.Equal(HttpStatusCode.OK,created.StatusCode);
        client.DefaultRequestHeaders.Authorization=null;using var login=await client.PostAsJsonAsync("/api/identity/auth/login",new LoginRequest(email,password));var token=Assert.IsType<TokenResponse>(await login.Content.ReadFromJsonAsync<TokenResponse>());Bearer(client,token.AccessToken);using var forbidden=await client.GetAsync("/api/inventory/lots");Assert.Equal(HttpStatusCode.Forbidden,forbidden.StatusCode);
    }

    /// <summary>Inventory staff can select Catalog references without a separate Catalog grant.</summary>
    [Fact]
    public async Task InventoryOnlyRoleCanReadActiveReferenceChoices()
    {
        using var client = await AdminClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var unit = await Post<UnitOfMeasureResponse>(client, "/api/catalog/units",
            new CreateUnitOfMeasureRequest($"ref-u-{suffix}", "Reference unit", "ea", "Count", 0));
        var item = await Post<CatalogItemResponse>(client, "/api/catalog/items",
            new CreateCatalogItemRequest($"ref-i-{suffix}", "Reference material", "", "RawMaterial", unit.Id, null, null, 10m, null, false));
        var warehouse = await Post<WarehouseResponse>(client, "/api/catalog/warehouses",
            new CreateWarehouseRequest($"ref-w-{suffix}", "Reference warehouse", "Test"));
        var role = await Post<RoleResponse>(client, "/api/identity/roles",
            new CreateRoleRequest($"Store {suffix}", "Inventory-only test role"));
        using var grant = await client.PutAsJsonAsync($"/api/identity/roles/{role.Id}/permissions",
            new SetRolePermissionsRequest(["inventory.stock.read"]));
        Assert.Equal(HttpStatusCode.NoContent, grant.StatusCode);
        var email = $"store-{suffix}@test.local";
        var password = IdentityWebApplicationFactory.CreateValidPassword();
        await Post<UserSummaryResponse>(client, "/api/identity/users",
            new CreateUserRequest(email, "Store Test", password, [role.Name]));
        var token = await Login(client, email, password);
        Bearer(client, token.AccessToken);

        using var catalogDenied = await client.GetAsync("/api/catalog/items");
        Assert.Equal(HttpStatusCode.Forbidden, catalogDenied.StatusCode);
        var choices = await client.GetFromJsonAsync<InventoryCatalogReferenceData>("/api/inventory/references");
        Assert.NotNull(choices);
        Assert.Contains(choices.Items, choice => choice.Id == item.Id);
        Assert.Contains(choices.Warehouses, choice => choice.Id == warehouse.Id);
    }

    /// <summary>Receipt, reservation, issue, release, and ledger projections preserve both balances.</summary>
    [Fact]
    public async Task StockLifecycleMaintainsBalancesAndLedger()
    {
        using var client=await AdminClient();var lot=await Receipt(client,10m,DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
        var reservation=await Post<ReservationResponse>(client,"/api/inventory/reservations",new CreateReservationRequest(lot.Id,4m,"Kitchen","KOT-100",null,"prep"));
        lot=Assert.Single((await client.GetFromJsonAsync<StockLotResponse[]>("/api/inventory/lots"))!,x=>x.Id==lot.Id);Assert.Equal(6m,lot.AvailableQuantity);
        using var excessive=await client.PostAsJsonAsync("/api/inventory/issues",new GoodsIssueRequest(lot.Id,null,7m,"Manual","GI-X","",lot.Version,null));Assert.Equal(HttpStatusCode.Conflict,excessive.StatusCode);
        lot=await Post<StockLotResponse>(client,"/api/inventory/issues",new GoodsIssueRequest(lot.Id,reservation.Id,3m,"Kitchen","GI-100","served",lot.Version,reservation.Version));Assert.Equal(7m,lot.OnHandQuantity);Assert.Equal(1m,lot.ReservedQuantity);
        reservation=Assert.Single((await client.GetFromJsonAsync<ReservationResponse[]>("/api/inventory/reservations"))!,x=>x.Id==reservation.Id);await Post<ReservationResponse>(client,$"/api/inventory/reservations/{reservation.Id}/release",new ReleaseReservationRequest(1m,"cancelled",reservation.Version));
        lot=Assert.Single((await client.GetFromJsonAsync<StockLotResponse[]>("/api/inventory/lots"))!,x=>x.Id==lot.Id);Assert.Equal(7m,lot.AvailableQuantity);
        var ledger=await client.GetFromJsonAsync<StockMovementResponse[]>($"/api/inventory/movements?lotId={lot.Id}");Assert.Contains(ledger!,x=>x.MovementType=="GoodsReceipt");Assert.Contains(ledger!,x=>x.MovementType=="Reservation");Assert.Contains(ledger!,x=>x.MovementType=="GoodsIssue");Assert.Contains(ledger!,x=>x.MovementType=="ReservationRelease");
    }

    /// <summary>Expired lots are automatically blocked and excluded from reservation and issue.</summary>
    [Fact]
    public async Task ExpiredLotIsBlockedAndAlerted()
    {
        using var client=await AdminClient();var lot=await Receipt(client,3m,DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        using var reserve=await client.PostAsJsonAsync("/api/inventory/reservations",new CreateReservationRequest(lot.Id,1m,"Kitchen","KOT-EXP",null,""));Assert.Equal(HttpStatusCode.Conflict,reserve.StatusCode);
        lot=Assert.Single((await client.GetFromJsonAsync<StockLotResponse[]>("/api/inventory/lots"))!,x=>x.Id==lot.Id);Assert.True(lot.IsBlocked);
        var alerts=await client.GetFromJsonAsync<ExpiryAlertResponse[]>("/api/inventory/expiry-alerts");Assert.Contains(alerts!,x=>x.StockLotId==lot.Id&&x.Severity=="Expired"&&x.IsBlocked);
        using var issue=await client.PostAsJsonAsync("/api/inventory/issues",new GoodsIssueRequest(lot.Id,null,1m,"Manual","GI-EXP","",lot.Version,null));Assert.Equal(HttpStatusCode.Conflict,issue.StatusCode);
    }

    /// <summary>Adjustments and wastage are ledger-backed, concurrency checked, and cannot make inventory negative.</summary>
    [Fact]
    public async Task AdjustmentAndWastageEnforceConcurrencyAndNonnegativeStock()
    {
        using var client=await AdminClient();var lot=await Receipt(client,5m,DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
        using var stale=await client.PostAsJsonAsync("/api/inventory/adjustments",new StockAdjustmentRequest(lot.Id,1m,"count","ADJ-X","not-base64"));Assert.Equal(HttpStatusCode.Conflict,stale.StatusCode);
        lot=await Post<StockLotResponse>(client,"/api/inventory/adjustments",new StockAdjustmentRequest(lot.Id,2m,"count gain","ADJ-1",lot.Version));Assert.Equal(7m,lot.OnHandQuantity);
        lot=await Post<StockLotResponse>(client,"/api/inventory/wastage",new WastageRequest(lot.Id,3m,"damaged","W-1",lot.Version));Assert.Equal(4m,lot.OnHandQuantity);
        using var negative=await client.PostAsJsonAsync("/api/inventory/adjustments",new StockAdjustmentRequest(lot.Id,-5m,"invalid","ADJ-2",lot.Version));Assert.Equal(HttpStatusCode.Conflict,negative.StatusCode);
    }

    private HttpClient Client()=>factory.CreateClient(new(){BaseAddress=new Uri("https://localhost")});
    private async Task<HttpClient> AdminClient(){var client=Client();var tokens=await LoginAdmin(client);Bearer(client,tokens.AccessToken);return client;}
    private async Task<TokenResponse> LoginAdmin(HttpClient client){client.DefaultRequestHeaders.Authorization=null;using var response=await client.PostAsJsonAsync("/api/identity/auth/login",new LoginRequest(factory.AdministratorEmail,factory.AdministratorPassword));Assert.Equal(HttpStatusCode.OK,response.StatusCode);return Assert.IsType<TokenResponse>(await response.Content.ReadFromJsonAsync<TokenResponse>());}
    private static async Task<TokenResponse> Login(HttpClient client, string email, string password){client.DefaultRequestHeaders.Authorization=null;using var response=await client.PostAsJsonAsync("/api/identity/auth/login",new LoginRequest(email,password));Assert.Equal(HttpStatusCode.OK,response.StatusCode);return Assert.IsType<TokenResponse>(await response.Content.ReadFromJsonAsync<TokenResponse>());}
    private static async Task<StockLotResponse> Receipt(HttpClient client,decimal quantity,DateOnly expiry)
    {
        var suffix=Guid.NewGuid().ToString("N")[..8];
        var unit=await Post<UnitOfMeasureResponse>(client,"/api/catalog/units",new CreateUnitOfMeasureRequest($"iu-{suffix}","Inventory test unit","kg","Weight",3));
        var item=await Post<CatalogItemResponse>(client,"/api/catalog/items",new CreateCatalogItemRequest($"ii-{suffix}","Inventory test material","","RawMaterial",unit.Id,null,null,10m,null,false));
        var warehouse=await Post<WarehouseResponse>(client,"/api/catalog/warehouses",new CreateWarehouseRequest($"iw-{suffix}","Inventory test warehouse","Test"));
        return await Post<StockLotResponse>(client,"/api/inventory/receipts",new GoodsReceiptRequest(item.Id,warehouse.Id,$"LOT-{suffix}",DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),expiry,quantity,"ManualReceipt",$"GR-{suffix}","test"));
    }
    private static void Bearer(HttpClient client,string token)=>client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",token);
    private static async Task<T> Post<T>(HttpClient client,string uri,object request){using var response=await client.PostAsJsonAsync(uri,request);Assert.Equal(HttpStatusCode.OK,response.StatusCode);return Assert.IsType<T>(await response.Content.ReadFromJsonAsync<T>());}
}
