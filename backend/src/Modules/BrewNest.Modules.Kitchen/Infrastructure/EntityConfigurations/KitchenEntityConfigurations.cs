using BrewNest.Modules.Kitchen.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewNest.Modules.Kitchen.Infrastructure.EntityConfigurations;

internal sealed class KitchenTicketConfiguration : IEntityTypeConfiguration<KitchenTicket>
{
    public void Configure(EntityTypeBuilder<KitchenTicket> builder)
    {
        builder.ToTable("Tickets"); builder.HasKey(x => x.Id);
        builder.Property(x => x.TicketNumber).HasMaxLength(40).IsRequired(); builder.HasIndex(x => x.TicketNumber).IsUnique();
        builder.Property(x => x.SalesOrderNumber).HasMaxLength(40).IsRequired(); builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired(); builder.Property(x => x.ModifiedBy).HasMaxLength(100); builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.SalesOrderId, x.Sequence }).IsUnique(); builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}

internal sealed class KitchenTicketLineConfiguration : IEntityTypeConfiguration<KitchenTicketLine>
{
    public void Configure(EntityTypeBuilder<KitchenTicketLine> builder)
    {
        builder.ToTable("TicketLines", table => table.HasCheckConstraint("CK_TicketLines_Quantity_Positive", "[Quantity] > 0")); builder.HasKey(x => x.Id);
        builder.Property(x => x.ItemName).HasMaxLength(200).IsRequired(); builder.Property(x => x.Quantity).HasPrecision(18, 3); builder.Property(x => x.SpecialInstructions).HasMaxLength(500).IsRequired(); builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20); builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Ticket).WithMany(x => x.Lines).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Restrict); builder.HasIndex(x => x.SalesOrderLineId);
    }
}

internal sealed class KitchenReservationLinkConfiguration : IEntityTypeConfiguration<KitchenReservationLink>
{
    public void Configure(EntityTypeBuilder<KitchenReservationLink> builder)
    {
        builder.ToTable("ReservationLinks", table => table.HasCheckConstraint("CK_ReservationLinks_Quantity_Positive", "[Quantity] > 0")); builder.HasKey(x => x.Id); builder.Property(x => x.Quantity).HasPrecision(18, 3); builder.HasIndex(x => x.InventoryReservationId).IsUnique(); builder.HasOne(x => x.Ticket).WithMany(x => x.Reservations).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class KitchenStatusHistoryConfiguration : IEntityTypeConfiguration<KitchenStatusHistory>
{
    public void Configure(EntityTypeBuilder<KitchenStatusHistory> builder)
    {
        builder.ToTable("StatusHistory"); builder.HasKey(x => x.Id); builder.Property(x => x.PreviousStatus).HasMaxLength(20).IsRequired(); builder.Property(x => x.NewStatus).HasMaxLength(20).IsRequired(); builder.Property(x => x.Reason).HasMaxLength(500).IsRequired(); builder.Property(x => x.ChangedBy).HasMaxLength(100).IsRequired(); builder.HasIndex(x => new { x.TicketId, x.ChangedAt });
    }
}
