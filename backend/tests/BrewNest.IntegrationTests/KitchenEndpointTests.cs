using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrewNest.Contracts.Catalog;
using BrewNest.Contracts.Identity;
using BrewNest.Contracts.Inventory;
using BrewNest.Contracts.Kitchen;
using BrewNest.Contracts.Sales;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using BrewNest.Modules.Inventory.Infrastructure.Persistence;
using BrewNest.Modules.Kitchen.Infrastructure.Persistence;
using BrewNest.Modules.Sales.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.IntegrationTests;

/// <summary>Exercises KOT authorization, generation, Inventory reservation, status, and cancellation endpoints.</summary>
public sealed class KitchenEndpointTests : IClassFixture<IdentityWebApplicationFactory>, IAsyncLifetime
{
    private readonly IdentityWebApplicationFactory factory;

    /// <summary>Creates the Kitchen HTTP test suite.</summary>
    public KitchenEndpointTests(IdentityWebApplicationFactory factory) => this.factory = factory;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<SalesDbContext>().Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<KitchenDbContext>().Database.EnsureCreatedAsync();
    }

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Kitchen APIs reject anonymous access and users without explicit Kitchen permissions.</summary>
    [Fact]
    public async Task KitchenRequiresExplicitPermission()
    {
        using var client = Client();
        using var anonymous = await client.GetAsync("/api/kitchen/tickets");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        var admin = await LoginAdmin(client); Bearer(client, admin.AccessToken);
        var email = $"kitchen-denied-{Guid.NewGuid():N}@test.local"; var password = IdentityWebApplicationFactory.CreateValidPassword();
        using var created = await client.PostAsJsonAsync("/api/identity/users", new CreateUserRequest(email, "Kitchen Denied", password, []));
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        client.DefaultRequestHeaders.Authorization = null;
        using var login = await client.PostAsJsonAsync("/api/identity/auth/login", new LoginRequest(email, password));
        var token = Assert.IsType<TokenResponse>(await login.Content.ReadFromJsonAsync<TokenResponse>()); Bearer(client, token.AccessToken);
        using var forbidden = await client.GetAsync("/api/kitchen/tickets");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    /// <summary>Initial and additional KOTs reserve only newly added recipe requirements.</summary>
    [Fact]
    public async Task InitialAndAdditionalTicketsReserveIngredients()
    {
        using var client = await AdminClient(); var setup = await CreateConfirmedOrder(client);
        var available = await client.GetFromJsonAsync<KitchenConfirmedOrderReference[]>("/api/kitchen/references/confirmed-orders");
        Assert.Contains(available!, x => x.Id == setup.Order.Id && x.PendingLineCount == 1);
        Assert.Contains(available!, x => x.Id == setup.Order.Id && x.ItemPreview.Contains("Test coffee", StringComparison.Ordinal) && x.OrderType == "Takeaway");
        var first = await Post<KitchenTicketResponse>(client, "/api/kitchen/tickets", new GenerateKitchenTicketRequest(setup.Order.Id));
        Assert.Equal(1, first.Sequence); Assert.Single(first.Lines); Assert.NotEmpty(first.Reservations);
        available = await client.GetFromJsonAsync<KitchenConfirmedOrderReference[]>("/api/kitchen/references/confirmed-orders");
        Assert.DoesNotContain(available!, x => x.Id == setup.Order.Id);
        setup.Order = await Post<SalesOrderResponse>(client, $"/api/sales/orders/{setup.Order.Id}/lines", new AddSalesOrderLineRequest(setup.MenuItem.Id, 2m, "no sugar", setup.Order.Version));
        available = await client.GetFromJsonAsync<KitchenConfirmedOrderReference[]>("/api/kitchen/references/confirmed-orders");
        Assert.Contains(available!, x => x.Id == setup.Order.Id && x.PendingLineCount == 1);
        var second = await Post<KitchenTicketResponse>(client, "/api/kitchen/tickets", new GenerateKitchenTicketRequest(setup.Order.Id));
        Assert.Equal(2, second.Sequence); Assert.Equal(2m, Assert.Single(second.Lines).Quantity);
        using var duplicate = await client.PostAsJsonAsync("/api/kitchen/tickets", new GenerateKitchenTicketRequest(setup.Order.Id));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    /// <summary>Queued cancellation releases reservations, while started preparation cannot be cancelled.</summary>
    [Fact]
    public async Task CancellationRulesAndStatusHistoryAreEnforced()
    {
        using var client = await AdminClient(); var setup = await CreateConfirmedOrder(client);
        var ticket = await Post<KitchenTicketResponse>(client, "/api/kitchen/tickets", new GenerateKitchenTicketRequest(setup.Order.Id));
        var cancelled = await Post<KitchenTicketResponse>(client, $"/api/kitchen/tickets/{ticket.Id}/cancel", new ChangeKitchenTicketStatusRequest(KitchenTicketStatus.Cancelled, "Customer cancelled.", ticket.Version));
        Assert.Equal(KitchenTicketStatus.Cancelled, cancelled.Status);
        var reservations = await client.GetFromJsonAsync<ReservationResponse[]>($"/api/inventory/reservations?referenceType=KitchenKOT&referenceId={ticket.Id:N}");
        Assert.NotEmpty(reservations!); Assert.All(reservations!, reservation => Assert.Equal("Released", reservation.Status));

        setup = await CreateConfirmedOrder(client); ticket = await Post<KitchenTicketResponse>(client, "/api/kitchen/tickets", new GenerateKitchenTicketRequest(setup.Order.Id)); var line = Assert.Single(ticket.Lines);
        ticket = await Post<KitchenTicketResponse>(client, $"/api/kitchen/tickets/{ticket.Id}/lines/{line.Id}/status", new ChangeKitchenLineStatusRequest(KitchenLineStatus.Preparing, "Preparation started.", ticket.Version, line.Version));
        using var tooLate = await client.PostAsJsonAsync($"/api/kitchen/tickets/{ticket.Id}/cancel", new ChangeKitchenTicketStatusRequest(KitchenTicketStatus.Cancelled, "Late cancellation.", ticket.Version));
        Assert.Equal(HttpStatusCode.Conflict, tooLate.StatusCode);
        var history = await client.GetFromJsonAsync<KitchenStatusHistoryResponse[]>($"/api/kitchen/status-history?ticketId={ticket.Id}");
        Assert.Contains(history!, item => item.NewStatus == "Preparing");
    }

    /// <summary>A stock shortage identifies the material without exposing its database ID.</summary>
    [Fact]
    public async Task InsufficientStockReturnsStaffFriendlyProblemDetails()
    {
        using var client=await AdminClient();
        var setup=await CreateConfirmedOrder(client,receiveStock:false);
        using var response=await client.PostAsJsonAsync("/api/kitchen/tickets",new GenerateKitchenTicketRequest(setup.Order.Id));
        Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);
        var detail=await response.Content.ReadAsStringAsync();
        Assert.Contains("Coffee beans",detail,StringComparison.Ordinal);
        Assert.Contains("kg",detail,StringComparison.Ordinal);
        Assert.Contains("receive more stock",detail,StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(setup.IngredientId.ToString(),detail,StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<KitchenSetup> CreateConfirmedOrder(HttpClient client,bool receiveStock=true)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var unit = await Post<UnitOfMeasureResponse>(client, "/api/catalog/units", new CreateUnitOfMeasureRequest($"ku-{suffix}", "Kitchen unit", "kg", "Weight", 3));
        var ingredient = await Post<CatalogItemResponse>(client, "/api/catalog/items", new CreateCatalogItemRequest($"ki-{suffix}", "Coffee beans", "", "RawMaterial", unit.Id, null, null, 1000m, 365, false));
        var menu = await Post<CatalogItemResponse>(client, "/api/catalog/items", new CreateCatalogItemRequest($"km-{suffix}", "Test coffee", "", "MenuItem", unit.Id, null, 500m, 100m, null, false));
        var warehouse = await Post<WarehouseResponse>(client, "/api/catalog/warehouses", new CreateWarehouseRequest($"kw-{suffix}", "Kitchen warehouse", "Test"));
        await Post<RecipeResponse>(client, "/api/catalog/recipes", new CreateRecipeRequest($"kr-{suffix}", "Coffee recipe", menu.Id, 1m, unit.Id, [new RecipeLineRequest(ingredient.Id, 0.020m, 0m, 1)]));
        if(receiveStock)await Post<StockLotResponse>(client, "/api/inventory/receipts", new GoodsReceiptRequest(ingredient.Id, warehouse.Id, $"LOT-{suffix}", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)), 10m, "Test", suffix, "Kitchen test stock"));
        var order = await Post<SalesOrderResponse>(client, "/api/sales/orders", new CreateSalesOrderRequest(SalesOrderType.Takeaway, null, null, "Walk-in", "", "", "Kitchen test"));
        order = await Post<SalesOrderResponse>(client, $"/api/sales/orders/{order.Id}/lines", new AddSalesOrderLineRequest(menu.Id, 1m, "extra hot", order.Version));
        order = await Post<SalesOrderResponse>(client, $"/api/sales/orders/{order.Id}/confirm", new ChangeSalesOrderStatusRequest("Send to kitchen", order.Version));
        return new(order, menu, ingredient.Id);
    }

    private HttpClient Client() => factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
    private async Task<HttpClient> AdminClient() { var client = Client(); var token = await LoginAdmin(client); Bearer(client, token.AccessToken); return client; }
    private async Task<TokenResponse> LoginAdmin(HttpClient client) { client.DefaultRequestHeaders.Authorization = null; using var response = await client.PostAsJsonAsync("/api/identity/auth/login", new LoginRequest(factory.AdministratorEmail, factory.AdministratorPassword)); Assert.Equal(HttpStatusCode.OK, response.StatusCode); return Assert.IsType<TokenResponse>(await response.Content.ReadFromJsonAsync<TokenResponse>()); }
    private static void Bearer(HttpClient client, string token) => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    private static async Task<T> Post<T>(HttpClient client, string uri, object request) { using var response = await client.PostAsJsonAsync(uri, request); Assert.Equal(HttpStatusCode.OK, response.StatusCode); return Assert.IsType<T>(await response.Content.ReadFromJsonAsync<T>()); }
    private sealed class KitchenSetup(SalesOrderResponse order, CatalogItemResponse menuItem, Guid ingredientId)
    {
        public SalesOrderResponse Order { get; set; } = order;
        public CatalogItemResponse MenuItem { get; } = menuItem;
        public Guid IngredientId { get; } = ingredientId;
    }
}
