using BrewNest.Modules.Production.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Production.Infrastructure.Persistence;

/// <summary>Owns production plans, orders, material requirements, and status history.</summary>
public sealed class ProductionDbContext(DbContextOptions<ProductionDbContext> options) : DbContext(options)
{
    internal DbSet<ProductionPlan> Plans => Set<ProductionPlan>(); internal DbSet<ProductionPlanLine> PlanLines => Set<ProductionPlanLine>(); internal DbSet<ProductionOrder> Orders => Set<ProductionOrder>(); internal DbSet<ProductionMaterialRequirement> Requirements => Set<ProductionMaterialRequirement>(); internal DbSet<ProductionStatusHistory> StatusHistory => Set<ProductionStatusHistory>();
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder) { modelBuilder.HasDefaultSchema("production"); modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductionDbContext).Assembly); if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory") { modelBuilder.Entity<ProductionPlan>().Property(x => x.RowVersion).IsConcurrencyToken(false).ValueGeneratedNever(); modelBuilder.Entity<ProductionOrder>().Property(x => x.RowVersion).IsConcurrencyToken(false).ValueGeneratedNever(); modelBuilder.Entity<ProductionMaterialRequirement>().Property(x => x.RowVersion).IsConcurrencyToken(false).ValueGeneratedNever(); } base.OnModelCreating(modelBuilder); }
}
