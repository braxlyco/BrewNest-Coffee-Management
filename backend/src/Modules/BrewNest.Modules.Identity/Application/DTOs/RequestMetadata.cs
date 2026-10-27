using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BrewNest.Modules.Identity.Application.DTOs;

internal sealed record RequestMetadata(string Actor, string? IpAddress, string? UserAgent)
{
    public static RequestMetadata FromHttpContext(HttpContext context)
    {
        var actor = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.Identity?.Name
            ?? "anonymous";
        return new RequestMetadata(
            actor,
            context.Connection.RemoteIpAddress?.ToString(),
            context.Request.Headers.UserAgent.ToString());
    }
}
