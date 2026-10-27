using System.ComponentModel.DataAnnotations;

namespace BrewNest.Web.Features.Kitchen.Models;

internal static class KitchenPermissionNames
{
    public const string QueueRead = "kitchen.queue.read";
    public const string TicketsGenerate = "kitchen.tickets.generate";
    public const string PreparationManage = "kitchen.preparation.manage";
    public const string TicketsCancel = "kitchen.tickets.cancel";
}

internal sealed class GenerateTicketForm
{
    [Required]
    public string SalesOrderId { get; set; } = string.Empty;
}

internal sealed class KitchenStatusForm
{
    [Required, StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}

internal sealed record KitchenQueueNotification(Guid TicketId, string Action, DateTimeOffset ChangedAt);
