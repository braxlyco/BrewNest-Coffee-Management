using BrewNest.Modules.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Sales.Infrastructure.Persistence;

/// <summary>Owns Sales orders, lines, discounts, bills, and status history.</summary>
public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options):DbContext(options)
{
    internal DbSet<SalesOrder> Orders=>Set<SalesOrder>(); internal DbSet<SalesOrderLine> OrderLines=>Set<SalesOrderLine>(); internal DbSet<SalesDiscount> Discounts=>Set<SalesDiscount>(); internal DbSet<SalesBill> Bills=>Set<SalesBill>(); internal DbSet<SalesStatusHistory> StatusHistory=>Set<SalesStatusHistory>();
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder){modelBuilder.HasDefaultSchema("sales");modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);if(Database.ProviderName=="Microsoft.EntityFrameworkCore.InMemory"){modelBuilder.Entity<SalesOrder>().Property(x=>x.RowVersion).IsConcurrencyToken(false).ValueGeneratedNever();modelBuilder.Entity<SalesOrderLine>().Property(x=>x.RowVersion).IsConcurrencyToken(false).ValueGeneratedNever();modelBuilder.Entity<SalesDiscount>().Property(x=>x.RowVersion).IsConcurrencyToken(false).ValueGeneratedNever();}base.OnModelCreating(modelBuilder);}
}
