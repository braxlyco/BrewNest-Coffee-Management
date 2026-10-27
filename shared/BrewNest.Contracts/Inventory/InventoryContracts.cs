namespace BrewNest.Contracts.Inventory;

/// <summary>Current stock state for one lot in one Catalog warehouse.</summary>
public sealed record StockLotResponse(Guid Id, Guid ItemId, Guid WarehouseId, string LotNumber, DateOnly? ManufacturingDate, DateOnly? ExpiryDate, decimal OnHandQuantity, decimal ReservedQuantity, decimal AvailableQuantity, bool IsBlocked, string? BlockReason, string Version);
/// <summary>Creates a lot through a goods receipt.</summary>
public sealed record GoodsReceiptRequest(Guid ItemId, Guid WarehouseId, string LotNumber, DateOnly? ManufacturingDate, DateOnly? ExpiryDate, decimal Quantity, string ReferenceType, string ReferenceNumber, string Remarks);
/// <summary>Reserves available lot stock for a future workflow.</summary>
public sealed record CreateReservationRequest(Guid StockLotId, decimal Quantity, string ReferenceType, string ReferenceId, DateTimeOffset? ExpiresAt, string Remarks);
/// <summary>A stock reservation.</summary>
public sealed record ReservationResponse(Guid Id, Guid StockLotId, Guid ItemId, Guid WarehouseId, decimal Quantity, decimal RemainingQuantity, string ReferenceType, string ReferenceId, string Status, DateTimeOffset? ExpiresAt, string Version);
/// <summary>Releases all or part of a reservation.</summary>
public sealed record ReleaseReservationRequest(decimal Quantity, string Reason, string Version);
/// <summary>Issues physical stock, optionally consuming a reservation.</summary>
public sealed record GoodsIssueRequest(Guid StockLotId, Guid? ReservationId, decimal Quantity, string ReferenceType, string ReferenceNumber, string Remarks, string LotVersion, string? ReservationVersion);
/// <summary>Adjusts physical on-hand stock while preserving a ledger entry.</summary>
public sealed record StockAdjustmentRequest(Guid StockLotId, decimal QuantityDelta, string Reason, string ReferenceNumber, string Version);
/// <summary>Records physical stock as wastage.</summary>
public sealed record WastageRequest(Guid StockLotId, decimal Quantity, string Reason, string ReferenceNumber, string Version);
/// <summary>Blocks or unblocks a lot.</summary>
public sealed record SetLotBlockRequest(bool IsBlocked, string Reason, string Version);
/// <summary>Append-only stock movement.</summary>
public sealed record StockMovementResponse(Guid Id, Guid StockLotId, Guid ItemId, Guid WarehouseId, string MovementType, decimal Quantity, decimal OnHandAfter, decimal ReservedAfter, string ReferenceType, string ReferenceNumber, string Remarks, string PerformedBy, DateTimeOffset PerformedAt);
/// <summary>Expiry alert projection for a lot.</summary>
public sealed record ExpiryAlertResponse(Guid StockLotId, Guid ItemId, Guid WarehouseId, string LotNumber, DateOnly ExpiryDate, int DaysUntilExpiry, string Severity, bool IsBlocked, decimal OnHandQuantity, decimal AvailableQuantity);
/// <summary>Current configurable expiry thresholds.</summary>
public sealed record InventoryExpiryThresholds(int FirstAlertDays, int EscalatedAlertDays, int CriticalAlertDays);

/// <summary>Catalog item label; inactive records remain available for historical display.</summary>
public sealed record InventoryCatalogItemReference(Guid Id, string Code, string Name, string ItemType, bool IsActive);
/// <summary>Catalog warehouse label; inactive records remain available for historical display.</summary>
public sealed record InventoryCatalogWarehouseReference(Guid Id, string Code, string Name, bool IsActive);
/// <summary>Active references needed for Inventory workflows.</summary>
public sealed record InventoryCatalogReferenceData(InventoryCatalogItemReference[] Items, InventoryCatalogWarehouseReference[] Warehouses);

/// <summary>Supplies Catalog labels to Inventory without sharing persistence tables.</summary>
public interface IInventoryCatalogReferenceLookup
{
    /// <summary>Gets current labels and active states for entry and historical screens.</summary>
    Task<InventoryCatalogReferenceData> GetAsync(CancellationToken cancellationToken);
}

/// <summary>Validates Catalog item and warehouse identifiers without allowing Inventory to access Catalog tables.</summary>
public interface IInventoryCatalogReferenceValidator
{
    /// <summary>Returns whether an item and warehouse are active Catalog references.</summary>
    ValueTask<bool> AreValidAsync(Guid itemId, Guid warehouseId, CancellationToken cancellationToken);
}

/// <summary>Supplies expiry thresholds, allowing a future Administration adapter to replace configuration.</summary>
public interface IInventoryExpiryPolicyProvider
{
    /// <summary>Gets ordered expiry thresholds.</summary>
    ValueTask<InventoryExpiryThresholds> GetAsync(CancellationToken cancellationToken);
}

/// <summary>Inventory reservation port for future Kitchen, Sales, Production, and Procurement modules.</summary>
public interface IInventoryReservationPort
{
    /// <summary>Reserves stock on a specific eligible lot.</summary>
    Task<ReservationResponse> ReserveAsync(CreateReservationRequest request, string actor, CancellationToken cancellationToken);
    /// <summary>Releases a reservation quantity.</summary>
    Task<ReservationResponse> ReleaseAsync(Guid reservationId, ReleaseReservationRequest request, string actor, CancellationToken cancellationToken);
}
