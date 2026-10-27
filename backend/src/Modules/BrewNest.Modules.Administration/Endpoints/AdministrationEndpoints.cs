using BrewNest.Contracts.Administration;
using BrewNest.Modules.Administration.Application.Services;
using BrewNest.Modules.Administration.Domain.BusinessRules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BrewNest.Modules.Administration.Endpoints;

internal static class AdministrationEndpoints
{
    public static IEndpointRouteBuilder MapAdministrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/administration").WithTags("Administration").RequireAuthorization();
        group.MapGet("/company", (AdministrationService service, CancellationToken token) => Execute(service.GetCompanyAsync(token))).RequireAuthorization(AdministrationPermissions.SettingsRead);
        group.MapPut("/company", (UpdateCompanySettingsRequest request, HttpContext http, AdministrationService service, CancellationToken token) => Execute(service.UpdateCompanyAsync(request, AdministrationRequestContext.FromHttpContext(http), token))).RequireAuthorization(AdministrationPermissions.SettingsManage);
        group.MapGet("/branches", (AdministrationService service, CancellationToken token) => Execute(service.GetBranchesAsync(token))).RequireAuthorization(AdministrationPermissions.SettingsRead);
        group.MapPost("/branches", (CreateBranchRequest request, HttpContext http, AdministrationService service, CancellationToken token) => Execute(service.CreateBranchAsync(request, AdministrationRequestContext.FromHttpContext(http), token))).RequireAuthorization(AdministrationPermissions.SettingsManage);
        group.MapPut("/branches/{id:guid}", (Guid id, UpdateBranchRequest request, HttpContext http, AdministrationService service, CancellationToken token) => Execute(service.UpdateBranchAsync(id, request, AdministrationRequestContext.FromHttpContext(http), token))).RequireAuthorization(AdministrationPermissions.SettingsManage);
        group.MapGet("/fiscal-years", (AdministrationService service, CancellationToken token) => Execute(service.GetFiscalYearsAsync(token))).RequireAuthorization(AdministrationPermissions.SettingsRead);
        group.MapPost("/fiscal-years", (CreateFiscalYearRequest request, HttpContext http, AdministrationService service, CancellationToken token) => Execute(service.CreateFiscalYearAsync(request, AdministrationRequestContext.FromHttpContext(http), token))).RequireAuthorization(AdministrationPermissions.SettingsManage);
        group.MapPut("/fiscal-years/{id:guid}", (Guid id, UpdateFiscalYearRequest request, HttpContext http, AdministrationService service, CancellationToken token) => Execute(service.UpdateFiscalYearAsync(id, request, AdministrationRequestContext.FromHttpContext(http), token))).RequireAuthorization(AdministrationPermissions.SettingsManage);
        group.MapGet("/document-numbers", (AdministrationService service, CancellationToken token) => Execute(service.GetDocumentNumbersAsync(token))).RequireAuthorization(AdministrationPermissions.SettingsRead);
        group.MapPost("/document-numbers", (CreateDocumentNumberSequenceRequest request, HttpContext http, AdministrationService service, CancellationToken token) => Execute(service.CreateDocumentNumberAsync(request, AdministrationRequestContext.FromHttpContext(http), token))).RequireAuthorization(AdministrationPermissions.SettingsManage);
        group.MapPut("/document-numbers/{id:guid}", (Guid id, UpdateDocumentNumberSequenceRequest request, HttpContext http, AdministrationService service, CancellationToken token) => Execute(service.UpdateDocumentNumberAsync(id, request, AdministrationRequestContext.FromHttpContext(http), token))).RequireAuthorization(AdministrationPermissions.SettingsManage);
        group.MapGet("/approvals", (AdministrationService service, CancellationToken token) => Execute(service.GetApprovalsAsync(token))).RequireAuthorization(AdministrationPermissions.ApprovalsRead);
        group.MapPost("/approvals", (CreateApprovalPolicyRequest request, HttpContext http, AdministrationService service, CancellationToken token) => Execute(service.CreateApprovalAsync(request, AdministrationRequestContext.FromHttpContext(http), token))).RequireAuthorization(AdministrationPermissions.ApprovalsManage);
        group.MapPut("/approvals/{id:guid}", (Guid id, UpdateApprovalPolicyRequest request, HttpContext http, AdministrationService service, CancellationToken token) => Execute(service.UpdateApprovalAsync(id, request, AdministrationRequestContext.FromHttpContext(http), token))).RequireAuthorization(AdministrationPermissions.ApprovalsManage);
        group.MapGet("/alerts", (AdministrationService service, CancellationToken token) => Execute(service.GetAlertsAsync(token))).RequireAuthorization(AdministrationPermissions.SettingsRead);
        group.MapPut("/alerts/{id:guid}", (Guid id, UpdateAlertSettingRequest request, HttpContext http, AdministrationService service, CancellationToken token) => Execute(service.UpdateAlertAsync(id, request, AdministrationRequestContext.FromHttpContext(http), token))).RequireAuthorization(AdministrationPermissions.SettingsManage);
        group.MapGet("/expense-allocations", (AdministrationService service, CancellationToken token) => Execute(service.GetExpenseAllocationsAsync(token))).RequireAuthorization(AdministrationPermissions.SettingsRead);
        group.MapPost("/expense-allocations", (CreateExpenseAllocationSettingRequest request, HttpContext http, AdministrationService service, CancellationToken token) => Execute(service.CreateExpenseAllocationAsync(request, AdministrationRequestContext.FromHttpContext(http), token))).RequireAuthorization(AdministrationPermissions.SettingsManage);
        group.MapPut("/expense-allocations/{id:guid}", (Guid id, UpdateExpenseAllocationSettingRequest request, HttpContext http, AdministrationService service, CancellationToken token) => Execute(service.UpdateExpenseAllocationAsync(id, request, AdministrationRequestContext.FromHttpContext(http), token))).RequireAuthorization(AdministrationPermissions.SettingsManage);
        group.MapGet("/audit", (int? take, AdministrationService service, CancellationToken token) => Execute(service.GetAuditAsync(take ?? 100, token))).RequireAuthorization(AdministrationPermissions.AuditRead);
        return endpoints;
    }

    private static async Task<IResult> Execute<T>(Task<AdministrationResult<T>> operation)
    {
        var result = await operation;
        if (result.Succeeded) return Results.Ok(result.Value);
        var (status, title) = result.Status switch
        {
            AdministrationResultStatus.Validation => (StatusCodes.Status400BadRequest, "Administration validation failed"),
            AdministrationResultStatus.NotFound => (StatusCodes.Status404NotFound, "Administration resource not found"),
            AdministrationResultStatus.Conflict => (StatusCodes.Status409Conflict, "Administration resource conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Administration operation failed")
        };
        return Results.Problem(statusCode: status, title: title, detail: string.Join(' ', result.Errors), extensions: new Dictionary<string, object?> { ["errors"] = result.Errors });
    }
}
