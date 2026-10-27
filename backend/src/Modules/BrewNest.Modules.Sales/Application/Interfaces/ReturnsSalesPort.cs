using BrewNest.Contracts.Returns;using BrewNest.Modules.Sales.Domain.Entities;using BrewNest.Modules.Sales.Infrastructure.Persistence;using Microsoft.EntityFrameworkCore;
namespace BrewNest.Modules.Sales.Application.Interfaces;
internal sealed class ReturnsSalesPort(SalesDbContext database,TimeProvider clock):IReturnsSalesPort
{
 public async Task<ReturnsBillSnapshot?> FindBillAsync(string billNumber,CancellationToken t){var x=await database.Orders.AsNoTracking().Include(o=>o.Lines).Include(o=>o.Bill).SingleOrDefaultAsync(o=>o.Bill!=null&&o.Bill.BillNumber==billNumber,t);return x?.Bill is null?null:new(x.Id,x.OrderNumber,x.Status.ToString(),x.Bill.Id,x.Bill.BillNumber,x.Bill.NetAmount,x.Lines.Where(l=>!l.IsCancelled).Select(l=>new ReturnsSalesLine(l.Id,l.MenuItemId,l.ItemCode,l.ItemName,l.Quantity,l.UnitPrice)).ToArray());}
 public async Task<ReturnsOrderSnapshot?> GetOrderAsync(Guid id,CancellationToken t){var x=await database.Orders.AsNoTracking().AsSplitQuery().Include(o=>o.Lines).Include(o=>o.Bill).Include(o=>o.Discounts).SingleOrDefaultAsync(o=>o.Id==id,t);if(x is null)return null;var gross=x.Lines.Where(l=>!l.IsCancelled).Sum(l=>l.Quantity*l.UnitPrice);var discount=x.Discounts.Where(d=>d.Status==DiscountStatus.Approved).OrderByDescending(d=>d.ApprovedAt).Select(d=>d.Amount).FirstOrDefault();return new(x.Id,x.OrderNumber,x.Status.ToString(),x.Bill?.BillNumber,x.Bill?.NetAmount??decimal.Round(gross-discount,2));}
 public async Task<ReturnsPortResult> VoidUnpaidAsync(Guid id,string reason,string actor,CancellationToken t)
 {
  if(string.IsNullOrWhiteSpace(reason)||reason.Length>500)return new(false,["A void reason up to 500 characters is required."]);
  var order=await database.Orders.SingleOrDefaultAsync(x=>x.Id==id,t);
  if(order is null)return new(false,["Sales order not found."]);
  if(order.Status==OrderStatus.Cancelled)return new(true,[]);
  if(order.Status is not(OrderStatus.Draft or OrderStatus.Confirmed or OrderStatus.Billed or OrderStatus.Closed))return new(false,["The Sales order cannot be voided in its current state."]);
  var previous=order.Status;
  var now=clock.GetUtcNow();
  order.Status=OrderStatus.Cancelled;
  order.ModifiedAt=now;
  order.ModifiedBy=actor;
  database.StatusHistory.Add(new(){Id=Guid.NewGuid(),OrderId=order.Id,PreviousStatus=previous.ToString(),NewStatus=OrderStatus.Cancelled.ToString(),Reason=reason.Trim(),ChangedBy=actor,ChangedAt=now});
  try{await database.SaveChangesAsync(t);return new(true,[]);}
  catch(DbUpdateConcurrencyException){return new(false,["The Sales order changed. Reload and retry the void decision."]);}
 }
 public async Task<ReturnsPortResult> VoidPaidAsync(Guid id,string reason,string actor,CancellationToken t)
 {
  if(string.IsNullOrWhiteSpace(reason)||reason.Length>500)return new(false,["A paid-void reason up to 500 characters is required."]);
  var order=await database.Orders.Include(x=>x.Bill).SingleOrDefaultAsync(x=>x.Id==id,t);
  if(order is null)return new(false,["Sales order not found."]);
  if(order.Bill is null)return new(false,["A paid void requires an issued bill."]);
  if(order.Status==OrderStatus.Cancelled)return new(true,[]);
  if(order.Status is not(OrderStatus.Billed or OrderStatus.Closed))return new(false,["Only billed or closed orders may use the paid-void path."]);
  var previous=order.Status;
  var now=clock.GetUtcNow();
  order.Status=OrderStatus.Cancelled;
  order.ModifiedAt=now;
  order.ModifiedBy=actor;
  database.StatusHistory.Add(new(){Id=Guid.NewGuid(),OrderId=order.Id,PreviousStatus=previous.ToString(),NewStatus=OrderStatus.Cancelled.ToString(),Reason=reason.Trim(),ChangedBy=actor,ChangedAt=now});
  try{await database.SaveChangesAsync(t);return new(true,[]);}
  catch(DbUpdateConcurrencyException){return new(false,["The Sales order changed. Reload and retry the void decision."]);}
 }
}
