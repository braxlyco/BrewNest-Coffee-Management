using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrewNest.Contracts.Identity;
using BrewNest.Contracts.Returns;
using BrewNest.Modules.Returns.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.IntegrationTests;

/// <summary>Exercises Returns authorization and API error contracts.</summary>
public sealed class ReturnsEndpointTests : IClassFixture<IdentityWebApplicationFactory>, IAsyncLifetime
{
    private readonly IdentityWebApplicationFactory factory;

    /// <summary>Creates the suite.</summary>
    public ReturnsEndpointTests(IdentityWebApplicationFactory factory) => this.factory = factory;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ReturnsDbContext>().Database.EnsureCreatedAsync();
    }

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Anonymous requests are rejected.</summary>
    [Fact]
    public async Task ReturnsRequiresAuthentication()
    {
        using var client = Client();
        using var response = await client.GetAsync("/api/returns/requests");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>Explicit Returns permission is required.</summary>
    [Fact]
    public async Task UserWithoutPermissionIsForbidden()
    {
        using var client = Client();
        var admin = await Login(client, factory.AdministratorEmail, factory.AdministratorPassword);
        Bearer(client, admin.AccessToken);

        var email = $"returns-{Guid.NewGuid():N}@test.local";
        var password = IdentityWebApplicationFactory.CreateValidPassword();
        using var created = await client.PostAsJsonAsync(
            "/api/identity/users",
            new CreateUserRequest(email, "Denied", password, []));
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);

        var user = await Login(client, email, password);
        Bearer(client, user.AccessToken);
        using var denied = await client.GetAsync("/api/returns/requests");
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
    }

    /// <summary>Validation errors use Problem Details.</summary>
    [Fact]
    public async Task InvalidReturnUsesProblemDetails()
    {
        using var client = Client();
        var token = await Login(client, factory.AdministratorEmail, factory.AdministratorPassword);
        Bearer(client, token.AccessToken);

        using var response = await client.PostAsJsonAsync(
            "/api/returns/requests",
            new CreateReturnRequest(string.Empty, string.Empty, []));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private HttpClient Client() => factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

    private static void Bearer(HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private static async Task<TokenResponse> Login(HttpClient client, string email, string password)
    {
        client.DefaultRequestHeaders.Authorization = null;
        using var response = await client.PostAsJsonAsync(
            "/api/identity/auth/login",
            new LoginRequest(email, password));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return Assert.IsType<TokenResponse>(await response.Content.ReadFromJsonAsync<TokenResponse>());
    }
}
