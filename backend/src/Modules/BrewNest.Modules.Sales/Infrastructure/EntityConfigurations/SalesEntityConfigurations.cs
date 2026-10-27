using BrewNest.Modules.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewNest.Modules.Sales.Infrastructure.EntityConfigurations;

internal sealed class SalesOrderConfiguration:IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> b){b.ToTable("Orders");b.HasKey(x=>x.Id);b.Property(x=>x.OrderNumber).HasMaxLength(40).IsRequired();b.HasIndex(x=>x.OrderNumber).IsUnique();b.Property(x=>x.OrderType).HasConversion<string>().HasMaxLength(20);b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(20);b.Property(x=>x.CustomerName).HasMaxLength(200);b.Property(x=>x.CustomerPhone).HasMaxLength(50);b.Property(x=>x.DeliveryAddress).HasMaxLength(500);b.Property(x=>x.Notes).HasMaxLength(1000);b.Property(x=>x.CreatedBy).HasMaxLength(100).IsRequired();b.Property(x=>x.ModifiedBy).HasMaxLength(100);b.Property(x=>x.RowVersion).IsRowVersion();b.HasIndex(x=>new{x.Status,x.CreatedAt});b.HasIndex(x=>x.DiningTableId).IsUnique().HasFilter("[DiningTableId] IS NOT NULL AND [Status] IN ('Draft','Confirmed','Billed')");}
}
internal sealed class SalesOrderLineConfiguration:IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> b){b.ToTable("OrderLines",t=>t.HasCheckConstraint("CK_OrderLines_Quantity_Positive","[Quantity] > 0"));b.HasKey(x=>x.Id);b.Property(x=>x.ItemCode).HasMaxLength(30).IsRequired();b.Property(x=>x.ItemName).HasMaxLength(200).IsRequired();b.Property(x=>x.Quantity).HasPrecision(18,3);b.Property(x=>x.UnitPrice).HasPrecision(18,2);b.Property(x=>x.SpecialInstructions).HasMaxLength(500);b.Property(x=>x.CancellationReason).HasMaxLength(500);b.Property(x=>x.AddedBy).HasMaxLength(100).IsRequired();b.Property(x=>x.ModifiedBy).HasMaxLength(100);b.Property(x=>x.RowVersion).IsRowVersion();b.HasOne(x=>x.Order).WithMany(x=>x.Lines).HasForeignKey(x=>x.OrderId).OnDelete(DeleteBehavior.Restrict);b.HasIndex(x=>new{x.OrderId,x.AddedAt});}
}
internal sealed class SalesDiscountConfiguration:IEntityTypeConfiguration<SalesDiscount>
{
    public void Configure(EntityTypeBuilder<SalesDiscount> b){b.ToTable("Discounts",t=>{t.HasCheckConstraint("CK_Discounts_Percentage","[Percentage] > 0 AND [Percentage] <= 100");t.HasCheckConstraint("CK_Discounts_Amount","[Amount] >= 0");});b.HasKey(x=>x.Id);b.Property(x=>x.Percentage).HasPrecision(5,2);b.Property(x=>x.Amount).HasPrecision(18,2);b.Property(x=>x.Reason).HasMaxLength(500).IsRequired();b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(20);b.Property(x=>x.RequestedBy).HasMaxLength(100).IsRequired();b.Property(x=>x.ApprovedBy).HasMaxLength(100);b.Property(x=>x.DecisionRemarks).HasMaxLength(500);b.Property(x=>x.RowVersion).IsRowVersion();b.HasOne(x=>x.Order).WithMany(x=>x.Discounts).HasForeignKey(x=>x.OrderId).OnDelete(DeleteBehavior.Restrict);b.HasIndex(x=>new{x.OrderId,x.Status});}
}
internal sealed class SalesBillConfiguration:IEntityTypeConfiguration<SalesBill>
{
    public void Configure(EntityTypeBuilder<SalesBill> b){b.ToTable("Bills",t=>{t.HasCheckConstraint("CK_Bills_GrossAmount","[GrossAmount] >= 0");t.HasCheckConstraint("CK_Bills_DiscountAmount","[DiscountAmount] >= 0 AND [DiscountAmount] <= [GrossAmount]");t.HasCheckConstraint("CK_Bills_NetAmount","[NetAmount] = [GrossAmount] - [DiscountAmount]");});b.HasKey(x=>x.Id);b.Property(x=>x.BillNumber).HasMaxLength(40).IsRequired();b.HasIndex(x=>x.BillNumber).IsUnique();b.HasIndex(x=>x.OrderId).IsUnique();b.Property(x=>x.GrossAmount).HasPrecision(18,2);b.Property(x=>x.DiscountAmount).HasPrecision(18,2);b.Property(x=>x.NetAmount).HasPrecision(18,2);b.Property(x=>x.IssuedBy).HasMaxLength(100).IsRequired();b.HasOne(x=>x.Order).WithOne(x=>x.Bill).HasForeignKey<SalesBill>(x=>x.OrderId).OnDelete(DeleteBehavior.Restrict);}
}
internal sealed class SalesStatusHistoryConfiguration:IEntityTypeConfiguration<SalesStatusHistory>
{
    public void Configure(EntityTypeBuilder<SalesStatusHistory> b){b.ToTable("StatusHistory");b.HasKey(x=>x.Id);b.Property(x=>x.PreviousStatus).HasMaxLength(20);b.Property(x=>x.NewStatus).HasMaxLength(20);b.Property(x=>x.Reason).HasMaxLength(500).IsRequired();b.Property(x=>x.ChangedBy).HasMaxLength(100).IsRequired();b.HasIndex(x=>new{x.OrderId,x.ChangedAt});}
}
