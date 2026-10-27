using BrewNest.BuildingBlocks.Authorization;

namespace BrewNest.Modules.Kitchen.Domain.BusinessRules;

internal static class KitchenPermissions
{
    public const string QueueRead = "kitchen.queue.read";
    public const string TicketsGenerate = "kitchen.tickets.generate";
    public const string PreparationManage = "kitchen.preparation.manage";
    public const string TicketsCancel = "kitchen.tickets.cancel";
}

internal sealed class KitchenPermissionDefinitionProvider : IPermissionDefinitionProvider
{
    public IReadOnlyCollection<PermissionDefinition> Permissions { get; } =
    [
        new(KitchenPermissions.QueueRead, "View KOTs, preparation queues, and status history.", "Kitchen"),
        new(KitchenPermissions.TicketsGenerate, "Generate initial and additional KOTs from confirmed Sales orders.", "Kitchen"),
        new(KitchenPermissions.PreparationManage, "Start preparation and mark KOT lines ready.", "Kitchen"),
        new(KitchenPermissions.TicketsCancel, "Cancel queued KOTs and release their ingredient reservations.", "Kitchen")
    ];
}
