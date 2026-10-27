namespace BrewNest.Contracts.Kitchen;

/// <summary>Kitchen order ticket lifecycle state.</summary>
public enum KitchenTicketStatus
{
    /// <summary>Waiting for preparation to begin.</summary>
    Queued,
    /// <summary>At least one line is being prepared.</summary>
    Preparing,
    /// <summary>Every active line is ready.</summary>
    Ready,
    /// <summary>Cancelled before preparation began.</summary>
    Cancelled
}

/// <summary>Kitchen ticket-line preparation state.</summary>
public enum KitchenLineStatus
{
    /// <summary>Waiting for preparation.</summary>
    Pending,
    /// <summary>Currently being prepared.</summary>
    Preparing,
    /// <summary>Preparation is complete.</summary>
    Ready,
    /// <summary>Cancelled as part of a queued ticket cancellation.</summary>
    Cancelled
}

/// <summary>Requests generation of the next KOT for a confirmed Sales order.</summary>
public sealed record GenerateKitchenTicketRequest(Guid SalesOrderId);

/// <summary>A confirmed order with items not yet sent to the kitchen.</summary>
public sealed record KitchenConfirmedOrderReference(Guid Id, string OrderNumber, int PendingLineCount, string OrderType, string ItemPreview);

/// <summary>Requests a ticket status transition.</summary>
public sealed record ChangeKitchenTicketStatusRequest(KitchenTicketStatus Status, string Reason, string Version);

/// <summary>Requests a ticket-line status transition.</summary>
public sealed record ChangeKitchenLineStatusRequest(KitchenLineStatus Status, string Reason, string TicketVersion, string LineVersion);

/// <summary>A kitchen ticket line returned to clients.</summary>
public sealed record KitchenTicketLineResponse(Guid Id, Guid SalesOrderLineId, Guid MenuItemId, string ItemName, decimal Quantity, string SpecialInstructions, KitchenLineStatus Status, string Version);

/// <summary>An Inventory reservation created for a KOT.</summary>
public sealed record KitchenReservationResponse(Guid InventoryReservationId, Guid IngredientItemId, decimal Quantity);

/// <summary>A kitchen order ticket.</summary>
public sealed record KitchenTicketResponse(Guid Id, string TicketNumber, Guid SalesOrderId, string SalesOrderNumber, int Sequence, KitchenTicketStatus Status, DateTimeOffset CreatedAt, string CreatedBy, DateTimeOffset? ModifiedAt, KitchenTicketLineResponse[] Lines, KitchenReservationResponse[] Reservations, string Version);

/// <summary>An append-only Kitchen status-history record.</summary>
public sealed record KitchenStatusHistoryResponse(Guid Id, Guid TicketId, Guid? LineId, string PreviousStatus, string NewStatus, string Reason, string ChangedBy, DateTimeOffset ChangedAt);

/// <summary>Sales line snapshot supplied to Kitchen without exposing Sales persistence.</summary>
public sealed record KitchenSalesLineSnapshot(Guid Id, Guid MenuItemId, string ItemName, decimal Quantity, string SpecialInstructions, bool IsCancelled);

/// <summary>Confirmed Sales order snapshot supplied to Kitchen.</summary>
public sealed record KitchenSalesOrderSnapshot(Guid Id, string OrderNumber, string OrderType, string Status, KitchenSalesLineSnapshot[] Lines);

/// <summary>Supplies Sales order snapshots to Kitchen.</summary>
public interface IKitchenSalesOrderPort
{
    /// <summary>Gets an order snapshot, or null when it does not exist.</summary>
    Task<KitchenSalesOrderSnapshot?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>Lists recent confirmed orders for kitchen ticket selection.</summary>
    Task<KitchenSalesOrderSnapshot[]> GetConfirmedOrdersAsync(CancellationToken cancellationToken);
}

/// <summary>Recipe ingredient required by Kitchen.</summary>
public sealed record KitchenRecipeIngredient(Guid ItemId, decimal Quantity, decimal WastagePercentage, string Name = "", string UnitSymbol = "");

/// <summary>Current recipe snapshot supplied by Catalog.</summary>
public sealed record KitchenRecipeSnapshot(Guid MenuItemId, decimal YieldQuantity, KitchenRecipeIngredient[] Ingredients);

/// <summary>Supplies active recipe snapshots to Kitchen.</summary>
public interface IKitchenRecipePort
{
    /// <summary>Gets the active recipe for a menu item, or null when none is configured.</summary>
    Task<KitchenRecipeSnapshot?> GetRecipeAsync(Guid menuItemId, CancellationToken cancellationToken);
}

/// <summary>An ingredient quantity Kitchen asks Inventory to reserve.</summary>
public sealed record KitchenIngredientRequirement(Guid ItemId, decimal Quantity, string Name = "", string UnitSymbol = "");

/// <summary>A reservation allocated by Inventory.</summary>
public sealed record KitchenInventoryAllocation(Guid ReservationId, Guid ItemId, decimal Quantity);

/// <summary>Result of an Inventory reservation request.</summary>
public sealed record KitchenInventoryReservationResult(bool Succeeded, KitchenInventoryAllocation[] Allocations, string[] Errors);

/// <summary>Inventory boundary used by Kitchen for ingredient reservations and releases.</summary>
public interface IKitchenInventoryPort
{
    /// <summary>Atomically reserves requirements using eligible FEFO lots.</summary>
    Task<KitchenInventoryReservationResult> ReserveAsync(string referenceId, KitchenIngredientRequirement[] requirements, string actor, CancellationToken cancellationToken);

    /// <summary>Releases the remaining quantities reserved for a Kitchen reference.</summary>
    Task<KitchenInventoryReservationResult> ReleaseAsync(string referenceId, string reason, string actor, CancellationToken cancellationToken);
}

/// <summary>Publishes Kitchen changes after persistence succeeds.</summary>
public interface IKitchenRealtimePublisher
{
    /// <summary>Notifies subscribed clients that the kitchen queue changed.</summary>
    Task QueueChangedAsync(Guid ticketId, string action, CancellationToken cancellationToken);
}
