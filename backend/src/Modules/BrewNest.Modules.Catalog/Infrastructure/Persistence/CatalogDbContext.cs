using BrewNest.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Catalog.Infrastructure.Persistence;

/// <summary>Owns Catalog master-data, recipe, quality, and status-history persistence.</summary>
public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    internal DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    internal DbSet<ItemGroup> ItemGroups => Set<ItemGroup>();
    internal DbSet<CatalogItem> Items => Set<CatalogItem>();
    internal DbSet<Customer> Customers => Set<Customer>();
    internal DbSet<Vendor> Vendors => Set<Vendor>();
    internal DbSet<DiningTable> DiningTables => Set<DiningTable>();
    internal DbSet<Warehouse> Warehouses => Set<Warehouse>();
    internal DbSet<Rider> Riders => Set<Rider>();
    internal DbSet<Recipe> Recipes => Set<Recipe>();
    internal DbSet<RecipeLine> RecipeLines => Set<RecipeLine>();
    internal DbSet<InspectionRequirement> InspectionRequirements => Set<InspectionRequirement>();
    internal DbSet<InspectionCriterion> InspectionCriteria => Set<InspectionCriterion>();
    internal DbSet<CatalogStatusHistory> StatusHistory => Set<CatalogStatusHistory>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
