using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrewNest.Contracts.Administration;
using BrewNest.Contracts.Identity;
using BrewNest.Modules.Administration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.IntegrationTests;

/// <summary>Exercises seeded configuration, authorization, validation, concurrency, and auditing.</summary>
public sealed class AdministrationEndpointTests : IClassFixture<IdentityWebApplicationFactory>, IAsyncLifetime
{
    private readonly IdentityWebApplicationFactory factory;

    /// <summary>Creates the Administration endpoint test suite.</summary>
    public AdministrationEndpointTests(IdentityWebApplicationFactory factory) => this.factory = factory;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AdministrationDbContext>().Database.EnsureCreatedAsync();
    }

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Confirms the required PO approval tiers and expiry thresholds are data-driven defaults.</summary>
    [Fact]
    public async Task DefaultsContainRequiredApprovalTiersAndExpiryThresholds()
    {
        using var client = await CreateAdministratorClientAsync();
        var approvals = await client.GetFromJsonAsync<ApprovalPolicyResponse[]>("/api/administration/approvals");
        var alerts = await client.GetFromJsonAsync<AlertSettingResponse[]>("/api/administration/alerts");

        var purchaseOrders = Assert.IsType<ApprovalPolicyResponse[]>(approvals).Where(item => item.ProcessCode == "PurchaseOrder").ToArray();
        Assert.Equal(3, purchaseOrders.Length);
        Assert.Contains(purchaseOrders, item => item.MaximumAmount == 25_000m && item.RequiredRoles.SequenceEqual(["Purchasing Manager"]));
        Assert.Contains(purchaseOrders, item => item.MinimumAmount == 25_000.01m && item.MaximumAmount == 100_000m && item.RequiredRoles.Contains("Finance Manager"));
        Assert.Contains(purchaseOrders, item => item.MinimumAmount == 100_000.01m && item.RequiredRoles.Contains("Owner/Admin"));
        Assert.Equal([15, 7, 3], Assert.IsType<AlertSettingResponse[]>(alerts).Select(item => item.DaysBefore).ToArray());
    }

    /// <summary>Confirms anonymous and authenticated users without explicit permission are rejected.</summary>
    [Fact]
    public async Task AdministrationRequiresExplicitPermission()
    {
        using var client = CreateClient();
        using var anonymous = await client.GetAsync("/api/administration/company");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var administrator = await LoginAdministratorAsync(client);
        UseBearer(client, administrator.AccessToken);
        var email = $"admin-denied-{Guid.NewGuid():N}@test.brewnest.local";
        var password = IdentityWebApplicationFactory.CreateValidPassword();
        using var created = await client.PostAsJsonAsync("/api/identity/users", new CreateUserRequest(email, "No Administration Access", password, []));
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        client.DefaultRequestHeaders.Authorization = null;
        using var login = await client.PostAsJsonAsync("/api/identity/auth/login", new LoginRequest(email, password));
        var tokens = await login.Content.ReadFromJsonAsync<TokenResponse>();
        UseBearer(client, Assert.IsType<TokenResponse>(tokens).AccessToken);

        using var forbidden = await client.GetAsync("/api/administration/company");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    /// <summary>Confirms validated company updates create an append-only audit entry.</summary>
    [Fact]
    public async Task CompanyUpdateCreatesAuditHistory()
    {
        using var client = await CreateAdministratorClientAsync();
        var company = Assert.IsType<CompanySettingsResponse>(await client.GetFromJsonAsync<CompanySettingsResponse>("/api/administration/company"));
        using var update = await client.PutAsJsonAsync("/api/administration/company", new UpdateCompanySettingsRequest("BrewNest Cafe (Pvt.) Ltd.", "BrewNest", "", "PKR", "Asia/Karachi", "University Road", "", "admin@brewnest.local", company.Version));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var audit = Assert.IsType<AdministrationAuditResponse[]>(await client.GetFromJsonAsync<AdministrationAuditResponse[]>("/api/administration/audit"));
        Assert.Contains(audit, item => item.EntityType == "CompanySettings" && item.Action == "Updated" && item.BeforeJson is not null && item.AfterJson is not null);
    }

    /// <summary>Confirms fiscal-year overlap and invalid expiry ordering return Problem Details validation statuses.</summary>
    [Fact]
    public async Task ConfigurationRulesRejectOverlapAndInvalidAlertOrdering()
    {
        using var client = await CreateAdministratorClientAsync();
        using var first = await client.PostAsJsonAsync("/api/administration/fiscal-years", new CreateFiscalYearRequest("FY 2026", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));
        using var overlap = await client.PostAsJsonAsync("/api/administration/fiscal-years", new CreateFiscalYearRequest("FY overlap", new DateOnly(2026, 7, 1), new DateOnly(2027, 6, 30)));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, overlap.StatusCode);

        var alerts = Assert.IsType<AlertSettingResponse[]>(await client.GetFromJsonAsync<AlertSettingResponse[]>("/api/administration/alerts"));
        var critical = alerts.Single(item => item.Code == "ExpiryCritical");
        using var invalid = await client.PutAsJsonAsync($"/api/administration/alerts/{critical.Id}", new UpdateAlertSettingRequest(8, true, critical.Version));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    private HttpClient CreateClient() => factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

    private async Task<HttpClient> CreateAdministratorClientAsync()
    {
        var client = CreateClient();
        var tokens = await LoginAdministratorAsync(client);
        UseBearer(client, tokens.AccessToken);
        return client;
    }

    private async Task<TokenResponse> LoginAdministratorAsync(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
        using var response = await client.PostAsJsonAsync("/api/identity/auth/login", new LoginRequest(factory.AdministratorEmail, factory.AdministratorPassword));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return Assert.IsType<TokenResponse>(await response.Content.ReadFromJsonAsync<TokenResponse>());
    }

    private static void UseBearer(HttpClient client, string token) => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
