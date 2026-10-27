using BrewNest.Contracts.Production;
using BrewNest.Modules.Production.Application.Services;
using BrewNest.Modules.Production.Domain.BusinessRules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
namespace BrewNest.Modules.Production.Endpoints;
internal static class ProductionEndpoints
{
    public static IEndpointRouteBuilder MapProductionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group=endpoints.MapGroup("/api/production").WithTags("Production").RequireAuthorization();
        group.MapGet("/references",(ProductionService s,CancellationToken t)=>Execute(s.GetReferencesAsync(t))).RequireAuthorization(ProductionPermissions.Read);
        group.MapGet("/item-names",(ProductionService s,CancellationToken t)=>Execute(s.GetItemNamesAsync(t))).RequireAuthorization(ProductionPermissions.Read);
        group.MapGet("/plans",(ProductionService s,CancellationToken t)=>Execute(s.GetPlansAsync(t))).RequireAuthorization(ProductionPermissions.Read);
        group.MapGet("/plans/{id:guid}",(Guid id,ProductionService s,CancellationToken t)=>Execute(s.GetPlanAsync(id,t))).RequireAuthorization(ProductionPermissions.Read);
        group.MapGet("/orders",(Guid? planId,string? status,ProductionService s,CancellationToken t)=>Execute(s.GetOrdersAsync(planId,status,t))).RequireAuthorization(ProductionPermissions.Read);
        group.MapGet("/orders/{id:guid}",(Guid id,ProductionService s,CancellationToken t)=>Execute(s.GetOrderAsync(id,t))).RequireAuthorization(ProductionPermissions.Read);
        group.MapGet("/status-history",(string? type,Guid? id,ProductionService s,CancellationToken t)=>Execute(s.GetHistoryAsync(type,id,t))).RequireAuthorization(ProductionPermissions.Read);
        group.MapPost("/plans",(CreateProductionPlanRequest r,HttpContext h,ProductionService s,CancellationToken t)=>Execute(s.CreatePlanAsync(r,ProductionRequestContext.FromHttpContext(h),t))).RequireAuthorization(ProductionPermissions.PlansManage);
        group.MapPost("/plans/{id:guid}/release",(Guid id,ChangeProductionStatusRequest r,HttpContext h,ProductionService s,CancellationToken t)=>Execute(s.ReleasePlanAsync(id,r,ProductionRequestContext.FromHttpContext(h),t))).RequireAuthorization(ProductionPermissions.PlansManage);
        group.MapPost("/plans/{id:guid}/cancel",(Guid id,ChangeProductionStatusRequest r,HttpContext h,ProductionService s,CancellationToken t)=>Execute(s.CancelPlanAsync(id,r,ProductionRequestContext.FromHttpContext(h),t))).RequireAuthorization(ProductionPermissions.PlansManage);
        group.MapPost("/orders/{id:guid}/release",(Guid id,ChangeProductionStatusRequest r,HttpContext h,ProductionService s,CancellationToken t)=>Execute(s.ReleaseOrderAsync(id,r,ProductionRequestContext.FromHttpContext(h),t))).RequireAuthorization(ProductionPermissions.OrdersRelease);
        group.MapPost("/orders/{id:guid}/issue",(Guid id,IssueProductionMaterialsRequest r,HttpContext h,ProductionService s,CancellationToken t)=>Execute(s.IssueMaterialsAsync(id,r,ProductionRequestContext.FromHttpContext(h),t))).RequireAuthorization(ProductionPermissions.MaterialsIssue);
        group.MapPost("/orders/{id:guid}/complete",(Guid id,ChangeProductionStatusRequest r,HttpContext h,ProductionService s,CancellationToken t)=>Execute(s.CompleteOrderAsync(id,r,ProductionRequestContext.FromHttpContext(h),t))).RequireAuthorization(ProductionPermissions.CompletionManage);
        group.MapPost("/orders/{id:guid}/cancel",(Guid id,ChangeProductionStatusRequest r,HttpContext h,ProductionService s,CancellationToken t)=>Execute(s.CancelOrderAsync(id,r,ProductionRequestContext.FromHttpContext(h),t))).RequireAuthorization(ProductionPermissions.OrdersRelease);
        return endpoints;
    }
    private static async Task<IResult> Execute<T>(Task<ProductionResult<T>> operation){var result=await operation;if(result.Succeeded)return Results.Ok(result.Value);var(status,title)=result.Status switch{ProductionResultStatus.Validation=>(400,"Production validation failed"),ProductionResultStatus.NotFound=>(404,"Production record not found"),ProductionResultStatus.Conflict=>(409,"Production conflict"),_=>(500,"Production operation failed")};return Results.Problem(statusCode:status,title:title,detail:string.Join(' ',result.Errors),extensions:new Dictionary<string,object?>{{"errors",result.Errors}});}
}
