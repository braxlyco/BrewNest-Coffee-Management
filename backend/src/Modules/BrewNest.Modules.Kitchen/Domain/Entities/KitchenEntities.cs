namespace BrewNest.Modules.Kitchen.Domain.Entities;

internal enum TicketStatus { Queued, Preparing, Ready, Cancelled }
internal enum TicketLineStatus { Pending, Preparing, Ready, Cancelled }

internal sealed class KitchenTicket
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public Guid SalesOrderId { get; set; }
    public string SalesOrderNumber { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public TicketStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public List<KitchenTicketLine> Lines { get; set; } = [];
    public List<KitchenReservationLink> Reservations { get; set; } = [];
}

internal sealed class KitchenTicketLine
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public KitchenTicket Ticket { get; set; } = null!;
    public Guid SalesOrderLineId { get; set; }
    public Guid MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string SpecialInstructions { get; set; } = string.Empty;
    public TicketLineStatus Status { get; set; }
    public DateTimeOffset? PreparationStartedAt { get; set; }
    public DateTimeOffset? ReadyAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

internal sealed class KitchenReservationLink
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public KitchenTicket Ticket { get; set; } = null!;
    public Guid InventoryReservationId { get; set; }
    public Guid IngredientItemId { get; set; }
    public decimal Quantity { get; set; }
}

internal sealed class KitchenStatusHistory
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid? LineId { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; set; }
}
