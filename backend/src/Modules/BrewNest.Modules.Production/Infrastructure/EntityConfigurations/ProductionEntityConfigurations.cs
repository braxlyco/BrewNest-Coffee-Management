using BrewNest.Modules.Production.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewNest.Modules.Production.Infrastructure.EntityConfigurations;

internal sealed class ProductionPlanConfiguration : IEntityTypeConfiguration<ProductionPlan>
{
    public void Configure(EntityTypeBuilder<ProductionPlan> b) { b.ToTable("Plans"); b.HasKey(x => x.Id); b.Property(x => x.PlanNumber).HasMaxLength(40).IsRequired(); b.HasIndex(x => x.PlanNumber).IsUnique(); b.Property(x => x.Name).HasMaxLength(200).IsRequired(); b.Property(x => x.Notes).HasMaxLength(1000).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20); b.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired(); b.Property(x => x.ModifiedBy).HasMaxLength(100); b.Property(x => x.RowVersion).IsRowVersion(); b.HasIndex(x => new { x.Status, x.PlannedDate }); }
}
internal sealed class ProductionPlanLineConfiguration : IEntityTypeConfiguration<ProductionPlanLine>
{
    public void Configure(EntityTypeBuilder<ProductionPlanLine> b) { b.ToTable("PlanLines", t => t.HasCheckConstraint("CK_ProductionPlanLines_Quantity", "[Quantity] > 0")); b.HasKey(x => x.Id); b.Property(x => x.Quantity).HasPrecision(18, 3); b.HasIndex(x => new { x.PlanId, x.MenuItemId }).IsUnique(); b.HasOne(x => x.Plan).WithMany(x => x.Lines).HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class ProductionOrderConfiguration : IEntityTypeConfiguration<ProductionOrder>
{
    public void Configure(EntityTypeBuilder<ProductionOrder> b) { b.ToTable("Orders", t => t.HasCheckConstraint("CK_ProductionOrders_PlannedQuantity", "[PlannedQuantity] > 0")); b.HasKey(x => x.Id); b.Property(x => x.OrderNumber).HasMaxLength(40).IsRequired(); b.HasIndex(x => x.OrderNumber).IsUnique(); b.HasIndex(x => x.PlanLineId).IsUnique(); b.Property(x => x.PlannedQuantity).HasPrecision(18, 3); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20); b.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired(); b.Property(x => x.ModifiedBy).HasMaxLength(100); b.Property(x => x.RowVersion).IsRowVersion(); b.HasOne(x => x.Plan).WithMany(x => x.Orders).HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class ProductionMaterialRequirementConfiguration : IEntityTypeConfiguration<ProductionMaterialRequirement>
{
    public void Configure(EntityTypeBuilder<ProductionMaterialRequirement> b) { b.ToTable("MaterialRequirements", t => { t.HasCheckConstraint("CK_ProductionRequirements_Quantities", "[BomQuantity] >= 0 AND [ExpectedWastage] >= 0 AND [RequiredQuantity] > 0 AND [ActualConsumption] >= 0 AND [ActualWastage] >= 0"); }); b.HasKey(x => x.Id); b.Property(x => x.BomQuantity).HasPrecision(18, 3); b.Property(x => x.ExpectedWastage).HasPrecision(18, 3); b.Property(x => x.RequiredQuantity).HasPrecision(18, 3); b.Property(x => x.ActualConsumption).HasPrecision(18, 3); b.Property(x => x.ActualWastage).HasPrecision(18, 3); b.Property(x => x.Variance).HasPrecision(18, 3); b.Property(x => x.RowVersion).IsRowVersion(); b.HasIndex(x => new { x.OrderId, x.IngredientItemId }).IsUnique(); b.HasOne(x => x.Order).WithMany(x => x.Requirements).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class ProductionStatusHistoryConfiguration : IEntityTypeConfiguration<ProductionStatusHistory>
{
    public void Configure(EntityTypeBuilder<ProductionStatusHistory> b) { b.ToTable("StatusHistory"); b.HasKey(x => x.Id); b.Property(x => x.DocumentType).HasMaxLength(30).IsRequired(); b.Property(x => x.PreviousStatus).HasMaxLength(20).IsRequired(); b.Property(x => x.NewStatus).HasMaxLength(20).IsRequired(); b.Property(x => x.Reason).HasMaxLength(500).IsRequired(); b.Property(x => x.ChangedBy).HasMaxLength(100).IsRequired(); b.HasIndex(x => new { x.DocumentType, x.DocumentId, x.ChangedAt }); }
}
