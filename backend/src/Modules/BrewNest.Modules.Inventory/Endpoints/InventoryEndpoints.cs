using BrewNest.Contracts.Inventory;
using BrewNest.Modules.Inventory.Application.Services;
using BrewNest.Modules.Inventory.Domain.BusinessRules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
namespace BrewNest.Modules.Inventory.Endpoints;
internal static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g=endpoints.MapGroup("/api/inventory").WithTags("Inventory").RequireAuthorization();
        g.MapGet("/references",async (IInventoryCatalogReferenceLookup lookup,CancellationToken token)=>Results.Ok(await lookup.GetAsync(token))).RequireAuthorization(InventoryPermissions.StockRead);
        g.MapGet("/lots",(Guid? itemId,Guid? warehouseId,InventoryService s,CancellationToken t)=>Execute(s.GetLotsAsync(itemId,warehouseId,t))).RequireAuthorization(InventoryPermissions.StockRead);
        g.MapPost("/receipts",(GoodsReceiptRequest r,HttpContext h,InventoryService s,CancellationToken t)=>Execute(s.ReceiveAsync(r,InventoryRequestContext.FromHttpContext(h),t))).RequireAuthorization(InventoryPermissions.StockManage);
        g.MapGet("/reservations",(string? referenceType,string? referenceId,InventoryService s,CancellationToken t)=>Execute(s.GetReservationsAsync(referenceType,referenceId,t))).RequireAuthorization(InventoryPermissions.StockRead);
        g.MapPost("/reservations",(CreateReservationRequest r,HttpContext h,InventoryService s,CancellationToken t)=>Execute(s.ReserveForApiAsync(r,InventoryRequestContext.FromHttpContext(h),t))).RequireAuthorization(InventoryPermissions.ReservationsManage);
        g.MapPost("/reservations/{id:guid}/release",(Guid id,ReleaseReservationRequest r,HttpContext h,InventoryService s,CancellationToken t)=>Execute(s.ReleaseForApiAsync(id,r,InventoryRequestContext.FromHttpContext(h),t))).RequireAuthorization(InventoryPermissions.ReservationsManage);
        g.MapPost("/issues",(GoodsIssueRequest r,HttpContext h,InventoryService s,CancellationToken t)=>Execute(s.IssueAsync(r,InventoryRequestContext.FromHttpContext(h),t))).RequireAuthorization(InventoryPermissions.StockManage);
        g.MapPost("/adjustments",(StockAdjustmentRequest r,HttpContext h,InventoryService s,CancellationToken t)=>Execute(s.AdjustAsync(r,InventoryRequestContext.FromHttpContext(h),t))).RequireAuthorization(InventoryPermissions.AdjustmentsManage);
        g.MapPost("/wastage",(WastageRequest r,HttpContext h,InventoryService s,CancellationToken t)=>Execute(s.WasteAsync(r,InventoryRequestContext.FromHttpContext(h),t))).RequireAuthorization(InventoryPermissions.WastageManage);
        g.MapPut("/lots/{id:guid}/block",(Guid id,SetLotBlockRequest r,HttpContext h,InventoryService s,CancellationToken t)=>Execute(s.SetBlockAsync(id,r,InventoryRequestContext.FromHttpContext(h),t))).RequireAuthorization(InventoryPermissions.StockManage);
        g.MapGet("/movements",(Guid? lotId,Guid? itemId,Guid? warehouseId,int? take,InventoryService s,CancellationToken t)=>Execute(s.GetMovementsAsync(lotId,itemId,warehouseId,take??200,t))).RequireAuthorization(InventoryPermissions.StockRead);
        g.MapGet("/expiry-alerts",(Guid? warehouseId,InventoryService s,CancellationToken t)=>Execute(s.GetExpiryAlertsAsync(warehouseId,t))).RequireAuthorization(InventoryPermissions.ExpiryRead);
        return endpoints;
    }
    private static async Task<IResult> Execute<T>(Task<InventoryResult<T>> operation){var result=await operation;if(result.Succeeded)return Results.Ok(result.Value);var(status,title)=result.Status switch{InventoryResultStatus.Validation=>(400,"Inventory validation failed"),InventoryResultStatus.NotFound=>(404,"Inventory record not found"),InventoryResultStatus.Conflict=>(409,"Inventory conflict"),_=>(500,"Inventory operation failed")};return Results.Problem(statusCode:status,title:title,detail:string.Join(' ',result.Errors),extensions:new Dictionary<string,object?>{{"errors",result.Errors}});}
}
