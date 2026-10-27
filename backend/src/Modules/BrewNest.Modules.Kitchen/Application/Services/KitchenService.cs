using System.Security.Claims;
using BrewNest.Contracts.Kitchen;
using BrewNest.Modules.Kitchen.Domain.Entities;
using BrewNest.Modules.Kitchen.Infrastructure.Persistence;
using BrewNest.Modules.Kitchen.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Kitchen.Application.Services;

internal enum KitchenResultStatus { Success, Validation, NotFound, Conflict }

internal sealed record KitchenResult<T>(KitchenResultStatus Status, T? Value, string[] Errors)
{
    public bool Succeeded => Status == KitchenResultStatus.Success;
    public static KitchenResult<T> Success(T value) => new(KitchenResultStatus.Success, value, []);
    public static KitchenResult<T> Failure(KitchenResultStatus status, params string[] errors) => new(status, default, errors);
}

internal sealed record KitchenRequestContext(string Actor)
{
    public static KitchenRequestContext FromHttpContext(HttpContext context) =>
        new(context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.Identity?.Name ?? "anonymous");
}

internal sealed class KitchenService(
    KitchenDbContext database,
    IKitchenSalesOrderPort sales,
    IKitchenRecipePort recipes,
    IKitchenInventoryPort inventory,
    IKitchenRealtimePublisher realtime,
    IKitchenDocumentNumberPort numbers,
    TimeProvider timeProvider)
{
    private DateTimeOffset Now => timeProvider.GetUtcNow();

    public async Task<KitchenResult<KitchenConfirmedOrderReference[]>> GetConfirmedOrdersAsync(CancellationToken token)
    {
        var orders = await sales.GetConfirmedOrdersAsync(token);
        var ids = orders.SelectMany(order => order.Lines).Select(line => line.Id).ToArray();
        var ticketed = await database.TicketLines.AsNoTracking()
            .Where(line => ids.Contains(line.SalesOrderLineId) && line.Ticket.Status != TicketStatus.Cancelled && line.Status != TicketLineStatus.Cancelled)
            .GroupBy(line => line.SalesOrderLineId)
            .Select(group => new { Id = group.Key, Quantity = group.Sum(line => line.Quantity) })
            .ToDictionaryAsync(x => x.Id, x => x.Quantity, token);
        var choices = orders.Select(order =>
            {
                var pending = order.Lines.Where(line => !line.IsCancelled && line.Quantity > ticketed.GetValueOrDefault(line.Id)).ToArray();
                var preview = string.Join(", ", pending.Take(2).Select(line => $"{line.ItemName} x{line.Quantity - ticketed.GetValueOrDefault(line.Id):0.###}"));
                return new KitchenConfirmedOrderReference(order.Id, order.OrderNumber, pending.Length, order.OrderType, pending.Length > 2 ? $"{preview}, +{pending.Length - 2} more" : preview);
            })
            .Where(order => order.PendingLineCount > 0)
            .ToArray();
        return KitchenResult<KitchenConfirmedOrderReference[]>.Success(choices);
    }

    public async Task<KitchenResult<KitchenTicketResponse[]>> GetQueueAsync(string? status, CancellationToken token)
    {
        var query = FullQuery().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<TicketStatus>(status, true, out var parsed)) return Validation<KitchenTicketResponse[]>("The KOT status is invalid.");
            query = query.Where(ticket => ticket.Status == parsed);
        }
        else
        {
            query = query.Where(ticket => ticket.Status != TicketStatus.Cancelled);
        }
        var tickets = await query.OrderBy(ticket => ticket.Status).ThenBy(ticket => ticket.CreatedAt).ToArrayAsync(token);
        return KitchenResult<KitchenTicketResponse[]>.Success(tickets.Select(Map).ToArray());
    }

    public async Task<KitchenResult<KitchenTicketResponse>> GetAsync(Guid id, CancellationToken token)
    {
        var ticket = await FullQuery().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, token);
        return ticket is null ? NotFound<KitchenTicketResponse>("The KOT was not found.") : KitchenResult<KitchenTicketResponse>.Success(Map(ticket));
    }

    public async Task<KitchenResult<KitchenStatusHistoryResponse[]>> GetHistoryAsync(Guid? ticketId, CancellationToken token)
    {
        var query = database.StatusHistory.AsNoTracking();
        if (ticketId is not null) query = query.Where(x => x.TicketId == ticketId);
        var values = await query.OrderByDescending(x => x.ChangedAt).Take(500).ToArrayAsync(token);
        return KitchenResult<KitchenStatusHistoryResponse[]>.Success(values.Select(Map).ToArray());
    }

    public async Task<KitchenResult<KitchenTicketResponse>> GenerateAsync(GenerateKitchenTicketRequest request, KitchenRequestContext context, CancellationToken token)
    {
        if (request.SalesOrderId == Guid.Empty) return Validation<KitchenTicketResponse>("A Sales order identifier is required.");
        var order = await sales.GetOrderAsync(request.SalesOrderId, token);
        if (order is null) return NotFound<KitchenTicketResponse>("The Sales order was not found.");
        if (!string.Equals(order.Status, "Confirmed", StringComparison.Ordinal)) return Conflict<KitchenTicketResponse>("KOTs may be generated only from confirmed Sales orders.");

        var existing = await database.TicketLines.AsNoTracking()
            .Where(line => line.Ticket.SalesOrderId == order.Id && line.Ticket.Status != TicketStatus.Cancelled && line.Status != TicketLineStatus.Cancelled)
            .GroupBy(line => line.SalesOrderLineId)
            .Select(group => new { SalesOrderLineId = group.Key, Quantity = group.Sum(line => line.Quantity) })
            .ToDictionaryAsync(x => x.SalesOrderLineId, x => x.Quantity, token);

        var pending = order.Lines.Where(line => !line.IsCancelled)
            .Select(line => new { Line = line, Quantity = line.Quantity - existing.GetValueOrDefault(line.Id) })
            .Where(x => x.Quantity > 0)
            .ToArray();
        if (pending.Length == 0) return Conflict<KitchenTicketResponse>("No new Sales order items require an additional KOT.");

        var requirements = new List<KitchenIngredientRequirement>();
        foreach (var pendingLine in pending)
        {
            var recipe = await recipes.GetRecipeAsync(pendingLine.Line.MenuItemId, token);
            if (recipe is null || recipe.YieldQuantity <= 0 || recipe.Ingredients.Length == 0)
                return Conflict<KitchenTicketResponse>($"No active recipe is configured for {pendingLine.Line.ItemName}.");
            foreach (var ingredient in recipe.Ingredients)
            {
                var raw = ingredient.Quantity / recipe.YieldQuantity * pendingLine.Quantity * (1m + ingredient.WastagePercentage / 100m);
                var quantity = Math.Max(0.001m, decimal.Round(raw, 3, MidpointRounding.AwayFromZero));
                requirements.Add(new(ingredient.ItemId, quantity, ingredient.Name, ingredient.UnitSymbol));
            }
        }

        var id = Guid.NewGuid();
        var reference = id.ToString("N");
        var combined = requirements.GroupBy(x => x.ItemId).Select(group => new KitchenIngredientRequirement(group.Key, group.Sum(x => x.Quantity), group.First().Name, group.First().UnitSymbol)).ToArray();
        var reservation = await inventory.ReserveAsync(reference, combined, context.Actor, token);
        if (!reservation.Succeeded) return Conflict<KitchenTicketResponse>(reservation.Errors);

        var sequence = await database.Tickets.CountAsync(x => x.SalesOrderId == order.Id, token) + 1;
        var ticket = new KitchenTicket
        {
            Id = id,
            TicketNumber = numbers.NextTicketNumber(),
            SalesOrderId = order.Id,
            SalesOrderNumber = order.OrderNumber,
            Sequence = sequence,
            Status = TicketStatus.Queued,
            CreatedAt = Now,
            CreatedBy = context.Actor
        };
        foreach (var item in pending)
        {
            var line = new KitchenTicketLine
            {
                Id = Guid.NewGuid(), TicketId = id, SalesOrderLineId = item.Line.Id, MenuItemId = item.Line.MenuItemId,
                ItemName = item.Line.ItemName, Quantity = item.Quantity, SpecialInstructions = item.Line.SpecialInstructions,
                Status = TicketLineStatus.Pending
            };
            ticket.Lines.Add(line); database.TicketLines.Add(line);
        }
        foreach (var allocation in reservation.Allocations)
        {
            var link = new KitchenReservationLink { Id = Guid.NewGuid(), TicketId = id, InventoryReservationId = allocation.ReservationId, IngredientItemId = allocation.ItemId, Quantity = allocation.Quantity };
            ticket.Reservations.Add(link); database.ReservationLinks.Add(link);
        }
        database.Tickets.Add(ticket);
        AddHistory(ticket.Id, null, "None", TicketStatus.Queued.ToString(), sequence == 1 ? "Initial KOT generated." : "Additional KOT generated.", context);
        try
        {
            await database.SaveChangesAsync(token);
        }
        catch (DbUpdateException)
        {
            await inventory.ReleaseAsync(reference, "Kitchen ticket creation failed.", context.Actor, token);
            return Conflict<KitchenTicketResponse>("The KOT conflicted with another update. Reload and try again.");
        }
        await realtime.QueueChangedAsync(ticket.Id, "Generated", token);
        return KitchenResult<KitchenTicketResponse>.Success(Map(ticket));
    }

    public Task<KitchenResult<KitchenTicketResponse>> ChangeTicketStatusAsync(Guid id, ChangeKitchenTicketStatusRequest request, KitchenRequestContext context, CancellationToken token)
    {
        return request.Status == KitchenTicketStatus.Cancelled
            ? Task.FromResult(Validation<KitchenTicketResponse>("Use the protected KOT cancellation operation."))
            : ChangeTicketStatusCoreAsync(id, request, context, token);
    }

    public Task<KitchenResult<KitchenTicketResponse>> CancelAsync(Guid id, string reason, string version, KitchenRequestContext context, CancellationToken token) =>
        ChangeTicketStatusCoreAsync(id, new(KitchenTicketStatus.Cancelled, reason, version), context, token);

    private async Task<KitchenResult<KitchenTicketResponse>> ChangeTicketStatusCoreAsync(Guid id, ChangeKitchenTicketStatusRequest request, KitchenRequestContext context, CancellationToken token)
    {
        var ticket = await FullQuery().SingleOrDefaultAsync(x => x.Id == id, token);
        if (ticket is null) return NotFound<KitchenTicketResponse>("The KOT was not found.");
        if (!ApplyVersion(ticket, request.Version)) return VersionFailure<KitchenTicketResponse>();
        if (!ValidReason(request.Reason)) return Validation<KitchenTicketResponse>("A reason up to 500 characters is required.");
        if (request.Status == KitchenTicketStatus.Cancelled)
        {
            if (ticket.Status != TicketStatus.Queued || ticket.Lines.Any(x => x.Status != TicketLineStatus.Pending)) return Conflict<KitchenTicketResponse>("Only a queued, unstarted KOT may be cancelled. Started preparation requires a later reversal or wastage workflow.");
            var release = await inventory.ReleaseAsync(ticket.Id.ToString("N"), request.Reason, context.Actor, token);
            if (!release.Succeeded) return Conflict<KitchenTicketResponse>(release.Errors);
            var previous = ticket.Status; ticket.Status = TicketStatus.Cancelled;
            foreach (var line in ticket.Lines) line.Status = TicketLineStatus.Cancelled;
            Touch(ticket, context); AddHistory(ticket.Id, null, previous.ToString(), ticket.Status.ToString(), request.Reason, context);
        }
        else
        {
            var target = (TicketStatus)request.Status;
            if (!ValidTicketTransition(ticket.Status, target)) return Conflict<KitchenTicketResponse>("The requested KOT status transition is not allowed.");
            var previous = ticket.Status; ticket.Status = target;
            foreach (var line in ticket.Lines.Where(x => x.Status != TicketLineStatus.Cancelled))
            {
                if (target == TicketStatus.Preparing && line.Status == TicketLineStatus.Pending) { line.Status = TicketLineStatus.Preparing; line.PreparationStartedAt = Now; }
                if (target == TicketStatus.Ready) { line.Status = TicketLineStatus.Ready; line.PreparationStartedAt ??= Now; line.ReadyAt = Now; }
            }
            Touch(ticket, context); AddHistory(ticket.Id, null, previous.ToString(), target.ToString(), request.Reason, context);
        }
        return await SaveAndPublishAsync(ticket, "StatusChanged", token);
    }

    public async Task<KitchenResult<KitchenTicketResponse>> ChangeLineStatusAsync(Guid ticketId, Guid lineId, ChangeKitchenLineStatusRequest request, KitchenRequestContext context, CancellationToken token)
    {
        var ticket = await FullQuery().SingleOrDefaultAsync(x => x.Id == ticketId, token);
        if (ticket is null) return NotFound<KitchenTicketResponse>("The KOT was not found.");
        var line = ticket.Lines.SingleOrDefault(x => x.Id == lineId);
        if (line is null) return NotFound<KitchenTicketResponse>("The KOT line was not found.");
        if (!ApplyVersion(ticket, request.TicketVersion) || !ApplyVersion(line, request.LineVersion)) return VersionFailure<KitchenTicketResponse>();
        if (!ValidReason(request.Reason)) return Validation<KitchenTicketResponse>("A reason up to 500 characters is required.");
        if (request.Status == KitchenLineStatus.Cancelled) return Conflict<KitchenTicketResponse>("Cancel the whole queued KOT so ingredient reservations can be released safely.");
        var target = (TicketLineStatus)request.Status;
        if (!ValidLineTransition(line.Status, target)) return Conflict<KitchenTicketResponse>("The requested KOT-line transition is not allowed.");
        var previous = line.Status; line.Status = target;
        if (target == TicketLineStatus.Preparing) line.PreparationStartedAt = Now;
        if (target == TicketLineStatus.Ready) { line.PreparationStartedAt ??= Now; line.ReadyAt = Now; }
        var previousTicket = ticket.Status;
        if (ticket.Lines.Where(x => x.Status != TicketLineStatus.Cancelled).All(x => x.Status == TicketLineStatus.Ready)) ticket.Status = TicketStatus.Ready;
        else if (ticket.Lines.Any(x => x.Status is TicketLineStatus.Preparing or TicketLineStatus.Ready)) ticket.Status = TicketStatus.Preparing;
        Touch(ticket, context); AddHistory(ticket.Id, line.Id, previous.ToString(), target.ToString(), request.Reason, context);
        if (previousTicket != ticket.Status) AddHistory(ticket.Id, null, previousTicket.ToString(), ticket.Status.ToString(), "Derived from line preparation status.", context);
        return await SaveAndPublishAsync(ticket, "LineStatusChanged", token);
    }

    private IQueryable<KitchenTicket> FullQuery() => database.Tickets.AsSplitQuery().Include(x => x.Lines).Include(x => x.Reservations);
    private async Task<KitchenResult<KitchenTicketResponse>> SaveAndPublishAsync(KitchenTicket ticket, string action, CancellationToken token)
    {
        try { await database.SaveChangesAsync(token); }
        catch (DbUpdateConcurrencyException) { return VersionFailure<KitchenTicketResponse>(); }
        catch (DbUpdateException) { return Conflict<KitchenTicketResponse>("The Kitchen change conflicts with current data."); }
        await realtime.QueueChangedAsync(ticket.Id, action, token);
        return KitchenResult<KitchenTicketResponse>.Success(Map(ticket));
    }
    private void AddHistory(Guid ticketId, Guid? lineId, string previous, string next, string reason, KitchenRequestContext context) => database.StatusHistory.Add(new() { Id = Guid.NewGuid(), TicketId = ticketId, LineId = lineId, PreviousStatus = previous, NewStatus = next, Reason = reason.Trim(), ChangedBy = context.Actor, ChangedAt = Now });
    private void Touch(KitchenTicket ticket, KitchenRequestContext context) { ticket.ModifiedAt = Now; ticket.ModifiedBy = context.Actor; }
    private bool ApplyVersion(KitchenTicket value, string version) => ApplyVersionCore(database.Entry(value).Property(x => x.RowVersion), version);
    private bool ApplyVersion(KitchenTicketLine value, string version) => ApplyVersionCore(database.Entry(value).Property(x => x.RowVersion), version);
    private static bool ApplyVersionCore<TEntity>(Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry<TEntity, byte[]> property, string version) where TEntity : class { try { var bytes = Convert.FromBase64String(version); if (bytes.Length == 0 && property.CurrentValue.Length == 0) return true; property.OriginalValue = bytes; return bytes.Length > 0; } catch (FormatException) { return false; } }
    private static bool ValidTicketTransition(TicketStatus current, TicketStatus target) => (current, target) is (TicketStatus.Queued, TicketStatus.Preparing) or (TicketStatus.Preparing, TicketStatus.Ready);
    private static bool ValidLineTransition(TicketLineStatus current, TicketLineStatus target) => (current, target) is (TicketLineStatus.Pending, TicketLineStatus.Preparing) or (TicketLineStatus.Preparing, TicketLineStatus.Ready);
    private static bool ValidReason(string? value) => !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= 500;
    private static string Version(byte[] value) => Convert.ToBase64String(value);
    private static KitchenTicketResponse Map(KitchenTicket ticket) => new(ticket.Id, ticket.TicketNumber, ticket.SalesOrderId, ticket.SalesOrderNumber, ticket.Sequence, (KitchenTicketStatus)ticket.Status, ticket.CreatedAt, ticket.CreatedBy, ticket.ModifiedAt, ticket.Lines.OrderBy(x => x.ItemName).Select(x => new KitchenTicketLineResponse(x.Id, x.SalesOrderLineId, x.MenuItemId, x.ItemName, x.Quantity, x.SpecialInstructions, (KitchenLineStatus)x.Status, Version(x.RowVersion))).ToArray(), ticket.Reservations.Select(x => new KitchenReservationResponse(x.InventoryReservationId, x.IngredientItemId, x.Quantity)).ToArray(), Version(ticket.RowVersion));
    private static KitchenStatusHistoryResponse Map(KitchenStatusHistory value) => new(value.Id, value.TicketId, value.LineId, value.PreviousStatus, value.NewStatus, value.Reason, value.ChangedBy, value.ChangedAt);
    private static KitchenResult<T> Validation<T>(params string[] errors) => KitchenResult<T>.Failure(KitchenResultStatus.Validation, errors);
    private static KitchenResult<T> NotFound<T>(params string[] errors) => KitchenResult<T>.Failure(KitchenResultStatus.NotFound, errors);
    private static KitchenResult<T> Conflict<T>(params string[] errors) => KitchenResult<T>.Failure(KitchenResultStatus.Conflict, errors);
    private static KitchenResult<T> VersionFailure<T>() => Conflict<T>("The Kitchen record changed. Reload it and try again.");
}
