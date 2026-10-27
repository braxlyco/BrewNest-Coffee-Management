using BrewNest.Modules.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace BrewNest.Modules.Inventory.Infrastructure.Persistence;
/// <summary>Owns Inventory lot balances, reservations, and append-only movements.</summary>
public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options):DbContext(options)
{
    internal DbSet<StockLot> StockLots=>Set<StockLot>(); internal DbSet<StockReservation> Reservations=>Set<StockReservation>(); internal DbSet<StockMovement> Movements=>Set<StockMovement>();
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder){modelBuilder.HasDefaultSchema("inventory");modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);base.OnModelCreating(modelBuilder);}
}
