using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using BrewNest.Contracts.Identity;

namespace BrewNest.IntegrationTests;

/// <summary>Exercises Identity authentication, authorization, and account lifecycle behavior.</summary>
public sealed class IdentityEndpointTests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly IdentityWebApplicationFactory factory;

    /// <summary>Creates the endpoint test suite.</summary>
    public IdentityEndpointTests(IdentityWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>Malformed JSON is a client problem, not an internal server error.</summary>
    [Fact]
    public async Task MalformedLoginJsonReturnsProblemDetailsBadRequest()
    {
        using var client = CreateClient();
        using var body = new StringContent("{\"email\":", Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/identity/auth/login", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>Confirms login issues a valid JWT and altered tokens are rejected.</summary>
    [Fact]
    public async Task LoginIssuesJwtAcceptedByCurrentUserEndpoint()
    {
        using var client = CreateClient();
        var tokens = await LoginAdministratorAsync(client);
        UseBearer(client, tokens.AccessToken);

        using var meResponse = await client.GetAsync("/api/identity/auth/me");
        var currentUser = await meResponse.Content.ReadFromJsonAsync<CurrentUserResponse>();

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.NotNull(currentUser);
        Assert.Equal(factory.AdministratorEmail, currentUser.Email);

        UseBearer(client, string.Concat(tokens.AccessToken.AsSpan(0, tokens.AccessToken.Length - 1), "x"));
        using var alteredResponse = await client.GetAsync("/api/identity/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, alteredResponse.StatusCode);
    }

    /// <summary>Confirms refresh tokens rotate and replay revokes the entire token family.</summary>
    [Fact]
    public async Task RefreshTokenRotationRejectsReplayAndRevokesFamily()
    {
        using var client = CreateClient();
        var original = await LoginAdministratorAsync(client);

        using var rotateResponse = await client.PostAsJsonAsync(
            "/api/identity/auth/refresh",
            new RefreshTokenRequest(original.RefreshToken));
        var replacement = await rotateResponse.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.Equal(HttpStatusCode.OK, rotateResponse.StatusCode);
        Assert.NotNull(replacement);
        Assert.NotEqual(original.RefreshToken, replacement.RefreshToken);

        using var replayResponse = await client.PostAsJsonAsync(
            "/api/identity/auth/refresh",
            new RefreshTokenRequest(original.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, replayResponse.StatusCode);

        using var familyResponse = await client.PostAsJsonAsync(
            "/api/identity/auth/refresh",
            new RefreshTokenRequest(replacement.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, familyResponse.StatusCode);
    }

    /// <summary>Confirms logout revokes the supplied refresh token.</summary>
    [Fact]
    public async Task LogoutRevokesRefreshToken()
    {
        using var client = CreateClient();
        var tokens = await LoginAdministratorAsync(client);

        using var logoutResponse = await client.PostAsJsonAsync(
            "/api/identity/auth/logout",
            new LogoutRequest(tokens.RefreshToken));
        using var refreshResponse = await client.PostAsJsonAsync(
            "/api/identity/auth/refresh",
            new RefreshTokenRequest(tokens.RefreshToken));

        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    /// <summary>Confirms password policy failures and duplicate email conflicts use Problem Details statuses.</summary>
    [Fact]
    public async Task UserCreationEnforcesPasswordRulesAndDuplicateEmails()
    {
        using var client = CreateClient();
        var administrator = await LoginAdministratorAsync(client);
        UseBearer(client, administrator.AccessToken);
        var email = $"user-{Guid.NewGuid():N}@test.brewnest.local";

        using var weakResponse = await client.PostAsJsonAsync(
            "/api/identity/users",
            new CreateUserRequest(email, "Password Policy Test", "weak", []));
        Assert.Equal(HttpStatusCode.BadRequest, weakResponse.StatusCode);

        var request = new CreateUserRequest(email, "Duplicate User Test", IdentityWebApplicationFactory.CreateValidPassword(), []);
        using var createdResponse = await client.PostAsJsonAsync("/api/identity/users", request);
        using var duplicateResponse = await client.PostAsJsonAsync("/api/identity/users", request);

        Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    /// <summary>Confirms disabled accounts cannot authenticate.</summary>
    [Fact]
    public async Task DisabledUserCannotLogin()
    {
        using var client = CreateClient();
        var administrator = await LoginAdministratorAsync(client);
        UseBearer(client, administrator.AccessToken);
        var email = $"disabled-{Guid.NewGuid():N}@test.brewnest.local";
        var password = IdentityWebApplicationFactory.CreateValidPassword();
        using var createResponse = await client.PostAsJsonAsync(
            "/api/identity/users",
            new CreateUserRequest(email, "Disabled User", password, []));
        var created = await createResponse.Content.ReadFromJsonAsync<UserSummaryResponse>();
        Assert.NotNull(created);

        using var disableResponse = await client.PutAsJsonAsync(
            $"/api/identity/users/{created.Id}",
            new UpdateUserRequest(created.Email, created.FullName, false, created.Version));
        Assert.Equal(HttpStatusCode.OK, disableResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        using var loginResponse = await client.PostAsJsonAsync(
            "/api/identity/auth/login",
            new LoginRequest(email, password));
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    /// <summary>Confirms both authentication and explicit permission policies protect management APIs.</summary>
    [Fact]
    public async Task UserManagementRequiresExplicitPermission()
    {
        using var client = CreateClient();
        using var anonymousResponse = await client.GetAsync("/api/identity/users");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        var administrator = await LoginAdministratorAsync(client);
        UseBearer(client, administrator.AccessToken);
        var email = $"unprivileged-{Guid.NewGuid():N}@test.brewnest.local";
        var password = IdentityWebApplicationFactory.CreateValidPassword();
        using var createResponse = await client.PostAsJsonAsync(
            "/api/identity/users",
            new CreateUserRequest(email, "Unprivileged User", password, []));
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        using var loginResponse = await client.PostAsJsonAsync(
            "/api/identity/auth/login",
            new LoginRequest(email, password));
        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokens);
        UseBearer(client, tokens.AccessToken);

        using var forbiddenResponse = await client.GetAsync("/api/identity/users");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
    }

    private HttpClient CreateClient() => factory.CreateClient(new()
    {
        BaseAddress = new Uri("https://localhost")
    });

    private async Task<TokenResponse> LoginAdministratorAsync(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
        using var response = await client.PostAsJsonAsync(
            "/api/identity/auth/login",
            new LoginRequest(factory.AdministratorEmail, factory.AdministratorPassword));
        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return Assert.IsType<TokenResponse>(tokens);
    }

    private static void UseBearer(HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
