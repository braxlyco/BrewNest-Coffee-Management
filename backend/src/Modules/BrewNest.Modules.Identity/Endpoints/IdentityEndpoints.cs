using BrewNest.Contracts.Identity;
using BrewNest.Modules.Identity.Application.DTOs;
using BrewNest.Modules.Identity.Application.Interfaces;
using BrewNest.Modules.Identity.Domain.BusinessRules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BrewNest.Modules.Identity.Endpoints;

internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/identity").WithTags("Identity");

        group.MapPost("/auth/login", async (
            LoginRequest request,
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.LoginAsync(request, RequestMetadata.FromHttpContext(context), cancellationToken)))
            .AllowAnonymous();

        group.MapPost("/auth/refresh", async (
            RefreshTokenRequest request,
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.RefreshAsync(request, RequestMetadata.FromHttpContext(context), cancellationToken)))
            .AllowAnonymous();

        group.MapPost("/auth/logout", async (
            LogoutRequest request,
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.LogoutAsync(request, RequestMetadata.FromHttpContext(context), cancellationToken), noContent: true))
            .AllowAnonymous();

        group.MapPost("/auth/revoke-all", async (
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.RevokeAllAsync(context.User, RequestMetadata.FromHttpContext(context), cancellationToken), noContent: true))
            .RequireAuthorization();

        group.MapGet("/auth/me", async (
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.GetCurrentUserAsync(context.User, cancellationToken)))
            .RequireAuthorization();

        group.MapPost("/auth/change-password", async (
            ChangePasswordRequest request,
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.ChangePasswordAsync(context.User, request, RequestMetadata.FromHttpContext(context), cancellationToken), noContent: true))
            .RequireAuthorization();

        group.MapGet("/users", async (IIdentityService service, CancellationToken cancellationToken) =>
            ToHttpResult(await service.GetUsersAsync(cancellationToken)))
            .RequireAuthorization(IdentityPermissions.UsersRead);

        group.MapPost("/users", async (
            CreateUserRequest request,
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.CreateUserAsync(request, RequestMetadata.FromHttpContext(context), cancellationToken)))
            .RequireAuthorization(IdentityPermissions.UsersManage);

        group.MapPut("/users/{userId:guid}", async (
            Guid userId,
            UpdateUserRequest request,
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.UpdateUserAsync(userId, request, RequestMetadata.FromHttpContext(context), cancellationToken)))
            .RequireAuthorization(IdentityPermissions.UsersManage);

        group.MapPut("/users/{userId:guid}/roles", async (
            Guid userId,
            SetUserRolesRequest request,
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.SetUserRolesAsync(userId, request, RequestMetadata.FromHttpContext(context), cancellationToken), noContent: true))
            .RequireAuthorization(IdentityPermissions.UsersManage);

        group.MapPost("/users/{userId:guid}/reset-password", async (
            Guid userId,
            ResetPasswordRequest request,
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.ResetPasswordAsync(userId, request, RequestMetadata.FromHttpContext(context), cancellationToken), noContent: true))
            .RequireAuthorization(IdentityPermissions.UsersManage);

        group.MapGet("/roles", async (IIdentityService service, CancellationToken cancellationToken) =>
            ToHttpResult(await service.GetRolesAsync(cancellationToken)))
            .RequireAuthorization(IdentityPermissions.RolesRead);

        group.MapPost("/roles", async (
            CreateRoleRequest request,
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.CreateRoleAsync(request, RequestMetadata.FromHttpContext(context), cancellationToken)))
            .RequireAuthorization(IdentityPermissions.RolesManage);

        group.MapPut("/roles/{roleId:guid}", async (
            Guid roleId,
            UpdateRoleRequest request,
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.UpdateRoleAsync(roleId, request, RequestMetadata.FromHttpContext(context), cancellationToken)))
            .RequireAuthorization(IdentityPermissions.RolesManage);

        group.MapGet("/permissions", async (IIdentityService service, CancellationToken cancellationToken) =>
            ToHttpResult(await service.GetPermissionsAsync(cancellationToken)))
            .RequireAuthorization(IdentityPermissions.PermissionsRead);

        group.MapPut("/roles/{roleId:guid}/permissions", async (
            Guid roleId,
            SetRolePermissionsRequest request,
            HttpContext context,
            IIdentityService service,
            CancellationToken cancellationToken) =>
            ToHttpResult(await service.SetRolePermissionsAsync(roleId, request, RequestMetadata.FromHttpContext(context), cancellationToken), noContent: true))
            .RequireAuthorization(IdentityPermissions.PermissionsManage);

        return endpoints;
    }

    private static IResult ToHttpResult<T>(IdentityServiceResult<T> result, bool noContent = false)
    {
        if (result.Succeeded)
        {
            return noContent ? Results.NoContent() : Results.Ok(result.Value);
        }

        var (statusCode, title) = result.Status switch
        {
            IdentityResultStatus.Validation => (StatusCodes.Status400BadRequest, "Identity validation failed"),
            IdentityResultStatus.Unauthorized => (StatusCodes.Status401Unauthorized, "Authentication failed"),
            IdentityResultStatus.Forbidden => (StatusCodes.Status403Forbidden, "Authorization failed"),
            IdentityResultStatus.NotFound => (StatusCodes.Status404NotFound, "Identity resource not found"),
            IdentityResultStatus.Conflict => (StatusCodes.Status409Conflict, "Identity resource conflict"),
            IdentityResultStatus.ConcurrencyConflict => (StatusCodes.Status409Conflict, "The identity resource changed"),
            _ => (StatusCodes.Status500InternalServerError, "Identity operation failed")
        };
        return Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: string.Join(' ', result.Errors),
            extensions: new Dictionary<string, object?> { ["errors"] = result.Errors });
    }
}
