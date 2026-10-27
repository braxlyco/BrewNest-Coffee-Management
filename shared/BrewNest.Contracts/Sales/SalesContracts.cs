namespace BrewNest.Contracts.Sales;

/// <summary>Sales order fulfillment modes.</summary>
public enum SalesOrderType
{
    /// <summary>Order served at an assigned dining table.</summary>
    DineIn,
    /// <summary>Order collected by the customer.</summary>
    Takeaway,
    /// <summary>Order delivered to a customer address.</summary>
    Delivery
}
/// <summary>Sales order lifecycle states.</summary>
public enum SalesOrderStatus
{
    /// <summary>Open for editing.</summary>
    Draft,
    /// <summary>Confirmed for downstream processing.</summary>
    Confirmed,
    /// <summary>Final bill issued.</summary>
    Billed,
    /// <summary>Order completed and closed.</summary>
    Closed,
    /// <summary>Unbilled order cancelled.</summary>
    Cancelled
}
/// <summary>Discount approval states.</summary>
public enum SalesDiscountStatus
{
    /// <summary>Waiting for manager decision.</summary>
    Pending,
    /// <summary>Approved by a manager.</summary>
    Approved,
    /// <summary>Rejected by a manager.</summary>
    Rejected,
    /// <summary>Invalidated by an order change or cancellation.</summary>
    Cancelled
}

/// <summary>Creates a draft sales order.</summary>
public sealed record CreateSalesOrderRequest(SalesOrderType OrderType,Guid? CustomerId,Guid? DiningTableId,string CustomerName,string CustomerPhone,string DeliveryAddress,string Notes);
/// <summary>Adds a menu item to an open order.</summary>
public sealed record AddSalesOrderLineRequest(Guid MenuItemId,decimal Quantity,string SpecialInstructions,string Version);
/// <summary>Changes table, customer, delivery, or note details before billing.</summary>
public sealed record UpdateSalesOrderRequest(Guid? CustomerId,Guid? DiningTableId,string CustomerName,string CustomerPhone,string DeliveryAddress,string Notes,string Version);
/// <summary>Changes the quantity or cancellation state of an existing order line.</summary>
public sealed record UpdateSalesOrderLineRequest(decimal Quantity,bool IsCancelled,string Reason,string OrderVersion,string LineVersion);
/// <summary>Requests a percentage discount for manager approval.</summary>
public sealed record RequestSalesDiscountRequest(decimal Percentage,string Reason,string Version);
/// <summary>Approves or rejects a pending discount.</summary>
public sealed record DecideSalesDiscountRequest(bool Approve,string Remarks,string OrderVersion,string DiscountVersion);
/// <summary>Confirms, bills, closes, or cancels an order with optimistic concurrency.</summary>
public sealed record ChangeSalesOrderStatusRequest(string Reason,string Version);

/// <summary>Immutable price and description snapshot captured on an order line.</summary>
public sealed record SalesOrderLineResponse(Guid Id,Guid MenuItemId,string ItemCode,string ItemName,decimal Quantity,decimal UnitPrice,decimal LineTotal,string SpecialInstructions,bool IsCancelled,string? CancellationReason,string Version);
/// <summary>Discount request and decision audit information.</summary>
public sealed record SalesDiscountResponse(Guid Id,decimal Percentage,decimal Amount,string Reason,SalesDiscountStatus Status,string RequestedBy,DateTimeOffset RequestedAt,string? ApprovedBy,DateTimeOffset? ApprovedAt,string? DecisionRemarks,string Version);
/// <summary>Final immutable bill values.</summary>
public sealed record SalesBillResponse(Guid Id,string BillNumber,decimal GrossAmount,decimal DiscountAmount,decimal NetAmount,DateTimeOffset IssuedAt,string IssuedBy);
/// <summary>Complete sales order projection.</summary>
public sealed record SalesOrderResponse(Guid Id,string OrderNumber,SalesOrderType OrderType,SalesOrderStatus Status,Guid? CustomerId,Guid? DiningTableId,string CustomerName,string CustomerPhone,string DeliveryAddress,string Notes,decimal GrossAmount,decimal DiscountAmount,decimal NetAmount,SalesOrderLineResponse[] Lines,SalesDiscountResponse[] Discounts,SalesBillResponse? Bill,DateTimeOffset CreatedAt,string CreatedBy,string Version);
/// <summary>Append-only Sales status change.</summary>
public sealed record SalesStatusHistoryResponse(Guid Id,Guid OrderId,string PreviousStatus,string NewStatus,string Reason,string ChangedBy,DateTimeOffset ChangedAt);
/// <summary>Active reference data needed by the Sales UI.</summary>
public sealed record SalesReferenceDataResponse(SalesMenuItemReference[] MenuItems,SalesCustomerReference[] Customers,SalesDiningTableReference[] DiningTables);
/// <summary>Sellable Catalog item snapshot source.</summary>
public sealed record SalesMenuItemReference(Guid Id,string Code,string Name,decimal SalesPrice);
/// <summary>Customer reference for Sales.</summary>
public sealed record SalesCustomerReference(Guid Id,string Code,string Name,string Phone);
/// <summary>Dining-table reference for Sales.</summary>
public sealed record SalesDiningTableReference(Guid Id,string Code,string Name,int Capacity,string Area);

/// <summary>Catalog-owned query port used by Sales without accessing Catalog persistence.</summary>
public interface ISalesCatalogPort
{
    /// <summary>Returns all active Sales reference data.</summary>
    Task<SalesReferenceDataResponse> GetReferencesAsync(CancellationToken cancellationToken);
    /// <summary>Returns an active sellable menu item.</summary>
    Task<SalesMenuItemReference?> GetMenuItemAsync(Guid id,CancellationToken cancellationToken);
    /// <summary>Returns whether a customer is active.</summary>
    Task<bool> IsCustomerActiveAsync(Guid id,CancellationToken cancellationToken);
    /// <summary>Returns whether a dining table is active.</summary>
    Task<bool> IsDiningTableActiveAsync(Guid id,CancellationToken cancellationToken);
}

/// <summary>Replaceable document-number provider for Sales orders and bills.</summary>
public interface ISalesDocumentNumberPort
{
    /// <summary>Creates a human-readable order number.</summary>
    string NextOrderNumber();
    /// <summary>Creates a human-readable bill number.</summary>
    string NextBillNumber();
}
