namespace BrewNest.Contracts.Identity;

/// <summary>Credentials submitted to the Identity login endpoint.</summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The user's password.</param>
public sealed record LoginRequest(string Email, string Password);

/// <summary>A newly issued access-token and refresh-token pair.</summary>
/// <param name="AccessToken">The signed JWT access token.</param>
/// <param name="AccessTokenExpiresAt">The UTC access-token expiration time.</param>
/// <param name="RefreshToken">The opaque, rotating refresh token.</param>
/// <param name="RefreshTokenExpiresAt">The UTC refresh-token expiration time.</param>
/// <param name="User">The authenticated user snapshot.</param>
public sealed record TokenResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    CurrentUserResponse User);

/// <summary>A request to rotate an existing refresh token.</summary>
/// <param name="RefreshToken">The refresh token to rotate.</param>
public sealed record RefreshTokenRequest(string RefreshToken);

/// <summary>A request to revoke a refresh token during logout.</summary>
/// <param name="RefreshToken">The refresh token to revoke.</param>
public sealed record LogoutRequest(string RefreshToken);

/// <summary>A request by an authenticated user to change their password.</summary>
/// <param name="CurrentPassword">The current password.</param>
/// <param name="NewPassword">The requested new password.</param>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

/// <summary>The authenticated user's identity and effective access grants.</summary>
/// <param name="Id">The internal user identifier.</param>
/// <param name="Email">The normalized login email.</param>
/// <param name="FullName">The user's display name.</param>
/// <param name="Roles">The user's active roles.</param>
/// <param name="Permissions">The user's effective permissions.</param>
public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string FullName,
    string[] Roles,
    string[] Permissions);

/// <summary>A compact user record for administration lists.</summary>
/// <param name="Id">The internal user identifier.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="FullName">The user's display name.</param>
/// <param name="IsActive">Whether the user may authenticate.</param>
/// <param name="Roles">The user's active roles.</param>
/// <param name="CreatedAt">The UTC creation time.</param>
/// <param name="Version">The optimistic-concurrency token encoded as Base64.</param>
public sealed record UserSummaryResponse(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    string[] Roles,
    DateTimeOffset CreatedAt,
    string Version);

/// <summary>Data required to create a BrewNest user.</summary>
/// <param name="Email">The unique login email.</param>
/// <param name="FullName">The user's display name.</param>
/// <param name="Password">The initial password.</param>
/// <param name="Roles">Initial active role names.</param>
public sealed record CreateUserRequest(string Email, string FullName, string Password, string[] Roles);

/// <summary>Editable user profile and activation data.</summary>
/// <param name="Email">The unique login email.</param>
/// <param name="FullName">The user's display name.</param>
/// <param name="IsActive">Whether the user may authenticate.</param>
/// <param name="Version">The version returned by the most recent query.</param>
public sealed record UpdateUserRequest(string Email, string FullName, bool IsActive, string Version);

/// <summary>Replaces a user's active role assignments.</summary>
/// <param name="RoleNames">The complete desired set of active role names.</param>
public sealed record SetUserRolesRequest(string[] RoleNames);

/// <summary>An administrator-initiated password reset.</summary>
/// <param name="NewPassword">The user's replacement password.</param>
public sealed record ResetPasswordRequest(string NewPassword);

/// <summary>A role and its currently granted permissions.</summary>
/// <param name="Id">The internal role identifier.</param>
/// <param name="Name">The unique role name.</param>
/// <param name="Description">The administrator-facing description.</param>
/// <param name="IsActive">Whether this role can grant access.</param>
/// <param name="Permissions">The role's active permission names.</param>
/// <param name="Version">The optimistic-concurrency token encoded as Base64.</param>
public sealed record RoleResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    string[] Permissions,
    string Version);

/// <summary>Data required to create an authorization role.</summary>
/// <param name="Name">The unique role name.</param>
/// <param name="Description">The role's purpose.</param>
public sealed record CreateRoleRequest(string Name, string Description);

/// <summary>Editable role data.</summary>
/// <param name="Name">The unique role name.</param>
/// <param name="Description">The role's purpose.</param>
/// <param name="IsActive">Whether the role can grant access.</param>
/// <param name="Version">The version returned by the most recent query.</param>
public sealed record UpdateRoleRequest(string Name, string Description, bool IsActive, string Version);

/// <summary>A permission available for assignment to roles.</summary>
/// <param name="Id">The internal permission identifier.</param>
/// <param name="Name">The stable permission name used by authorization policies.</param>
/// <param name="Description">The administrator-facing description.</param>
/// <param name="Group">The UI grouping label.</param>
public sealed record PermissionResponse(Guid Id, string Name, string Description, string Group);

/// <summary>Replaces a role's active permission grants.</summary>
/// <param name="PermissionNames">The complete desired set of permission names.</param>
public sealed record SetRolePermissionsRequest(string[] PermissionNames);
