namespace BrewNest.Contracts.Production;

/// <summary>Production plan lifecycle.</summary>
public enum ProductionPlanStatus
{
    /// <summary>Editable planning state.</summary>
    Draft,
    /// <summary>Orders have been generated.</summary>
    Released,
    /// <summary>Every generated order is complete.</summary>
    Completed,
    /// <summary>Cancelled before release.</summary>
    Cancelled
}

/// <summary>Production order lifecycle.</summary>
public enum ProductionOrderStatus
{
    /// <summary>Calculated but not reserved.</summary>
    Draft,
    /// <summary>Materials are reserved.</summary>
    Released,
    /// <summary>Materials have been issued.</summary>
    InProgress,
    /// <summary>Production is complete.</summary>
    Completed,
    /// <summary>Cancelled before material issue.</summary>
    Cancelled
}

/// <summary>A menu item and quantity planned for production.</summary>
public sealed record ProductionPlanLineRequest(Guid MenuItemId, decimal Quantity);
/// <summary>Creates a production plan.</summary>
public sealed record CreateProductionPlanRequest(string Name, DateOnly PlannedDate, string Notes, ProductionPlanLineRequest[] Lines);
/// <summary>Changes a document status using optimistic concurrency.</summary>
public sealed record ChangeProductionStatusRequest(string Reason, string Version);
/// <summary>Reports actual material use for goods issue.</summary>
public sealed record ProductionMaterialUsageRequest(Guid IngredientItemId, decimal ActualConsumption, decimal Wastage);
/// <summary>Issues materials and records actual quantities.</summary>
public sealed record IssueProductionMaterialsRequest(ProductionMaterialUsageRequest[] Materials, string Reason, string Version);

/// <summary>A line in a production plan.</summary>
public sealed record ProductionPlanLineResponse(Guid Id, Guid MenuItemId, decimal Quantity);
/// <summary>A production plan.</summary>
public sealed record ProductionPlanResponse(Guid Id, string PlanNumber, string Name, DateOnly PlannedDate, string Notes, ProductionPlanStatus Status, ProductionPlanLineResponse[] Lines, DateTimeOffset CreatedAt, string CreatedBy, string Version);
/// <summary>A calculated or consumed material requirement.</summary>
public sealed record ProductionMaterialRequirementResponse(Guid Id, Guid IngredientItemId, decimal BomQuantity, decimal ExpectedWastage, decimal RequiredQuantity, decimal ActualConsumption, decimal ActualWastage, decimal Variance, string Version);
/// <summary>A production order.</summary>
public sealed record ProductionOrderResponse(Guid Id, string OrderNumber, Guid PlanId, Guid PlanLineId, Guid MenuItemId, decimal PlannedQuantity, ProductionOrderStatus Status, ProductionMaterialRequirementResponse[] Requirements, DateTimeOffset CreatedAt, string CreatedBy, DateTimeOffset? CompletedAt, string Version);
/// <summary>An append-only production status change.</summary>
public sealed record ProductionStatusHistoryResponse(Guid Id, string DocumentType, Guid DocumentId, string PreviousStatus, string NewStatus, string Reason, string ChangedBy, DateTimeOffset ChangedAt);

/// <summary>Recipe ingredient snapshot supplied by Catalog to Production.</summary>
public sealed record ProductionRecipeIngredient(Guid ItemId, decimal Quantity, decimal WastagePercentage);
/// <summary>A menu item with an active recipe available for production planning.</summary>
public sealed record ProductionMenuItemReference(Guid Id, string Code, string Name);
/// <summary>Catalog item name displayed for existing production orders.</summary>
public sealed record ProductionCatalogItemReference(Guid Id, string Name);
/// <summary>Recipe snapshot supplied by Catalog to Production.</summary>
public sealed record ProductionRecipeSnapshot(Guid MenuItemId, decimal YieldQuantity, ProductionRecipeIngredient[] Ingredients);
/// <summary>Supplies active recipes without exposing Catalog persistence.</summary>
public interface IProductionRecipePort
{
    /// <summary>Lists active menu items that have active recipes.</summary>
    Task<ProductionMenuItemReference[]> GetMenuItemsAsync(CancellationToken cancellationToken);
    /// <summary>Gets item names for displaying historical production documents.</summary>
    Task<ProductionCatalogItemReference[]> GetItemNamesAsync(CancellationToken cancellationToken);
    /// <summary>Gets the active recipe for a menu item.</summary>
    Task<ProductionRecipeSnapshot?> GetRecipeAsync(Guid menuItemId, CancellationToken cancellationToken);
}

/// <summary>A material quantity to reserve for a production order.</summary>
public sealed record ProductionInventoryRequirement(Guid ItemId, decimal Quantity);
/// <summary>Actual consumption and wastage passed to Inventory.</summary>
public sealed record ProductionInventoryUsage(Guid ItemId, decimal ConsumptionQuantity, decimal WastageQuantity);
/// <summary>An Inventory reservation or issue allocation.</summary>
public sealed record ProductionInventoryAllocation(Guid ReservationId, Guid StockLotId, Guid ItemId, decimal Quantity);
/// <summary>Result returned by the Production-to-Inventory boundary.</summary>
public sealed record ProductionInventoryResult(bool Succeeded, ProductionInventoryAllocation[] Allocations, string[] Errors);

/// <summary>Inventory operations required by Production.</summary>
public interface IProductionInventoryPort
{
    /// <summary>Reserves material requirements using eligible FEFO lots.</summary>
    Task<ProductionInventoryResult> ReserveAsync(string referenceId, ProductionInventoryRequirement[] requirements, string actor, CancellationToken cancellationToken);
    /// <summary>Issues actual consumption and wastage, consuming reservations first.</summary>
    Task<ProductionInventoryResult> IssueAsync(string referenceId, ProductionInventoryUsage[] usages, string actor, CancellationToken cancellationToken);
    /// <summary>Releases remaining material reservations.</summary>
    Task<ProductionInventoryResult> ReleaseAsync(string referenceId, string reason, string actor, CancellationToken cancellationToken);
}
