using BrewNest.Contracts.Kitchen;
using BrewNest.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Sales.Application.Interfaces;

internal sealed class KitchenSalesOrderPort(SalesDbContext database) : IKitchenSalesOrderPort
{
    public Task<KitchenSalesOrderSnapshot[]> GetConfirmedOrdersAsync(CancellationToken cancellationToken) =>
        database.Orders.AsNoTracking()
            .Where(order => order.Status == Domain.Entities.OrderStatus.Confirmed)
            .OrderByDescending(order => order.CreatedAt)
            .Take(100)
            .Select(order => new KitchenSalesOrderSnapshot(
                order.Id,
                order.OrderNumber,
                order.OrderType.ToString(),
                order.Status.ToString(),
                order.Lines.OrderBy(line => line.AddedAt).Select(line => new KitchenSalesLineSnapshot(
                    line.Id,
                    line.MenuItemId,
                    line.ItemName,
                    line.Quantity,
                    line.SpecialInstructions,
                    line.IsCancelled)).ToArray()))
            .ToArrayAsync(cancellationToken);

    public Task<KitchenSalesOrderSnapshot?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken) =>
        database.Orders.AsNoTracking()
            .Where(order => order.Id == orderId)
            .Select(order => new KitchenSalesOrderSnapshot(
                order.Id,
                order.OrderNumber,
                order.OrderType.ToString(),
                order.Status.ToString(),
                order.Lines.OrderBy(line => line.AddedAt).Select(line => new KitchenSalesLineSnapshot(
                    line.Id,
                    line.MenuItemId,
                    line.ItemName,
                    line.Quantity,
                    line.SpecialInstructions,
                    line.IsCancelled)).ToArray()))
            .SingleOrDefaultAsync(cancellationToken);
}
