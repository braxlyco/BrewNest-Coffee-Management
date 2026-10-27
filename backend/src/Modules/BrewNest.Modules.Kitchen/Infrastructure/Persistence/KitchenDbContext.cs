using BrewNest.Modules.Kitchen.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Kitchen.Infrastructure.Persistence;

/// <summary>Owns Kitchen tickets, preparation status, reservation links, and history.</summary>
public sealed class KitchenDbContext(DbContextOptions<KitchenDbContext> options) : DbContext(options)
{
    internal DbSet<KitchenTicket> Tickets => Set<KitchenTicket>();
    internal DbSet<KitchenTicketLine> TicketLines => Set<KitchenTicketLine>();
    internal DbSet<KitchenReservationLink> ReservationLinks => Set<KitchenReservationLink>();
    internal DbSet<KitchenStatusHistory> StatusHistory => Set<KitchenStatusHistory>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("kitchen");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KitchenDbContext).Assembly);
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            modelBuilder.Entity<KitchenTicket>().Property(x => x.RowVersion).IsConcurrencyToken(false).ValueGeneratedNever();
            modelBuilder.Entity<KitchenTicketLine>().Property(x => x.RowVersion).IsConcurrencyToken(false).ValueGeneratedNever();
        }
        base.OnModelCreating(modelBuilder);
    }
}
