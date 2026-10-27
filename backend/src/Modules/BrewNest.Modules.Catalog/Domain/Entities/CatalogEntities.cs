namespace BrewNest.Modules.Catalog.Domain.Entities;

internal abstract class AuditedMasterData
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

internal enum CatalogItemType { MenuItem, RawMaterial, PackagingMaterial }
internal enum InspectionResultType { PassFail, Numeric, Text }

internal sealed class UnitOfMeasure : AuditedMasterData
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; }
}

internal sealed class ItemGroup : AuditedMasterData
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public CatalogItemType? AppliesTo { get; set; }
    public bool RequiresInspection { get; set; }
}

internal sealed class CatalogItem : AuditedMasterData
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CatalogItemType ItemType { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
    public Guid? ItemGroupId { get; set; }
    public ItemGroup? ItemGroup { get; set; }
    public decimal? SalesPrice { get; set; }
    public decimal StandardCost { get; set; }
    public int? ShelfLifeDays { get; set; }
    public bool RequiresInspection { get; set; }
}

internal sealed class Customer : AuditedMasterData
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

internal sealed class Vendor : AuditedMasterData
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxRegistrationNumber { get; set; } = string.Empty;
}

internal sealed class DiningTable : AuditedMasterData
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Area { get; set; } = string.Empty;
}

internal sealed class Warehouse : AuditedMasterData
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

internal sealed class Rider : AuditedMasterData
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string VehicleRegistration { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

internal sealed class Recipe : AuditedMasterData
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid MenuItemId { get; set; }
    public CatalogItem MenuItem { get; set; } = null!;
    public decimal YieldQuantity { get; set; }
    public Guid YieldUnitOfMeasureId { get; set; }
    public UnitOfMeasure YieldUnitOfMeasure { get; set; } = null!;
    public List<RecipeLine> Lines { get; set; } = [];
}

internal sealed class RecipeLine
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public Guid IngredientItemId { get; set; }
    public CatalogItem IngredientItem { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal WastagePercentage { get; set; }
    public int Sequence { get; set; }
    public DateTimeOffset AddedAt { get; set; }
    public string AddedBy { get; set; } = string.Empty;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

internal sealed class InspectionRequirement : AuditedMasterData
{
    public Guid? ItemId { get; set; }
    public CatalogItem? Item { get; set; }
    public Guid? ItemGroupId { get; set; }
    public ItemGroup? ItemGroup { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public List<InspectionCriterion> Criteria { get; set; } = [];
}

internal sealed class InspectionCriterion
{
    public Guid Id { get; set; }
    public Guid InspectionRequirementId { get; set; }
    public InspectionRequirement InspectionRequirement { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public InspectionResultType ResultType { get; set; }
    public string? MinimumValue { get; set; }
    public string? MaximumValue { get; set; }
    public string? Unit { get; set; }
    public bool IsMandatory { get; set; }
    public int Sequence { get; set; }
    public DateTimeOffset AddedAt { get; set; }
    public string AddedBy { get; set; } = string.Empty;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

internal sealed class CatalogStatusHistory
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; set; }
}
