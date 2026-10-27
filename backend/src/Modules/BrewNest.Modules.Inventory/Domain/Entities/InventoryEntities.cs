namespace BrewNest.Modules.Inventory.Domain.Entities;

internal sealed class StockLot
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public DateOnly? ManufacturingDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public decimal OnHandQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
    public DateTimeOffset? BlockedAt { get; set; }
    public string? BlockedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
internal enum ReservationStatus { Active, PartiallyReleased, Released, PartiallyConsumed, Consumed, Cancelled }
internal sealed class StockReservation
{
    public Guid Id { get; set; }
    public Guid StockLotId { get; set; }
    public StockLot StockLot { get; set; } = null!;
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReleasedQuantity { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
    public ReservationStatus Status { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
internal enum StockMovementType { GoodsReceipt, VendorReturn, Reservation, ReservationRelease, GoodsIssue, AdjustmentIncrease, AdjustmentDecrease, Wastage, Blocked, Unblocked, ExpiredBlocked }
internal sealed class StockMovement
{
    public Guid Id { get; set; }
    public Guid StockLotId { get; set; }
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public decimal OnHandAfter { get; set; }
    public decimal ReservedAfter { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTimeOffset PerformedAt { get; set; }
}
