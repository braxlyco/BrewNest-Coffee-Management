using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrewNest.Contracts.Administration;
using BrewNest.Contracts.Catalog;
using BrewNest.Contracts.Identity;
using BrewNest.Modules.Administration.Infrastructure.Persistence;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.IntegrationTests;

/// <summary>Verifies phone validation through authenticated master-data endpoints.</summary>
public sealed class MasterDataPhoneValidationTests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly IdentityWebApplicationFactory factory;

    /// <summary>Creates the endpoint test fixture.</summary>
    public MasterDataPhoneValidationTests(IdentityWebApplicationFactory factory) => this.factory = factory;

    /// <summary>Rejects local or malformed numbers at the API boundary.</summary>
    [Fact]
    public async Task CatalogAndCompanyRejectPhoneWithoutCountryCode()
    {
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.EnsureCreatedAsync();
            await scope.ServiceProvider.GetRequiredService<AdministrationDbContext>().Database.EnsureCreatedAsync();
        }

        using var login = await client.PostAsJsonAsync("/api/identity/auth/login",
            new LoginRequest(factory.AdministratorEmail, factory.AdministratorPassword));
        var tokens = await login.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokens);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var customer = await client.PostAsJsonAsync("/api/catalog/customers",
            new CreateCustomerRequest("PHONE-CUSTOMER", "Customer", "03001234567", "", ""));
        using var vendor = await client.PostAsJsonAsync("/api/catalog/vendors",
            new CreateVendorRequest("PHONE-VENDOR", "Vendor", "Contact", "+92 3001234567", "", "", ""));
        using var rider = await client.PostAsJsonAsync("/api/catalog/riders",
            new CreateRiderRequest("PHONE-RIDER", "Rider", "+92300", "CAR-1", true));

        Assert.Equal(HttpStatusCode.BadRequest, customer.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, vendor.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, rider.StatusCode);

        var company = await client.GetFromJsonAsync<CompanySettingsResponse>("/api/administration/company");
        Assert.NotNull(company);
        using var update = await client.PutAsJsonAsync("/api/administration/company",
            new UpdateCompanySettingsRequest(company.LegalName, company.TradingName, company.TaxRegistrationNumber,
                company.CurrencyCode, company.TimeZoneId, company.Address, "03001234567", company.Email, company.Version));
        Assert.Equal(HttpStatusCode.BadRequest, update.StatusCode);
    }
}
