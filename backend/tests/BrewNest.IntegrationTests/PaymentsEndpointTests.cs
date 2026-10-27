using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrewNest.Contracts.Identity;
using BrewNest.Contracts.Payments;
using BrewNest.Modules.Payments.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.IntegrationTests;

/// <summary>Exercises Payments endpoint security and Problem Details.</summary>
public sealed class PaymentsEndpointTests : IClassFixture<IdentityWebApplicationFactory>, IAsyncLifetime
{
    private readonly IdentityWebApplicationFactory factory;
    /// <summary>Creates the test suite.</summary>
    public PaymentsEndpointTests(IdentityWebApplicationFactory factory) => this.factory = factory;
    /// <inheritdoc />
    public async Task InitializeAsync() { await using var scope = factory.Services.CreateAsyncScope(); await scope.ServiceProvider.GetRequiredService<PaymentsDbContext>().Database.EnsureCreatedAsync(); }
    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Anonymous access is rejected.</summary>
    [Fact]
    public async Task PaymentsRequireAuthentication() { using var client = Client(); using var response = await client.GetAsync("/api/payments/"); Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); }

    /// <summary>An authenticated user still needs explicit permission.</summary>
    [Fact]
    public async Task UserWithoutPermissionIsForbidden()
    {
        using var client = Client(); var admin = await Login(client, factory.AdministratorEmail, factory.AdministratorPassword); Bearer(client, admin.AccessToken);
        var email = $"payment-{Guid.NewGuid():N}@test.local"; var password = IdentityWebApplicationFactory.CreateValidPassword();
        using var created = await client.PostAsJsonAsync("/api/identity/users", new CreateUserRequest(email, "Denied", password, [])); Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var user = await Login(client, email, password); Bearer(client, user.AccessToken);
        using var denied = await client.GetAsync("/api/payments/"); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
    }

    /// <summary>Invalid payment input produces Problem Details.</summary>
    [Fact]
    public async Task InvalidPaymentUsesProblemDetails()
    {
        using var client = Client(); var admin = await Login(client, factory.AdministratorEmail, factory.AdministratorPassword); Bearer(client, admin.AccessToken);
        using var response = await client.PostAsJsonAsync("/api/payments/", new CreatePaymentRequest(Guid.Empty, "", 0, PaymentMethod.Cash, "", ""));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private HttpClient Client() => factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
    private static void Bearer(HttpClient client, string token) => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    private static async Task<TokenResponse> Login(HttpClient client, string email, string password) { client.DefaultRequestHeaders.Authorization = null; using var response = await client.PostAsJsonAsync("/api/identity/auth/login", new LoginRequest(email, password)); Assert.Equal(HttpStatusCode.OK, response.StatusCode); return Assert.IsType<TokenResponse>(await response.Content.ReadFromJsonAsync<TokenResponse>()); }
}
