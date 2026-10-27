using BrewNest.Contracts.Kitchen;
using BrewNest.Modules.Kitchen.Application.Services;
using BrewNest.Modules.Kitchen.Domain.BusinessRules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BrewNest.Modules.Kitchen.Endpoints;

internal static class KitchenEndpoints
{
    public static IEndpointRouteBuilder MapKitchenEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/kitchen").WithTags("Kitchen").RequireAuthorization();
        group.MapGet("/references/confirmed-orders", (KitchenService service, CancellationToken token) => Execute(service.GetConfirmedOrdersAsync(token))).RequireAuthorization(KitchenPermissions.QueueRead);
        group.MapGet("/tickets", (string? status, KitchenService service, CancellationToken token) => Execute(service.GetQueueAsync(status, token))).RequireAuthorization(KitchenPermissions.QueueRead);
        group.MapGet("/tickets/{id:guid}", (Guid id, KitchenService service, CancellationToken token) => Execute(service.GetAsync(id, token))).RequireAuthorization(KitchenPermissions.QueueRead);
        group.MapGet("/status-history", (Guid? ticketId, KitchenService service, CancellationToken token) => Execute(service.GetHistoryAsync(ticketId, token))).RequireAuthorization(KitchenPermissions.QueueRead);
        group.MapPost("/tickets", (GenerateKitchenTicketRequest request, HttpContext http, KitchenService service, CancellationToken token) => Execute(service.GenerateAsync(request, KitchenRequestContext.FromHttpContext(http), token))).RequireAuthorization(KitchenPermissions.TicketsGenerate);
        group.MapPost("/tickets/{id:guid}/status", (Guid id, ChangeKitchenTicketStatusRequest request, HttpContext http, KitchenService service, CancellationToken token) => Execute(service.ChangeTicketStatusAsync(id, request, KitchenRequestContext.FromHttpContext(http), token))).RequireAuthorization(KitchenPermissions.PreparationManage);
        group.MapPost("/tickets/{ticketId:guid}/lines/{lineId:guid}/status", (Guid ticketId, Guid lineId, ChangeKitchenLineStatusRequest request, HttpContext http, KitchenService service, CancellationToken token) => Execute(service.ChangeLineStatusAsync(ticketId, lineId, request, KitchenRequestContext.FromHttpContext(http), token))).RequireAuthorization(KitchenPermissions.PreparationManage);
        group.MapPost("/tickets/{id:guid}/cancel", (Guid id, ChangeKitchenTicketStatusRequest request, HttpContext http, KitchenService service, CancellationToken token) => Execute(service.CancelAsync(id, request.Reason, request.Version, KitchenRequestContext.FromHttpContext(http), token))).RequireAuthorization(KitchenPermissions.TicketsCancel);
        return endpoints;
    }

    private static async Task<IResult> Execute<T>(Task<KitchenResult<T>> operation)
    {
        var result = await operation;
        if (result.Succeeded) return Results.Ok(result.Value);
        var (status, title) = result.Status switch
        {
            KitchenResultStatus.Validation => (400, "Kitchen validation failed"),
            KitchenResultStatus.NotFound => (404, "Kitchen record not found"),
            KitchenResultStatus.Conflict => (409, "Kitchen conflict"),
            _ => (500, "Kitchen operation failed")
        };
        return Results.Problem(statusCode: status, title: title, detail: string.Join(' ', result.Errors), extensions: new Dictionary<string, object?> { ["errors"] = result.Errors });
    }
}
