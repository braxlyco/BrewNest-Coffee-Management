using BrewNest.Modules.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewNest.Modules.Payments.Infrastructure.EntityConfigurations;

internal static class PaymentConfiguration
{
    public static void Document<T>(EntityTypeBuilder<T> builder, string table) where T : PaymentDocument
    {
        builder.ToTable(table);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Number).HasMaxLength(40);
        builder.HasIndex(x => x.Number).IsUnique();
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.ModifiedBy).HasMaxLength(200);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class PaymentEntityConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        PaymentConfiguration.Document(builder, "Payments");
        builder.Property(x => x.OrderNumber).HasMaxLength(40);
        builder.Property(x => x.BillNumber).HasMaxLength(40);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Method).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.PayerReference).HasMaxLength(150);
        builder.Property(x => x.GatewayReference).HasMaxLength(150);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.VerifiedBy).HasMaxLength(200);
        builder.Property(x => x.SettledBy).HasMaxLength(200);
        builder.Property(x => x.SettlementReference).HasMaxLength(150);
        builder.HasIndex(x => new { x.SalesOrderId, x.BillNumber });
    }
}

internal sealed class RefundEntityConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        PaymentConfiguration.Document(builder, "Refunds");
        builder.HasIndex(x => x.SalesOrderId);
        builder.Property(x => x.SourceReference).HasMaxLength(100);
        builder.HasIndex(x => x.SourceReference).IsUnique();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.GatewayReference).HasMaxLength(150);
        builder.Property(x => x.ProcessedBy).HasMaxLength(200);
        builder.Property(x => x.Remarks).HasMaxLength(500);
        builder.HasOne<Payment>().WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PaymentHistoryConfiguration : IEntityTypeConfiguration<PaymentStatusHistory>
{
    public void Configure(EntityTypeBuilder<PaymentStatusHistory> builder)
    {
        builder.ToTable("StatusHistory");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).HasMaxLength(30);
        builder.Property(x => x.PreviousStatus).HasMaxLength(30);
        builder.Property(x => x.NewStatus).HasMaxLength(30);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.ChangedBy).HasMaxLength(200);
        builder.HasIndex(x => new { x.DocumentType, x.DocumentId, x.ChangedAt });
    }
}
