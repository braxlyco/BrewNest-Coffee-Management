using BrewNest.Contracts.Production;
using BrewNest.Modules.Inventory.Domain.Entities;
using BrewNest.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Inventory.Application.Interfaces;

internal sealed class ProductionInventoryPort(InventoryDbContext database, TimeProvider timeProvider) : IProductionInventoryPort
{
    private DateTimeOffset Now => timeProvider.GetUtcNow();
    private DateOnly Today => DateOnly.FromDateTime(Now.UtcDateTime);

    public async Task<ProductionInventoryResult> ReserveAsync(string referenceId, ProductionInventoryRequirement[] requirements, string actor, CancellationToken cancellationToken)
    {
        if (!ValidReference(referenceId) || requirements.Length == 0 || requirements.Any(x => x.ItemId == Guid.Empty || x.Quantity <= 0)) return Failure("Production reservation requirements are invalid.");
        var normalized = requirements.GroupBy(x => x.ItemId).Select(x => new ProductionInventoryRequirement(x.Key, x.Sum(y => y.Quantity))).ToArray();
        var itemIds = normalized.Select(x => x.ItemId).ToArray();
        var lots = await database.StockLots.Where(x => itemIds.Contains(x.ItemId) && !x.IsBlocked && (x.ExpiryDate == null || x.ExpiryDate >= Today) && x.OnHandQuantity > x.ReservedQuantity).OrderBy(x => x.ExpiryDate == null).ThenBy(x => x.ExpiryDate).ThenBy(x => x.CreatedAt).ToArrayAsync(cancellationToken);
        foreach (var requirement in normalized)
            if (lots.Where(x => x.ItemId == requirement.ItemId).Sum(Available) < requirement.Quantity) return Failure("Not enough usable ingredient stock to release this production order. Check Inventory lots and reservations, then try again.");
        var allocations = new List<ProductionInventoryAllocation>();
        foreach (var requirement in normalized)
        {
            var remaining = requirement.Quantity;
            foreach (var lot in lots.Where(x => x.ItemId == requirement.ItemId))
            {
                if (remaining <= 0) break; var quantity = Math.Min(remaining, Available(lot)); if (quantity <= 0) continue;
                var reservation = new StockReservation { Id = Guid.NewGuid(), StockLotId = lot.Id, ItemId = lot.ItemId, WarehouseId = lot.WarehouseId, Quantity = quantity, ReferenceType = "ProductionOrder", ReferenceId = referenceId, Status = ReservationStatus.Active, Remarks = "Reserved on production release.", CreatedAt = Now, CreatedBy = actor };
                lot.ReservedQuantity += quantity; lot.ModifiedAt = Now; lot.ModifiedBy = actor; database.Reservations.Add(reservation); AddMovement(lot, StockMovementType.Reservation, quantity, referenceId, "Reserved on production release.", actor); allocations.Add(new(reservation.Id, lot.Id, lot.ItemId, quantity)); remaining -= quantity;
            }
        }
        return await SaveAsync(allocations, cancellationToken);
    }

    public async Task<ProductionInventoryResult> IssueAsync(string referenceId, ProductionInventoryUsage[] usages, string actor, CancellationToken cancellationToken)
    {
        if (!ValidReference(referenceId) || usages.Length == 0 || usages.Any(x => x.ItemId == Guid.Empty || x.ConsumptionQuantity < 0 || x.WastageQuantity < 0 || x.ConsumptionQuantity + x.WastageQuantity <= 0)) return Failure("Production material usage is invalid.");
        var normalized = usages.GroupBy(x => x.ItemId).Select(x => new ProductionInventoryUsage(x.Key, x.Sum(y => y.ConsumptionQuantity), x.Sum(y => y.WastageQuantity))).ToArray();
        var itemIds = normalized.Select(x => x.ItemId).ToArray();
        var reservations = await database.Reservations.Include(x => x.StockLot).Where(x => x.ReferenceType == "ProductionOrder" && x.ReferenceId == referenceId && itemIds.Contains(x.ItemId)).ToArrayAsync(cancellationToken);
        var lots = await database.StockLots.Where(x => itemIds.Contains(x.ItemId) && !x.IsBlocked && (x.ExpiryDate == null || x.ExpiryDate >= Today)).OrderBy(x => x.ExpiryDate == null).ThenBy(x => x.ExpiryDate).ThenBy(x => x.CreatedAt).ToArrayAsync(cancellationToken);
        foreach (var usage in normalized)
        {
            var ownReserved = reservations.Where(x => x.ItemId == usage.ItemId).Sum(Remaining);
            var unreserved = lots.Where(x => x.ItemId == usage.ItemId).Sum(Available);
            if (ownReserved + unreserved < usage.ConsumptionQuantity + usage.WastageQuantity) return Failure("Not enough usable ingredient stock for the entered actual quantities. Review the material requirements and Inventory lots before posting goods issue.");
        }
        var allocations = new List<ProductionInventoryAllocation>();
        foreach (var usage in normalized)
        {
            Allocate(usage.ItemId, usage.ConsumptionQuantity, false, referenceId, actor, reservations, lots, allocations);
            Allocate(usage.ItemId, usage.WastageQuantity, true, referenceId, actor, reservations, lots, allocations);
        }
        return await SaveAsync(allocations, cancellationToken);
    }

    public async Task<ProductionInventoryResult> ReleaseAsync(string referenceId, string reason, string actor, CancellationToken cancellationToken)
    {
        if (!ValidReference(referenceId) || string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500) return Failure("A production reference and release reason are required.");
        var reservations = await database.Reservations.Include(x => x.StockLot).Where(x => x.ReferenceType == "ProductionOrder" && x.ReferenceId == referenceId).ToArrayAsync(cancellationToken);
        var allocations = new List<ProductionInventoryAllocation>();
        foreach (var reservation in reservations)
        {
            var remaining = Remaining(reservation); if (remaining <= 0) continue;
            reservation.ReleasedQuantity += remaining; reservation.Status = ReservationStatus.Released; reservation.ModifiedAt = Now; reservation.ModifiedBy = actor;
            reservation.StockLot.ReservedQuantity -= remaining; reservation.StockLot.ModifiedAt = Now; reservation.StockLot.ModifiedBy = actor;
            AddMovement(reservation.StockLot, StockMovementType.ReservationRelease, -remaining, referenceId, reason, actor); allocations.Add(new(reservation.Id, reservation.StockLotId, reservation.ItemId, remaining));
        }
        return await SaveAsync(allocations, cancellationToken);
    }

    private void Allocate(Guid itemId, decimal requested, bool wastage, string referenceId, string actor, StockReservation[] reservations, StockLot[] lots, List<ProductionInventoryAllocation> allocations)
    {
        var remaining = requested; if (remaining <= 0) return;
        foreach (var reservation in reservations.Where(x => x.ItemId == itemId && Remaining(x) > 0))
        {
            if (remaining <= 0) break; var quantity = Math.Min(remaining, Remaining(reservation)); var lot = reservation.StockLot;
            lot.OnHandQuantity -= quantity; lot.ReservedQuantity -= quantity; reservation.ConsumedQuantity += quantity; reservation.Status = Remaining(reservation) == 0 ? ReservationStatus.Consumed : ReservationStatus.PartiallyConsumed; reservation.ModifiedAt = Now; reservation.ModifiedBy = actor; lot.ModifiedAt = Now; lot.ModifiedBy = actor;
            AddMovement(lot, wastage ? StockMovementType.Wastage : StockMovementType.GoodsIssue, -quantity, referenceId, wastage ? "Production wastage." : "Production material consumption.", actor); allocations.Add(new(reservation.Id, lot.Id, itemId, quantity)); remaining -= quantity;
        }
        foreach (var lot in lots.Where(x => x.ItemId == itemId && Available(x) > 0))
        {
            if (remaining <= 0) break; var quantity = Math.Min(remaining, Available(lot)); lot.OnHandQuantity -= quantity; lot.ModifiedAt = Now; lot.ModifiedBy = actor;
            AddMovement(lot, wastage ? StockMovementType.Wastage : StockMovementType.GoodsIssue, -quantity, referenceId, wastage ? "Production wastage above reservation." : "Production consumption above reservation.", actor); allocations.Add(new(Guid.Empty, lot.Id, itemId, quantity)); remaining -= quantity;
        }
    }

    private void AddMovement(StockLot lot, StockMovementType type, decimal quantity, string referenceId, string remarks, string actor) => database.Movements.Add(new() { Id = Guid.NewGuid(), StockLotId = lot.Id, ItemId = lot.ItemId, WarehouseId = lot.WarehouseId, MovementType = type, Quantity = quantity, OnHandAfter = lot.OnHandQuantity, ReservedAfter = lot.ReservedQuantity, ReferenceType = "ProductionOrder", ReferenceNumber = referenceId, Remarks = remarks, PerformedBy = actor, PerformedAt = Now });
    private async Task<ProductionInventoryResult> SaveAsync(List<ProductionInventoryAllocation> allocations, CancellationToken token) { try { await database.SaveChangesAsync(token); return new(true, allocations.ToArray(), []); } catch (DbUpdateConcurrencyException) { return Failure("Inventory changed during the production operation. Reload and try again."); } catch (DbUpdateException) { return Failure("The production inventory operation conflicted with current stock."); } }
    private static decimal Available(StockLot lot) => lot.OnHandQuantity - lot.ReservedQuantity;
    private static decimal Remaining(StockReservation reservation) => reservation.Quantity - reservation.ReleasedQuantity - reservation.ConsumedQuantity;
    private static bool ValidReference(string? value) => !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= 100;
    private static ProductionInventoryResult Failure(params string[] errors) => new(false, [], errors);
}
