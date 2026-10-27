using BrewNest.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewNest.Modules.Catalog.Infrastructure.EntityConfigurations;

internal abstract class MasterDataConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : AuditedMasterData
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.ModifiedBy).HasMaxLength(100);
        builder.Property(entity => entity.RowVersion).IsRowVersion();
    }

    protected static void ConfigureCodeName(EntityTypeBuilder<TEntity> builder, string codeProperty, string nameProperty)
    {
        builder.Property(codeProperty).HasMaxLength(30).IsRequired();
        builder.Property(nameProperty).HasMaxLength(200).IsRequired();
        builder.HasIndex(codeProperty).IsUnique();
    }
}

internal sealed class UnitOfMeasureConfiguration : MasterDataConfiguration<UnitOfMeasure>
{
    public override void Configure(EntityTypeBuilder<UnitOfMeasure> builder) { base.Configure(builder); builder.ToTable("UnitsOfMeasure"); ConfigureCodeName(builder, nameof(UnitOfMeasure.Code), nameof(UnitOfMeasure.Name)); builder.Property(x => x.Symbol).HasMaxLength(20).IsRequired(); builder.Property(x => x.Category).HasMaxLength(50).IsRequired(); }
}
internal sealed class ItemGroupConfiguration : MasterDataConfiguration<ItemGroup>
{
    public override void Configure(EntityTypeBuilder<ItemGroup> builder) { base.Configure(builder); builder.ToTable("ItemGroups"); ConfigureCodeName(builder, nameof(ItemGroup.Code), nameof(ItemGroup.Name)); builder.Property(x => x.AppliesTo).HasConversion<string>().HasMaxLength(30); }
}
internal sealed class CatalogItemConfiguration : MasterDataConfiguration<CatalogItem>
{
    public override void Configure(EntityTypeBuilder<CatalogItem> builder) { base.Configure(builder); builder.ToTable("Items"); ConfigureCodeName(builder, nameof(CatalogItem.Code), nameof(CatalogItem.Name)); builder.Property(x => x.Description).HasMaxLength(1000); builder.Property(x => x.ItemType).HasConversion<string>().HasMaxLength(30).IsRequired(); builder.Property(x => x.SalesPrice).HasPrecision(18, 2); builder.Property(x => x.StandardCost).HasPrecision(18, 2); builder.HasOne(x => x.UnitOfMeasure).WithMany().HasForeignKey(x => x.UnitOfMeasureId).OnDelete(DeleteBehavior.Restrict); builder.HasOne(x => x.ItemGroup).WithMany().HasForeignKey(x => x.ItemGroupId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class CustomerConfiguration : MasterDataConfiguration<Customer>
{
    public override void Configure(EntityTypeBuilder<Customer> builder) { base.Configure(builder); builder.ToTable("Customers"); ConfigureParty(builder); }
    private static void ConfigureParty(EntityTypeBuilder<Customer> builder) { ConfigureCodeName(builder, nameof(Customer.Code), nameof(Customer.Name)); builder.Property(x => x.Phone).HasMaxLength(50); builder.Property(x => x.Email).HasMaxLength(256); builder.Property(x => x.Address).HasMaxLength(500); }
}
internal sealed class VendorConfiguration : MasterDataConfiguration<Vendor>
{
    public override void Configure(EntityTypeBuilder<Vendor> builder) { base.Configure(builder); builder.ToTable("Vendors"); ConfigureCodeName(builder, nameof(Vendor.Code), nameof(Vendor.Name)); builder.Property(x => x.ContactPerson).HasMaxLength(200); builder.Property(x => x.Phone).HasMaxLength(50); builder.Property(x => x.Email).HasMaxLength(256); builder.Property(x => x.Address).HasMaxLength(500); builder.Property(x => x.TaxRegistrationNumber).HasMaxLength(100); }
}
internal sealed class DiningTableConfiguration : MasterDataConfiguration<DiningTable>
{
    public override void Configure(EntityTypeBuilder<DiningTable> builder) { base.Configure(builder); builder.ToTable("DiningTables"); ConfigureCodeName(builder, nameof(DiningTable.Code), nameof(DiningTable.Name)); builder.Property(x => x.Area).HasMaxLength(100); }
}
internal sealed class WarehouseConfiguration : MasterDataConfiguration<Warehouse>
{
    public override void Configure(EntityTypeBuilder<Warehouse> builder) { base.Configure(builder); builder.ToTable("Warehouses"); ConfigureCodeName(builder, nameof(Warehouse.Code), nameof(Warehouse.Name)); builder.Property(x => x.Address).HasMaxLength(500); }
}
internal sealed class RiderConfiguration : MasterDataConfiguration<Rider>
{
    public override void Configure(EntityTypeBuilder<Rider> builder) { base.Configure(builder); builder.ToTable("Riders"); ConfigureCodeName(builder, nameof(Rider.Code), nameof(Rider.Name)); builder.Property(x => x.Phone).HasMaxLength(50); builder.Property(x => x.VehicleRegistration).HasMaxLength(50); }
}
internal sealed class RecipeConfiguration : MasterDataConfiguration<Recipe>
{
    public override void Configure(EntityTypeBuilder<Recipe> builder) { base.Configure(builder); builder.ToTable("Recipes"); ConfigureCodeName(builder, nameof(Recipe.Code), nameof(Recipe.Name)); builder.Property(x => x.YieldQuantity).HasPrecision(18, 3); builder.HasIndex(x => x.MenuItemId).HasFilter("[IsActive] = 1").IsUnique(); builder.HasOne(x => x.MenuItem).WithMany().HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.Restrict); builder.HasOne(x => x.YieldUnitOfMeasure).WithMany().HasForeignKey(x => x.YieldUnitOfMeasureId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class RecipeLineConfiguration : IEntityTypeConfiguration<RecipeLine>
{
    public void Configure(EntityTypeBuilder<RecipeLine> builder) { builder.ToTable("RecipeLines"); builder.HasKey(x => x.Id); builder.Property(x => x.Quantity).HasPrecision(18, 3); builder.Property(x => x.WastagePercentage).HasPrecision(5, 2); builder.Property(x => x.AddedBy).HasMaxLength(100).IsRequired(); builder.Property(x => x.RevokedBy).HasMaxLength(100); builder.Property(x => x.RowVersion).IsRowVersion(); builder.HasIndex(x => new { x.RecipeId, x.Sequence }).HasFilter("[RevokedAt] IS NULL").IsUnique(); builder.HasOne(x => x.Recipe).WithMany(x => x.Lines).HasForeignKey(x => x.RecipeId).OnDelete(DeleteBehavior.Restrict); builder.HasOne(x => x.IngredientItem).WithMany().HasForeignKey(x => x.IngredientItemId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class InspectionRequirementConfiguration : MasterDataConfiguration<InspectionRequirement>
{
    public override void Configure(EntityTypeBuilder<InspectionRequirement> builder) { base.Configure(builder); builder.ToTable("InspectionRequirements"); builder.Property(x => x.Name).HasMaxLength(200).IsRequired(); builder.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict); builder.HasOne(x => x.ItemGroup).WithMany().HasForeignKey(x => x.ItemGroupId).OnDelete(DeleteBehavior.Restrict); builder.HasIndex(x => x.ItemId).HasFilter("[ItemId] IS NOT NULL AND [IsActive] = 1").IsUnique(); builder.HasIndex(x => x.ItemGroupId).HasFilter("[ItemGroupId] IS NOT NULL AND [IsActive] = 1").IsUnique(); }
}
internal sealed class InspectionCriterionConfiguration : IEntityTypeConfiguration<InspectionCriterion>
{
    public void Configure(EntityTypeBuilder<InspectionCriterion> builder) { builder.ToTable("InspectionCriteria"); builder.HasKey(x => x.Id); builder.Property(x => x.Name).HasMaxLength(200).IsRequired(); builder.Property(x => x.ResultType).HasConversion<string>().HasMaxLength(20).IsRequired(); builder.Property(x => x.MinimumValue).HasMaxLength(100); builder.Property(x => x.MaximumValue).HasMaxLength(100); builder.Property(x => x.Unit).HasMaxLength(30); builder.Property(x => x.AddedBy).HasMaxLength(100).IsRequired(); builder.Property(x => x.RevokedBy).HasMaxLength(100); builder.Property(x => x.RowVersion).IsRowVersion(); builder.HasIndex(x => new { x.InspectionRequirementId, x.Sequence }).HasFilter("[RevokedAt] IS NULL").IsUnique(); builder.HasOne(x => x.InspectionRequirement).WithMany(x => x.Criteria).HasForeignKey(x => x.InspectionRequirementId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class CatalogStatusHistoryConfiguration : IEntityTypeConfiguration<CatalogStatusHistory>
{
    public void Configure(EntityTypeBuilder<CatalogStatusHistory> builder) { builder.ToTable("StatusHistory"); builder.HasKey(x => x.Id); builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired(); builder.Property(x => x.PreviousStatus).HasMaxLength(30).IsRequired(); builder.Property(x => x.NewStatus).HasMaxLength(30).IsRequired(); builder.Property(x => x.Reason).HasMaxLength(500).IsRequired(); builder.Property(x => x.ChangedBy).HasMaxLength(100).IsRequired(); builder.HasIndex(x => new { x.EntityType, x.EntityId, x.ChangedAt }); }
}
