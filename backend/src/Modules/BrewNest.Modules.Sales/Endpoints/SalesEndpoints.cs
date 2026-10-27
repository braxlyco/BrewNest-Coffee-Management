using BrewNest.Contracts.Sales;
using BrewNest.Modules.Sales.Application.Services;
using BrewNest.Modules.Sales.Domain.BusinessRules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BrewNest.Modules.Sales.Endpoints;

internal static class SalesEndpoints
{
    public static IEndpointRouteBuilder MapSalesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group=endpoints.MapGroup("/api/sales").WithTags("Sales").RequireAuthorization();
        group.MapGet("/orders",(string? status,SalesService service,CancellationToken token)=>Execute(service.GetOrdersAsync(status,token))).RequireAuthorization(SalesPermissions.OrdersRead);
        group.MapGet("/orders/{id:guid}",(Guid id,SalesService service,CancellationToken token)=>Execute(service.GetOrderAsync(id,token))).RequireAuthorization(SalesPermissions.OrdersRead);
        group.MapGet("/references",(SalesService service,CancellationToken token)=>Execute(service.GetReferencesAsync(token))).RequireAuthorization(SalesPermissions.OrdersRead);
        group.MapPost("/orders",(CreateSalesOrderRequest request,HttpContext http,SalesService service,CancellationToken token)=>Execute(service.CreateAsync(request,SalesRequestContext.FromHttpContext(http),token))).RequireAuthorization(SalesPermissions.OrdersManage);
        group.MapPut("/orders/{id:guid}",(Guid id,UpdateSalesOrderRequest request,HttpContext http,SalesService service,CancellationToken token)=>Execute(service.UpdateAsync(id,request,SalesRequestContext.FromHttpContext(http),token))).RequireAuthorization(SalesPermissions.OrdersManage);
        group.MapPost("/orders/{id:guid}/lines",(Guid id,AddSalesOrderLineRequest request,HttpContext http,SalesService service,CancellationToken token)=>Execute(service.AddLineAsync(id,request,SalesRequestContext.FromHttpContext(http),token))).RequireAuthorization(SalesPermissions.OrdersManage);
        group.MapPut("/orders/{id:guid}/lines/{lineId:guid}",(Guid id,Guid lineId,UpdateSalesOrderLineRequest request,HttpContext http,SalesService service,CancellationToken token)=>Execute(service.UpdateLineAsync(id,lineId,request,SalesRequestContext.FromHttpContext(http),token))).RequireAuthorization(SalesPermissions.OrdersManage);
        group.MapPost("/orders/{id:guid}/discounts",(Guid id,RequestSalesDiscountRequest request,HttpContext http,SalesService service,CancellationToken token)=>Execute(service.RequestDiscountAsync(id,request,SalesRequestContext.FromHttpContext(http),token))).RequireAuthorization(SalesPermissions.DiscountsRequest);
        group.MapPost("/orders/{id:guid}/discounts/{discountId:guid}/decision",(Guid id,Guid discountId,DecideSalesDiscountRequest request,HttpContext http,SalesService service,CancellationToken token)=>Execute(service.DecideDiscountAsync(id,discountId,request,SalesRequestContext.FromHttpContext(http),token))).RequireAuthorization(SalesPermissions.DiscountsApprove);
        group.MapPost("/orders/{id:guid}/confirm",(Guid id,ChangeSalesOrderStatusRequest request,HttpContext http,SalesService service,CancellationToken token)=>Execute(service.ConfirmAsync(id,request,SalesRequestContext.FromHttpContext(http),token))).RequireAuthorization(SalesPermissions.BillingManage);
        group.MapPost("/orders/{id:guid}/bill",(Guid id,ChangeSalesOrderStatusRequest request,HttpContext http,SalesService service,CancellationToken token)=>Execute(service.BillAsync(id,request,SalesRequestContext.FromHttpContext(http),token))).RequireAuthorization(SalesPermissions.BillingManage);
        group.MapPost("/orders/{id:guid}/close",(Guid id,ChangeSalesOrderStatusRequest request,HttpContext http,SalesService service,CancellationToken token)=>Execute(service.CloseAsync(id,request,SalesRequestContext.FromHttpContext(http),token))).RequireAuthorization(SalesPermissions.OrdersClose);
        group.MapPost("/orders/{id:guid}/cancel",(Guid id,ChangeSalesOrderStatusRequest request,HttpContext http,SalesService service,CancellationToken token)=>Execute(service.CancelAsync(id,request,SalesRequestContext.FromHttpContext(http),token))).RequireAuthorization(SalesPermissions.OrdersClose);
        group.MapGet("/status-history",(Guid? orderId,SalesService service,CancellationToken token)=>Execute(service.GetHistoryAsync(orderId,token))).RequireAuthorization(SalesPermissions.OrdersRead);
        return endpoints;
    }

    private static async Task<IResult> Execute<T>(Task<SalesResult<T>> operation){var result=await operation;if(result.Succeeded)return Results.Ok(result.Value);var(status,title)=result.Status switch{SalesResultStatus.Validation=>(400,"Sales validation failed"),SalesResultStatus.NotFound=>(404,"Sales order not found"),SalesResultStatus.Conflict=>(409,"Sales conflict"),_=>(500,"Sales operation failed")};return Results.Problem(statusCode:status,title:title,detail:string.Join(' ',result.Errors),extensions:new Dictionary<string,object?>{{"errors",result.Errors}});}
}
