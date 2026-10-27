namespace BrewNest.Modules.Production.Domain.Entities;

internal enum PlanStatus { Draft, Released, Completed, Cancelled }
internal enum OrderStatus { Draft, Released, InProgress, Completed, Cancelled }

internal sealed class ProductionPlan
{
    public Guid Id { get; set; } public string PlanNumber { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public DateOnly PlannedDate { get; set; } public string Notes { get; set; } = string.Empty; public PlanStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; } public string CreatedBy { get; set; } = string.Empty; public DateTimeOffset? ModifiedAt { get; set; } public string? ModifiedBy { get; set; } public byte[] RowVersion { get; set; } = [];
    public List<ProductionPlanLine> Lines { get; set; } = []; public List<ProductionOrder> Orders { get; set; } = [];
}

internal sealed class ProductionPlanLine
{
    public Guid Id { get; set; } public Guid PlanId { get; set; } public ProductionPlan Plan { get; set; } = null!; public Guid MenuItemId { get; set; } public decimal Quantity { get; set; }
}

internal sealed class ProductionOrder
{
    public Guid Id { get; set; } public string OrderNumber { get; set; } = string.Empty; public Guid PlanId { get; set; } public ProductionPlan Plan { get; set; } = null!; public Guid PlanLineId { get; set; } public Guid MenuItemId { get; set; } public decimal PlannedQuantity { get; set; } public OrderStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; } public string CreatedBy { get; set; } = string.Empty; public DateTimeOffset? ModifiedAt { get; set; } public string? ModifiedBy { get; set; } public DateTimeOffset? CompletedAt { get; set; } public byte[] RowVersion { get; set; } = [];
    public List<ProductionMaterialRequirement> Requirements { get; set; } = [];
}

internal sealed class ProductionMaterialRequirement
{
    public Guid Id { get; set; } public Guid OrderId { get; set; } public ProductionOrder Order { get; set; } = null!; public Guid IngredientItemId { get; set; }
    public decimal BomQuantity { get; set; } public decimal ExpectedWastage { get; set; } public decimal RequiredQuantity { get; set; } public decimal ActualConsumption { get; set; } public decimal ActualWastage { get; set; } public decimal Variance { get; set; } public byte[] RowVersion { get; set; } = [];
}

internal sealed class ProductionStatusHistory
{
    public Guid Id { get; set; } public string DocumentType { get; set; } = string.Empty; public Guid DocumentId { get; set; } public string PreviousStatus { get; set; } = string.Empty; public string NewStatus { get; set; } = string.Empty; public string Reason { get; set; } = string.Empty; public string ChangedBy { get; set; } = string.Empty; public DateTimeOffset ChangedAt { get; set; }
}
