using System.Security.Cryptography;
using BrewNest.Modules.Identity.Infrastructure.Persistence;
using BrewNest.Modules.Administration.Infrastructure.Persistence;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using BrewNest.Modules.Inventory.Infrastructure.Persistence;
using BrewNest.Modules.Sales.Infrastructure.Persistence;
using BrewNest.Modules.Kitchen.Infrastructure.Persistence;
using BrewNest.Modules.Production.Infrastructure.Persistence;
using BrewNest.Modules.Procurement.Infrastructure.Persistence;
using BrewNest.Modules.Returns.Infrastructure.Persistence;
using BrewNest.Modules.Payments.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrewNest.IntegrationTests;

/// <summary>Hosts Identity with isolated in-memory persistence and runtime-only test secrets.</summary>
public sealed class IdentityWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"identity-tests-{Guid.NewGuid():N}";
    private readonly string administrationDatabaseName = $"administration-tests-{Guid.NewGuid():N}";
    private readonly string catalogDatabaseName = $"catalog-tests-{Guid.NewGuid():N}";
    private readonly string inventoryDatabaseName = $"inventory-tests-{Guid.NewGuid():N}";
    private readonly string salesDatabaseName = $"sales-tests-{Guid.NewGuid():N}";
    private readonly string kitchenDatabaseName = $"kitchen-tests-{Guid.NewGuid():N}";
    private readonly string productionDatabaseName = $"production-tests-{Guid.NewGuid():N}";
    private readonly string procurementDatabaseName = $"procurement-tests-{Guid.NewGuid():N}";
    private readonly string returnsDatabaseName = $"returns-tests-{Guid.NewGuid():N}";
    private readonly string paymentsDatabaseName = $"payments-tests-{Guid.NewGuid():N}";
    private readonly string signingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    /// <summary>Gets the seeded development administrator email.</summary>
    public string AdministratorEmail { get; } = "identity-admin@test.brewnest.local";

    /// <summary>Gets the randomly generated development administrator password.</summary>
    public string AdministratorPassword { get; } = CreateValidPassword();

    /// <summary>Creates a password satisfying the configured Identity policy.</summary>
    public static string CreateValidPassword() => $"Aa!9{Guid.NewGuid():N}";

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "BrewNest.Tests",
                ["Jwt:Audience"] = "BrewNest.Tests.Client",
                ["Jwt:SigningKey"] = signingKey,
                ["Jwt:AccessTokenMinutes"] = "15",
                ["Jwt:RefreshTokenDays"] = "7",
                ["Identity:DevelopmentSeed:Enabled"] = "true",
                ["Identity:DevelopmentSeed:AdministratorEmail"] = AdministratorEmail,
                ["Identity:DevelopmentSeed:AdministratorPassword"] = AdministratorPassword,
                ["Identity:DevelopmentSeed:AdministratorFullName"] = "Integration Test Administrator"
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<IdentityDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<IdentityDbContext>>();
            services.AddDbContext<IdentityDbContext>(options => options.UseInMemoryDatabase(databaseName));
            services.RemoveAll<DbContextOptions<AdministrationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AdministrationDbContext>>();
            services.AddDbContext<AdministrationDbContext>(options => options.UseInMemoryDatabase(administrationDatabaseName));
            services.RemoveAll<DbContextOptions<CatalogDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<CatalogDbContext>>();
            services.AddDbContext<CatalogDbContext>(options => options.UseInMemoryDatabase(catalogDatabaseName));
            services.RemoveAll<DbContextOptions<InventoryDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<InventoryDbContext>>();
            services.AddDbContext<InventoryDbContext>(options => options.UseInMemoryDatabase(inventoryDatabaseName));
            services.RemoveAll<DbContextOptions<SalesDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<SalesDbContext>>();
            services.AddDbContext<SalesDbContext>(options => options.UseInMemoryDatabase(salesDatabaseName));
            services.RemoveAll<DbContextOptions<KitchenDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<KitchenDbContext>>();
            services.AddDbContext<KitchenDbContext>(options => options.UseInMemoryDatabase(kitchenDatabaseName));
            services.RemoveAll<DbContextOptions<ProductionDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ProductionDbContext>>();
            services.AddDbContext<ProductionDbContext>(options => options.UseInMemoryDatabase(productionDatabaseName));
            services.RemoveAll<DbContextOptions<ProcurementDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ProcurementDbContext>>();
            services.AddDbContext<ProcurementDbContext>(options => options.UseInMemoryDatabase(procurementDatabaseName));
            services.RemoveAll<DbContextOptions<ReturnsDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ReturnsDbContext>>();
            services.AddDbContext<ReturnsDbContext>(options => options.UseInMemoryDatabase(returnsDatabaseName));
            services.RemoveAll<DbContextOptions<PaymentsDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<PaymentsDbContext>>();
            services.AddDbContext<PaymentsDbContext>(options => options.UseInMemoryDatabase(paymentsDatabaseName));
        });
    }
}
