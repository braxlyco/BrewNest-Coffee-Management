using BrewNest.Contracts.Payments;
using BrewNest.Modules.Payments.Application.Services;
using BrewNest.Modules.Payments.Domain.BusinessRules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BrewNest.Modules.Payments.Endpoints;

internal static class PaymentsEndpoints
{
    public static IEndpointRouteBuilder MapPayments(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/payments").WithTags("Payments").RequireAuthorization();
        group.MapGet("/", (PaymentsService service, CancellationToken token) => Run(service.GetPayments(token))).RequireAuthorization(PaymentsPermissions.Read);
        group.MapPost("/", (CreatePaymentRequest request, HttpContext http, PaymentsService service, CancellationToken token) => Run(service.Create(request, PaymentsContext.From(http), token))).RequireAuthorization(PaymentsPermissions.Create);
        group.MapPost("/{id:guid}/verification", (Guid id, VerifyPaymentRequest request, HttpContext http, PaymentsService service, CancellationToken token) => Run(service.Verify(id, request, PaymentsContext.From(http), token))).RequireAuthorization(PaymentsPermissions.Verify);
        group.MapPost("/{id:guid}/settlement", (Guid id, SettlePaymentRequest request, HttpContext http, PaymentsService service, CancellationToken token) => Run(service.Settle(id, request, PaymentsContext.From(http), token))).RequireAuthorization(PaymentsPermissions.Settle);
        group.MapGet("/refunds", (PaymentsService service, CancellationToken token) => Run(service.GetRefunds(token))).RequireAuthorization(PaymentsPermissions.RefundsRead);
        group.MapPost("/refunds/{id:guid}/processing", (Guid id, ProcessRefundRequest request, HttpContext http, PaymentsService service, CancellationToken token) => Run(service.ProcessRefund(id, request, PaymentsContext.From(http), token))).RequireAuthorization(PaymentsPermissions.RefundsProcess);
        group.MapGet("/status-history", (PaymentsService service, CancellationToken token) => Run(service.GetHistory(token))).RequireAuthorization(PaymentsPermissions.Read);
        return endpoints;
    }

    private static async Task<IResult> Run<T>(Task<PaymentsResult<T>> task)
    {
        var result = await task;
        if (result.Succeeded) return Results.Ok(result.Value);
        var (status, title) = result.Status switch
        {
            PaymentsResultStatus.Validation => (400, "Payment validation failed"),
            PaymentsResultStatus.NotFound => (404, "Payment record not found"),
            PaymentsResultStatus.Conflict => (409, "Payment conflict"),
            _ => (500, "Payment operation failed")
        };
        return Results.Problem(statusCode: status, title: title, detail: string.Join(' ', result.Errors),
            extensions: new Dictionary<string, object?> { ["errors"] = result.Errors });
    }
}
