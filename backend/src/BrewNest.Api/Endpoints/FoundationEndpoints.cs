using BrewNest.BuildingBlocks.Modules;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace BrewNest.Api.Endpoints;

internal static class FoundationEndpoints
{
    public static IEndpointRouteBuilder MapFoundationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = static async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    status = report.Status.ToString(),
                    checkedAt = DateTimeOffset.UtcNow
                });
            }
        }).AllowAnonymous();

        endpoints.MapGet("/api/modules", (IEnumerable<IBrewNestModule> modules) =>
                Results.Ok(modules.Select(module => module.Name).OrderBy(name => name)))
            .WithName("GetRegisteredModules")
            .WithTags("Foundation")
            .AllowAnonymous();

        return endpoints;
    }
}
