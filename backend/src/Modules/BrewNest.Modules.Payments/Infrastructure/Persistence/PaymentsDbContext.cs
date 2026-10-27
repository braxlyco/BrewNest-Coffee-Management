using BrewNest.Modules.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Payments.Infrastructure.Persistence;

/// <summary>Owns Payments persistence in the payments schema.</summary>
public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    internal DbSet<Payment> Payments => Set<Payment>();
    internal DbSet<Refund> Refunds => Set<Refund>();
    internal DbSet<PaymentStatusHistory> StatusHistory => Set<PaymentStatusHistory>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("payments");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
