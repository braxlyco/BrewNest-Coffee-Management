using BrewNest.Contracts.Payments;
using BrewNest.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Sales.Application.Interfaces;

internal sealed class PaymentsSalesPort(SalesDbContext database) : IPaymentsSalesPort
{
    public async Task<PaymentsBillSnapshot?> GetBillAsync(Guid salesOrderId, string billNumber, CancellationToken cancellationToken)
    {
        var order = await database.Orders.AsNoTracking().Include(x => x.Bill)
            .SingleOrDefaultAsync(x => x.Id == salesOrderId && x.Bill != null && x.Bill.BillNumber == billNumber, cancellationToken);
        return order?.Bill is null ? null : new(order.Id, order.OrderNumber, order.Bill.BillNumber, order.Bill.NetAmount, order.Status.ToString());
    }
}
