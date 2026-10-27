using BrewNest.Contracts.Kitchen;
using BrewNest.Modules.Kitchen.Application.Services;
using BrewNest.Modules.Kitchen.Application.Interfaces;
using BrewNest.Modules.Kitchen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.UnitTests;

/// <summary>Verifies KOT generation, reservation, status, and cancellation rules.</summary>
public sealed class KitchenServiceTests
{
    /// <summary>Only confirmed Sales orders can generate a KOT.</summary>
    [Fact]
    public async Task GenerationRequiresConfirmedSalesOrder()
    {
        await using var database = Database(); var ports = new TestPorts { SalesStatus = "Draft" };
        var result = await Service(database, ports).GenerateAsync(new(ports.OrderId), new("chef"), default);
        Assert.False(result.Succeeded); Assert.Contains("confirmed", string.Join(' ', result.Errors), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>A configured recipe is required before ingredients can be reserved.</summary>
    [Fact]
    public async Task GenerationRequiresRecipe()
    {
        await using var database = Database(); var ports = new TestPorts { HasRecipe = false };
        var result = await Service(database, ports).GenerateAsync(new(ports.OrderId), new("chef"), default);
        Assert.False(result.Succeeded); Assert.Contains("recipe", string.Join(' ', result.Errors), StringComparison.OrdinalIgnoreCase); Assert.Equal(0, ports.ReserveCalls);
    }

    /// <summary>An additional KOT contains only quantity not already ticketed.</summary>
    [Fact]
    public async Task AdditionalTicketContainsOnlyNewQuantity()
    {
        await using var database = Database(); var ports = new TestPorts(); var service = Service(database, ports);
        var first = Value(await service.GenerateAsync(new(ports.OrderId), new("chef"), default));
        Assert.Equal(1m, Assert.Single(first.Lines).Quantity);
        ports.LineQuantity = 3m;
        var second = Value(await service.GenerateAsync(new(ports.OrderId), new("chef"), default));
        Assert.Equal(2, second.Sequence); Assert.Equal(2m, Assert.Single(second.Lines).Quantity); Assert.Equal(2, ports.ReserveCalls);
        var duplicate = await service.GenerateAsync(new(ports.OrderId), new("chef"), default);
        Assert.False(duplicate.Succeeded); Assert.Contains("No new", string.Join(' ', duplicate.Errors));
    }

    /// <summary>Cancelling an unstarted KOT releases Inventory reservations.</summary>
    [Fact]
    public async Task QueuedCancellationReleasesReservations()
    {
        await using var database = Database(); var ports = new TestPorts(); var service = Service(database, ports);
        var ticket = Value(await service.GenerateAsync(new(ports.OrderId), new("chef"), default));
        var bypass = await service.ChangeTicketStatusAsync(ticket.Id, new(KitchenTicketStatus.Cancelled, "Bypass.", ticket.Version), new("chef"), default);
        Assert.False(bypass.Succeeded); Assert.Contains("protected", string.Join(' ', bypass.Errors));
        var cancelled = Value(await service.CancelAsync(ticket.Id, "Customer cancelled.", ticket.Version, new("manager"), default));
        Assert.Equal(KitchenTicketStatus.Cancelled, cancelled.Status); Assert.Equal(1, ports.ReleaseCalls);
    }

    /// <summary>Preparation follows Pending to Preparing to Ready and then cannot be cancelled.</summary>
    [Fact]
    public async Task PreparationTransitionPreventsLateCancellation()
    {
        await using var database = Database(); var ports = new TestPorts(); var service = Service(database, ports);
        var ticket = Value(await service.GenerateAsync(new(ports.OrderId), new("chef"), default)); var line = Assert.Single(ticket.Lines);
        ticket = Value(await service.ChangeLineStatusAsync(ticket.Id, line.Id, new(KitchenLineStatus.Preparing, "Started.", ticket.Version, line.Version), new("chef"), default));
        Assert.Equal(KitchenTicketStatus.Preparing, ticket.Status);
        var cancelled = await service.CancelAsync(ticket.Id, "Too late.", ticket.Version, new("manager"), default);
        Assert.False(cancelled.Succeeded); Assert.Contains("unstarted", string.Join(' ', cancelled.Errors));
        line = Assert.Single(ticket.Lines);
        ticket = Value(await service.ChangeLineStatusAsync(ticket.Id, line.Id, new(KitchenLineStatus.Ready, "Ready.", ticket.Version, line.Version), new("chef"), default));
        Assert.Equal(KitchenTicketStatus.Ready, ticket.Status);
    }

    private static KitchenDbContext Database() => new(new DbContextOptionsBuilder<KitchenDbContext>().UseInMemoryDatabase($"kitchen-{Guid.NewGuid():N}").Options);
    private static KitchenService Service(KitchenDbContext database, TestPorts ports) => new(database, ports, ports, ports, ports, ports, new FixedTime());
    private static T Value<T>(KitchenResult<T> result) { Assert.True(result.Succeeded, string.Join(' ', result.Errors)); return Assert.IsType<T>(result.Value); }

    private sealed class TestPorts : IKitchenSalesOrderPort, IKitchenRecipePort, IKitchenInventoryPort, IKitchenRealtimePublisher, IKitchenDocumentNumberPort
    {
        public Guid OrderId { get; } = Guid.NewGuid(); public Guid LineId { get; } = Guid.NewGuid(); public Guid MenuItemId { get; } = Guid.NewGuid(); public Guid IngredientId { get; } = Guid.NewGuid();
        public string SalesStatus { get; set; } = "Confirmed"; public bool HasRecipe { get; set; } = true; public decimal LineQuantity { get; set; } = 1m; public int ReserveCalls { get; private set; } public int ReleaseCalls { get; private set; }
        public string NextTicketNumber() => $"KOT-TEST-{Guid.NewGuid():N}"[..30];
        public Task<KitchenSalesOrderSnapshot?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken) => Task.FromResult<KitchenSalesOrderSnapshot?>(orderId == OrderId ? new(OrderId, "SO-TEST", "Takeaway", SalesStatus, [new(LineId, MenuItemId, "Coffee", LineQuantity, "hot", false)]) : null);
        public Task<KitchenSalesOrderSnapshot[]> GetConfirmedOrdersAsync(CancellationToken cancellationToken) => Task.FromResult(SalesStatus == "Confirmed" ? new KitchenSalesOrderSnapshot[] { new(OrderId, "SO-TEST", "Takeaway", SalesStatus, [new(LineId, MenuItemId, "Coffee", LineQuantity, "hot", false)]) } : []);
        public Task<KitchenRecipeSnapshot?> GetRecipeAsync(Guid menuItemId, CancellationToken cancellationToken) => Task.FromResult<KitchenRecipeSnapshot?>(HasRecipe ? new(menuItemId, 1m, [new(IngredientId, 0.018m, 0m)]) : null);
        public Task<KitchenInventoryReservationResult> ReserveAsync(string referenceId, KitchenIngredientRequirement[] requirements, string actor, CancellationToken cancellationToken) { ReserveCalls++; return Task.FromResult(new KitchenInventoryReservationResult(true, requirements.Select(x => new KitchenInventoryAllocation(Guid.NewGuid(), x.ItemId, x.Quantity)).ToArray(), [])); }
        public Task<KitchenInventoryReservationResult> ReleaseAsync(string referenceId, string reason, string actor, CancellationToken cancellationToken) { ReleaseCalls++; return Task.FromResult(new KitchenInventoryReservationResult(true, [], [])); }
        public Task QueueChangedAsync(Guid ticketId, string action, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FixedTime : TimeProvider { public override DateTimeOffset GetUtcNow() => new(2026, 9, 24, 9, 0, 0, TimeSpan.Zero); }
}
