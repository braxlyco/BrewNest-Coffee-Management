using BrewNest.BuildingBlocks.RealTime;
using BrewNest.Contracts.Kitchen;
using Microsoft.AspNetCore.SignalR;

namespace BrewNest.Modules.Kitchen.Infrastructure.Integrations;

internal sealed class SignalRKitchenRealtimePublisher(IHubContext<BrewNestHub> hub) : IKitchenRealtimePublisher
{
    public Task QueueChangedAsync(Guid ticketId, string action, CancellationToken cancellationToken) =>
        hub.Clients.All.SendAsync("KitchenQueueChanged", new { TicketId = ticketId, Action = action, ChangedAt = DateTimeOffset.UtcNow }, cancellationToken);
}
