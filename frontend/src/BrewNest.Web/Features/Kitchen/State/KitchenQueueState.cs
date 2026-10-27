using BrewNest.Contracts.Kitchen;

namespace BrewNest.Web.Features.Kitchen.State;

internal sealed class KitchenQueueState
{
    public KitchenTicketResponse[] Tickets { get; private set; } = [];
    public event Action? Changed;

    public void Replace(KitchenTicketResponse[] tickets)
    {
        Tickets = tickets;
        Changed?.Invoke();
    }
}
