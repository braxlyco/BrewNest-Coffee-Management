using System.Security.Claims;
using BrewNest.Contracts.Identity;
using BrewNest.Modules.Identity.Application.DTOs;

namespace BrewNest.Modules.Identity.Application.Interfaces;

internal interface IIdentityService
{
    Task<IdentityServiceResult<TokenResponse>> LoginAsync(LoginRequest request, RequestMetadata metadata, CancellationToken cancellationToken);
    Task<IdentityServiceResult<TokenResponse>> RefreshAsync(RefreshTokenRequest request, RequestMetadata metadata, CancellationToken cancellationToken);
    Task<IdentityServiceResult<bool>> LogoutAsync(LogoutRequest request, RequestMetadata metadata, CancellationToken cancellationToken);
    Task<IdentityServiceResult<bool>> RevokeAllAsync(ClaimsPrincipal principal, RequestMetadata metadata, CancellationToken cancellationToken);
    Task<IdentityServiceResult<CurrentUserResponse>> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
    Task<IdentityServiceResult<bool>> ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordRequest request, RequestMetadata metadata, CancellationToken cancellationToken);
    Task<IdentityServiceResult<UserSummaryResponse[]>> GetUsersAsync(CancellationToken cancellationToken);
    Task<IdentityServiceResult<UserSummaryResponse>> CreateUserAsync(CreateUserRequest request, RequestMetadata metadata, CancellationToken cancellationToken);
    Task<IdentityServiceResult<UserSummaryResponse>> UpdateUserAsync(Guid userId, UpdateUserRequest request, RequestMetadata metadata, CancellationToken cancellationToken);
    Task<IdentityServiceResult<bool>> SetUserRolesAsync(Guid userId, SetUserRolesRequest request, RequestMetadata metadata, CancellationToken cancellationToken);
    Task<IdentityServiceResult<bool>> ResetPasswordAsync(Guid userId, ResetPasswordRequest request, RequestMetadata metadata, CancellationToken cancellationToken);
    Task<IdentityServiceResult<RoleResponse[]>> GetRolesAsync(CancellationToken cancellationToken);
    Task<IdentityServiceResult<RoleResponse>> CreateRoleAsync(CreateRoleRequest request, RequestMetadata metadata, CancellationToken cancellationToken);
    Task<IdentityServiceResult<RoleResponse>> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, RequestMetadata metadata, CancellationToken cancellationToken);
    Task<IdentityServiceResult<PermissionResponse[]>> GetPermissionsAsync(CancellationToken cancellationToken);
    Task<IdentityServiceResult<bool>> SetRolePermissionsAsync(Guid roleId, SetRolePermissionsRequest request, RequestMetadata metadata, CancellationToken cancellationToken);
}
