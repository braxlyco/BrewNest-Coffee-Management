using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrewNest.Contracts.Identity;
using BrewNest.Contracts.Inventory;
using BrewNest.Contracts.Sales;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using BrewNest.Modules.Inventory.Infrastructure.Persistence;
using BrewNest.Modules.Sales.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.IntegrationTests;

/// <summary>Checks cross-module input validation at the HTTP boundary.</summary>
public sealed class OperationalValidationEndpointTests : IClassFixture<IdentityWebApplicationFactory>, IAsyncLifetime
{
    private readonly IdentityWebApplicationFactory factory;
    /// <summary>Creates a test suite with isolated in-memory module stores.</summary>
    public OperationalValidationEndpointTests(IdentityWebApplicationFactory factory) => this.factory = factory;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<SalesDbContext>().Database.EnsureCreatedAsync();
    }

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Phone validation rejects local numbers and malformed country codes.</summary>
    [Theory]
    [InlineData("03001234567")]
    [InlineData("+00012345678")]
    [InlineData("+92300abc1234")]
    [InlineData("+9230012345678901")]
    public async Task DeliveryOrderRejectsInvalidPhone(string phone)
    {
        using var client = await AdminClient();
        using var response = await client.PostAsJsonAsync("/api/sales/orders",
            new CreateSalesOrderRequest(SalesOrderType.Delivery, null, null, "Guest", phone, "Lahore", ""));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>Receipts cannot introduce identifiers absent from active Catalog records.</summary>
    [Fact]
    public async Task ReceiptRejectsUnknownCatalogReferences()
    {
        using var client = await AdminClient();
        using var response = await client.PostAsJsonAsync("/api/inventory/receipts",
            new GoodsReceiptRequest(Guid.NewGuid(), Guid.NewGuid(), "UNKNOWN-LOT", null,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), 1m, "Test", "UNKNOWN-REF", ""));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private async Task<HttpClient> AdminClient()
    {
        var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        using var response = await client.PostAsJsonAsync("/api/identity/auth/login", new LoginRequest(factory.AdministratorEmail, factory.AdministratorPassword));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = Assert.IsType<TokenResponse>(await response.Content.ReadFromJsonAsync<TokenResponse>());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        return client;
    }
}
