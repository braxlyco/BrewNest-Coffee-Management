using BrewNest.Contracts.Catalog;
using BrewNest.Modules.Catalog.Application.Services;
using BrewNest.Modules.Catalog.Domain.BusinessRules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BrewNest.Modules.Catalog.Endpoints;

internal static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group=endpoints.MapGroup("/api/catalog").WithTags("Catalog").RequireAuthorization();
        group.MapGet("/units",(CatalogService s,CancellationToken t)=>Execute(s.GetUnitsAsync(t))).RequireAuthorization(CatalogPermissions.MasterDataRead);
        group.MapPost("/units",(CreateUnitOfMeasureRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.CreateUnitAsync(r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapPut("/units/{id:guid}",(Guid id,UpdateUnitOfMeasureRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.UpdateUnitAsync(id,r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapGet("/item-groups",(CatalogService s,CancellationToken t)=>Execute(s.GetGroupsAsync(t))).RequireAuthorization(CatalogPermissions.MasterDataRead);
        group.MapPost("/item-groups",(CreateItemGroupRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.CreateGroupAsync(r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapPut("/item-groups/{id:guid}",(Guid id,UpdateItemGroupRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.UpdateGroupAsync(id,r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapGet("/items",(string? type,CatalogService s,CancellationToken t)=>Execute(s.GetItemsAsync(type,t))).RequireAuthorization(CatalogPermissions.MasterDataRead);
        group.MapPost("/items",(CreateCatalogItemRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.CreateItemAsync(r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapPut("/items/{id:guid}",(Guid id,UpdateCatalogItemRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.UpdateItemAsync(id,r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapGet("/customers",(CatalogService s,CancellationToken t)=>Execute(s.GetCustomersAsync(t))).RequireAuthorization(CatalogPermissions.MasterDataRead);
        group.MapPost("/customers",(CreateCustomerRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.CreateCustomerAsync(r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapPut("/customers/{id:guid}",(Guid id,UpdateCustomerRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.UpdateCustomerAsync(id,r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapGet("/vendors",(CatalogService s,CancellationToken t)=>Execute(s.GetVendorsAsync(t))).RequireAuthorization(CatalogPermissions.MasterDataRead);
        group.MapPost("/vendors",(CreateVendorRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.CreateVendorAsync(r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapPut("/vendors/{id:guid}",(Guid id,UpdateVendorRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.UpdateVendorAsync(id,r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapGet("/dining-tables",(CatalogService s,CancellationToken t)=>Execute(s.GetTablesAsync(t))).RequireAuthorization(CatalogPermissions.MasterDataRead);
        group.MapPost("/dining-tables",(CreateDiningTableRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.CreateTableAsync(r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapPut("/dining-tables/{id:guid}",(Guid id,UpdateDiningTableRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.UpdateTableAsync(id,r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapGet("/warehouses",(CatalogService s,CancellationToken t)=>Execute(s.GetWarehousesAsync(t))).RequireAuthorization(CatalogPermissions.MasterDataRead);
        group.MapPost("/warehouses",(CreateWarehouseRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.CreateWarehouseAsync(r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapPut("/warehouses/{id:guid}",(Guid id,UpdateWarehouseRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.UpdateWarehouseAsync(id,r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapGet("/riders",(CatalogService s,CancellationToken t)=>Execute(s.GetRidersAsync(t))).RequireAuthorization(CatalogPermissions.MasterDataRead);
        group.MapPost("/riders",(CreateRiderRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.CreateRiderAsync(r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapPut("/riders/{id:guid}",(Guid id,UpdateRiderRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.UpdateRiderAsync(id,r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.MasterDataManage);
        group.MapGet("/recipes",(CatalogService s,CancellationToken t)=>Execute(s.GetRecipesAsync(t))).RequireAuthorization(CatalogPermissions.RecipesRead);
        group.MapPost("/recipes",(CreateRecipeRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.CreateRecipeAsync(r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.RecipesManage);
        group.MapPut("/recipes/{id:guid}",(Guid id,UpdateRecipeRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.UpdateRecipeAsync(id,r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.RecipesManage);
        group.MapGet("/inspection-requirements",(CatalogService s,CancellationToken t)=>Execute(s.GetInspectionRequirementsAsync(t))).RequireAuthorization(CatalogPermissions.QualityRead);
        group.MapPost("/inspection-requirements",(CreateInspectionRequirementRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.CreateInspectionRequirementAsync(r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.QualityManage);
        group.MapPut("/inspection-requirements/{id:guid}",(Guid id,UpdateInspectionRequirementRequest r,HttpContext h,CatalogService s,CancellationToken t)=>Execute(s.UpdateInspectionRequirementAsync(id,r,CatalogRequestContext.FromHttpContext(h),t))).RequireAuthorization(CatalogPermissions.QualityManage);
        group.MapGet("/status-history",(string? entityType,Guid? entityId,int? take,CatalogService s,CancellationToken t)=>Execute(s.GetStatusHistoryAsync(entityType,entityId,take??100,t))).RequireAuthorization(CatalogPermissions.MasterDataRead);
        return endpoints;
    }

    private static async Task<IResult> Execute<T>(Task<CatalogResult<T>> operation)
    {
        var result=await operation;if(result.Succeeded)return Results.Ok(result.Value);
        var(status,title)=result.Status switch{CatalogResultStatus.Validation=>(400,"Catalog validation failed"),CatalogResultStatus.NotFound=>(404,"Catalog record not found"),CatalogResultStatus.Conflict=>(409,"Catalog conflict"),_=>(500,"Catalog operation failed")};
        return Results.Problem(statusCode:status,title:title,detail:string.Join(' ',result.Errors),extensions:new Dictionary<string,object?>{{"errors",result.Errors}});
    }
}
