using System.Net.Mail;
using System.Security.Claims;
using BrewNest.Contracts.Identity;
using BrewNest.Modules.Identity.Application.DTOs;
using BrewNest.Modules.Identity.Application.Interfaces;
using BrewNest.Modules.Identity.Domain.Entities;
using BrewNest.Modules.Identity.Domain.Enums;
using BrewNest.Modules.Identity.Infrastructure.Integrations;
using BrewNest.Modules.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Identity.Application.Services;

internal sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    SignInManager<ApplicationUser> signInManager,
    IdentityDbContext dbContext,
    IJwtTokenService tokenService,
    TimeProvider timeProvider) : IIdentityService
{
    public async Task<IdentityServiceResult<TokenResponse>> LoginAsync(
        LoginRequest request,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
        {
            AddAudit(user?.Id, SecurityEventType.LoginFailed, false, email, metadata, "Invalid credentials or inactive account.");
            await dbContext.SaveChangesAsync(cancellationToken);
            return IdentityServiceResult<TokenResponse>.Failure(IdentityResultStatus.Unauthorized, "Invalid email or password.");
        }

        var passwordResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!passwordResult.Succeeded)
        {
            AddAudit(user.Id, SecurityEventType.LoginFailed, false, email, metadata, passwordResult.IsLockedOut ? "Account locked." : "Invalid password.");
            await dbContext.SaveChangesAsync(cancellationToken);
            return IdentityServiceResult<TokenResponse>.Failure(IdentityResultStatus.Unauthorized, "Invalid email or password.");
        }

        var issued = await tokenService.CreateAsync(user, Guid.NewGuid(), metadata.IpAddress, cancellationToken);
        dbContext.RefreshTokens.Add(issued.RefreshTokenEntity);
        AddAudit(user.Id, SecurityEventType.LoginSucceeded, true, user.Id.ToString(), metadata, "Interactive login succeeded.");
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityServiceResult<TokenResponse>.Success(ToTokenResponse(issued));
    }

    public async Task<IdentityServiceResult<TokenResponse>> RefreshAsync(
        RefreshTokenRequest request,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return IdentityServiceResult<TokenResponse>.Failure(IdentityResultStatus.Validation, "A refresh token is required.");
        }

        var hash = RefreshTokenHash.Compute(request.RefreshToken);
        var token = await dbContext.RefreshTokens
            .Include(candidate => candidate.User)
            .SingleOrDefaultAsync(candidate => candidate.TokenHash == hash, cancellationToken);
        var now = timeProvider.GetUtcNow();
        if (token is null)
        {
            AddAudit(null, SecurityEventType.LoginFailed, false, "anonymous", metadata, "Unknown refresh token.");
            await dbContext.SaveChangesAsync(cancellationToken);
            return IdentityServiceResult<TokenResponse>.Failure(IdentityResultStatus.Unauthorized, "The refresh token is invalid.");
        }

        if (!token.IsActive(now) || !token.User.IsActive)
        {
            if (token.ReplacedByTokenId is not null)
            {
                await RevokeTokenFamilyAsync(token.UserId, token.FamilyId, now, metadata, "Refresh-token replay detected.", cancellationToken);
                AddAudit(token.UserId, SecurityEventType.TokenReplayDetected, false, token.UserId.ToString(), metadata, "A rotated token was reused; its family was revoked.");
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return IdentityServiceResult<TokenResponse>.Failure(IdentityResultStatus.Unauthorized, "The refresh token is expired or revoked.");
        }

        var replacement = await tokenService.CreateAsync(token.User, token.FamilyId, metadata.IpAddress, cancellationToken);
        token.Revoke(now, metadata.IpAddress, "Rotated.", replacement.RefreshTokenEntity.Id);
        dbContext.RefreshTokens.Add(replacement.RefreshTokenEntity);
        AddAudit(token.UserId, SecurityEventType.TokenRefreshed, true, token.UserId.ToString(), metadata, "Refresh token rotated.");

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityServiceResult<TokenResponse>.Failure(IdentityResultStatus.Unauthorized, "The refresh token has already been used.");
        }

        return IdentityServiceResult<TokenResponse>.Success(ToTokenResponse(replacement));
    }

    public async Task<IdentityServiceResult<bool>> LogoutAsync(
        LogoutRequest request,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return IdentityServiceResult<bool>.Failure(IdentityResultStatus.Validation, "A refresh token is required.");
        }

        var hash = RefreshTokenHash.Compute(request.RefreshToken);
        var token = await dbContext.RefreshTokens.SingleOrDefaultAsync(candidate => candidate.TokenHash == hash, cancellationToken);
        if (token is not null && token.IsActive(timeProvider.GetUtcNow()))
        {
            token.Revoke(timeProvider.GetUtcNow(), metadata.IpAddress, "User logout.");
            AddAudit(token.UserId, SecurityEventType.TokenRevoked, true, token.UserId.ToString(), metadata, "Refresh token revoked during logout.");
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return IdentityServiceResult<bool>.Success(true);
    }

    public async Task<IdentityServiceResult<bool>> RevokeAllAsync(
        ClaimsPrincipal principal,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return IdentityServiceResult<bool>.Failure(IdentityResultStatus.Unauthorized, "The authenticated user is invalid.");
        }

        await RevokeAllTokensAsync(userId, timeProvider.GetUtcNow(), metadata, "User requested revocation.", cancellationToken);
        AddAudit(userId, SecurityEventType.TokenRevoked, true, userId.ToString(), metadata, "All active refresh tokens revoked.");
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityServiceResult<bool>.Success(true);
    }

    public async Task<IdentityServiceResult<CurrentUserResponse>> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return IdentityServiceResult<CurrentUserResponse>.Failure(IdentityResultStatus.Unauthorized, "The authenticated user is invalid.");
        }

        var user = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return IdentityServiceResult<CurrentUserResponse>.Failure(IdentityResultStatus.Unauthorized, "The user is unavailable.");
        }

        var snapshot = await tokenService.GetAccessSnapshotAsync(userId, cancellationToken);
        return IdentityServiceResult<CurrentUserResponse>.Success(ToCurrentUser(snapshot));
    }

    public async Task<IdentityServiceResult<bool>> ChangePasswordAsync(
        ClaimsPrincipal principal,
        ChangePasswordRequest request,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return IdentityServiceResult<bool>.Failure(IdentityResultStatus.Unauthorized, "The authenticated user is invalid.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
        {
            return IdentityServiceResult<bool>.Failure(IdentityResultStatus.NotFound, "The user was not found.");
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return IdentityServiceResult<bool>.Failure(IdentityResultStatus.Validation, IdentityErrors(result));
        }

        await RevokeAllTokensAsync(user.Id, timeProvider.GetUtcNow(), metadata, "Password changed.", cancellationToken);
        AddAudit(user.Id, SecurityEventType.PasswordChanged, true, user.Id.ToString(), metadata, "Password changed by the user.");
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityServiceResult<bool>.Success(true);
    }

    public async Task<IdentityServiceResult<UserSummaryResponse[]>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var users = await dbContext.Users.AsNoTracking().OrderBy(user => user.FullName).ToArrayAsync(cancellationToken);
        var responses = new List<UserSummaryResponse>(users.Length);
        foreach (var user in users)
        {
            responses.Add(await MapUserAsync(user, cancellationToken));
        }

        return IdentityServiceResult<UserSummaryResponse[]>.Success(responses.ToArray());
    }

    public async Task<IdentityServiceResult<UserSummaryResponse>> CreateUserAsync(
        CreateUserRequest request,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateUserInput(request.Email, request.FullName);
        if (validationErrors.Length > 0)
        {
            return IdentityServiceResult<UserSummaryResponse>.Failure(IdentityResultStatus.Validation, validationErrors);
        }

        var email = request.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return IdentityServiceResult<UserSummaryResponse>.Failure(IdentityResultStatus.Conflict, "A user with this email already exists.");
        }

        var rolesResult = await ResolveRolesAsync(request.Roles, cancellationToken);
        if (!rolesResult.Succeeded)
        {
            return IdentityServiceResult<UserSummaryResponse>.Failure(rolesResult.Status, rolesResult.Errors);
        }

        var now = timeProvider.GetUtcNow();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FullName = request.FullName.Trim(),
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = now,
            CreatedBy = metadata.Actor,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var status = createResult.Errors.Any(error => error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase))
                ? IdentityResultStatus.Conflict
                : IdentityResultStatus.Validation;
            return IdentityServiceResult<UserSummaryResponse>.Failure(status, IdentityErrors(createResult));
        }

        AddRoleAssignments(user.Id, rolesResult.Value!, metadata.Actor, now);
        AddAudit(user.Id, SecurityEventType.UserCreated, true, metadata.Actor, metadata, $"User {user.Id} created.");
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityServiceResult<UserSummaryResponse>.Success(await MapUserAsync(user, cancellationToken));
    }

    public async Task<IdentityServiceResult<UserSummaryResponse>> UpdateUserAsync(
        Guid userId,
        UpdateUserRequest request,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateUserInput(request.Email, request.FullName);
        if (validationErrors.Length > 0)
        {
            return IdentityServiceResult<UserSummaryResponse>.Failure(IdentityResultStatus.Validation, validationErrors);
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return IdentityServiceResult<UserSummaryResponse>.Failure(IdentityResultStatus.NotFound, "The user was not found.");
        }

        var email = request.Email.Trim();
        var duplicate = await userManager.FindByEmailAsync(email);
        if (duplicate is not null && duplicate.Id != userId)
        {
            return IdentityServiceResult<UserSummaryResponse>.Failure(IdentityResultStatus.Conflict, "A user with this email already exists.");
        }

        if (!TryApplyVersion(user, request.Version))
        {
            return IdentityServiceResult<UserSummaryResponse>.Failure(IdentityResultStatus.Validation, "The user version is invalid.");
        }

        var wasActive = user.IsActive;
        user.Email = email;
        user.NormalizedEmail = userManager.NormalizeEmail(email);
        user.UserName = email;
        user.NormalizedUserName = userManager.NormalizeName(email);
        user.FullName = request.FullName.Trim();
        user.IsActive = request.IsActive;
        user.ModifiedAt = timeProvider.GetUtcNow();
        user.ModifiedBy = metadata.Actor;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return IdentityServiceResult<UserSummaryResponse>.Failure(IdentityResultStatus.ConcurrencyConflict, IdentityErrors(result));
        }

        if (wasActive && !user.IsActive)
        {
            await RevokeAllTokensAsync(user.Id, timeProvider.GetUtcNow(), metadata, "User disabled.", cancellationToken);
        }

        AddAudit(user.Id, SecurityEventType.UserUpdated, true, metadata.Actor, metadata, $"User {user.Id} updated; active={user.IsActive}.");
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityServiceResult<UserSummaryResponse>.Success(await MapUserAsync(user, cancellationToken));
    }

    public async Task<IdentityServiceResult<bool>> SetUserRolesAsync(
        Guid userId,
        SetUserRolesRequest request,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        var userExists = await dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken);
        if (!userExists)
        {
            return IdentityServiceResult<bool>.Failure(IdentityResultStatus.NotFound, "The user was not found.");
        }

        var rolesResult = await ResolveRolesAsync(request.RoleNames, cancellationToken);
        if (!rolesResult.Succeeded)
        {
            return IdentityServiceResult<bool>.Failure(rolesResult.Status, rolesResult.Errors);
        }

        var now = timeProvider.GetUtcNow();
        var desiredRoles = rolesResult.Value!;
        var desiredIds = desiredRoles.Select(role => role.Id).ToHashSet();
        var activeAssignments = await dbContext.UserRoleAssignments
            .Where(assignment => assignment.UserId == userId && assignment.RevokedAt == null)
            .ToArrayAsync(cancellationToken);
        foreach (var assignment in activeAssignments.Where(assignment => !desiredIds.Contains(assignment.RoleId)))
        {
            assignment.RevokedAt = now;
            assignment.RevokedBy = metadata.Actor;
        }

        var currentIds = activeAssignments.Select(assignment => assignment.RoleId).ToHashSet();
        AddRoleAssignments(userId, desiredRoles.Where(role => !currentIds.Contains(role.Id)), metadata.Actor, now);
        await RevokeAllTokensAsync(userId, now, metadata, "Role assignments changed.", cancellationToken);
        AddAudit(userId, SecurityEventType.UserRolesChanged, true, metadata.Actor, metadata, "User role assignments changed.");
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityServiceResult<bool>.Success(true);
    }

    public async Task<IdentityServiceResult<bool>> ResetPasswordAsync(
        Guid userId,
        ResetPasswordRequest request,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return IdentityServiceResult<bool>.Failure(IdentityResultStatus.NotFound, "The user was not found.");
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
        if (!result.Succeeded)
        {
            return IdentityServiceResult<bool>.Failure(IdentityResultStatus.Validation, IdentityErrors(result));
        }

        await RevokeAllTokensAsync(userId, timeProvider.GetUtcNow(), metadata, "Password reset.", cancellationToken);
        AddAudit(userId, SecurityEventType.PasswordReset, true, metadata.Actor, metadata, "Password reset by an authorized administrator.");
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityServiceResult<bool>.Success(true);
    }

    public async Task<IdentityServiceResult<RoleResponse[]>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await dbContext.Roles.AsNoTracking().OrderBy(role => role.Name).ToArrayAsync(cancellationToken);
        var responses = new List<RoleResponse>(roles.Length);
        foreach (var role in roles)
        {
            responses.Add(await MapRoleAsync(role, cancellationToken));
        }

        return IdentityServiceResult<RoleResponse[]>.Success(responses.ToArray());
    }

    public async Task<IdentityServiceResult<RoleResponse>> CreateRoleAsync(
        CreateRoleRequest request,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100)
        {
            return IdentityServiceResult<RoleResponse>.Failure(IdentityResultStatus.Validation, "A role name of 100 characters or fewer is required.");
        }

        if (await roleManager.FindByNameAsync(request.Name.Trim()) is not null)
        {
            return IdentityServiceResult<RoleResponse>.Failure(IdentityResultStatus.Conflict, "A role with this name already exists.");
        }

        var role = new ApplicationRole(request.Name.Trim())
        {
            Id = Guid.NewGuid(),
            Description = request.Description.Trim(),
            IsActive = true,
            CreatedAt = timeProvider.GetUtcNow(),
            CreatedBy = metadata.Actor,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            return IdentityServiceResult<RoleResponse>.Failure(IdentityResultStatus.Validation, IdentityErrors(result));
        }

        AddAudit(null, SecurityEventType.RoleCreated, true, metadata.Actor, metadata, $"Role {role.Id} created.");
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityServiceResult<RoleResponse>.Success(await MapRoleAsync(role, cancellationToken));
    }

    public async Task<IdentityServiceResult<RoleResponse>> UpdateRoleAsync(
        Guid roleId,
        UpdateRoleRequest request,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null)
        {
            return IdentityServiceResult<RoleResponse>.Failure(IdentityResultStatus.NotFound, "The role was not found.");
        }

        if (!TryApplyVersion(role, request.Version))
        {
            return IdentityServiceResult<RoleResponse>.Failure(IdentityResultStatus.Validation, "The role version is invalid.");
        }

        var duplicate = await roleManager.FindByNameAsync(request.Name.Trim());
        if (duplicate is not null && duplicate.Id != roleId)
        {
            return IdentityServiceResult<RoleResponse>.Failure(IdentityResultStatus.Conflict, "A role with this name already exists.");
        }

        var wasActive = role.IsActive;
        role.Name = request.Name.Trim();
        role.NormalizedName = roleManager.NormalizeKey(role.Name);
        role.Description = request.Description.Trim();
        role.IsActive = request.IsActive;
        role.ModifiedAt = timeProvider.GetUtcNow();
        role.ModifiedBy = metadata.Actor;
        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            return IdentityServiceResult<RoleResponse>.Failure(IdentityResultStatus.ConcurrencyConflict, IdentityErrors(result));
        }

        if (wasActive && !role.IsActive)
        {
            await RevokeTokensForRoleAsync(role.Id, timeProvider.GetUtcNow(), metadata, "Role disabled.", cancellationToken);
        }

        AddAudit(null, SecurityEventType.RoleUpdated, true, metadata.Actor, metadata, $"Role {role.Id} updated; active={role.IsActive}.");
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityServiceResult<RoleResponse>.Success(await MapRoleAsync(role, cancellationToken));
    }

    public async Task<IdentityServiceResult<PermissionResponse[]>> GetPermissionsAsync(CancellationToken cancellationToken)
    {
        var permissions = await dbContext.Permissions.AsNoTracking()
            .Where(permission => permission.IsActive)
            .OrderBy(permission => permission.Group)
            .ThenBy(permission => permission.Name)
            .Select(permission => new PermissionResponse(permission.Id, permission.Name, permission.Description, permission.Group))
            .ToArrayAsync(cancellationToken);
        return IdentityServiceResult<PermissionResponse[]>.Success(permissions);
    }

    public async Task<IdentityServiceResult<bool>> SetRolePermissionsAsync(
        Guid roleId,
        SetRolePermissionsRequest request,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        var roleExists = await dbContext.Roles.AnyAsync(role => role.Id == roleId, cancellationToken);
        if (!roleExists)
        {
            return IdentityServiceResult<bool>.Failure(IdentityResultStatus.NotFound, "The role was not found.");
        }

        var desiredNames = request.PermissionNames.Select(name => name.Trim()).Where(name => name.Length > 0).ToHashSet(StringComparer.Ordinal);
        var permissions = await dbContext.Permissions
            .Where(permission => permission.IsActive && desiredNames.Contains(permission.Name))
            .ToArrayAsync(cancellationToken);
        if (permissions.Length != desiredNames.Count)
        {
            return IdentityServiceResult<bool>.Failure(IdentityResultStatus.Validation, "One or more permission names are invalid or inactive.");
        }

        var now = timeProvider.GetUtcNow();
        var desiredIds = permissions.Select(permission => permission.Id).ToHashSet();
        var activeGrants = await dbContext.RolePermissions
            .Where(grant => grant.RoleId == roleId && grant.RevokedAt == null)
            .ToArrayAsync(cancellationToken);
        foreach (var grant in activeGrants.Where(grant => !desiredIds.Contains(grant.PermissionId)))
        {
            grant.RevokedAt = now;
            grant.RevokedBy = metadata.Actor;
        }

        var currentIds = activeGrants.Select(grant => grant.PermissionId).ToHashSet();
        foreach (var permission in permissions.Where(permission => !currentIds.Contains(permission.Id)))
        {
            dbContext.RolePermissions.Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                PermissionId = permission.Id,
                GrantedAt = now,
                GrantedBy = metadata.Actor
            });
        }

        await RevokeTokensForRoleAsync(roleId, now, metadata, "Role permissions changed.", cancellationToken);
        AddAudit(null, SecurityEventType.RolePermissionsChanged, true, metadata.Actor, metadata, $"Permission grants changed for role {roleId}.");
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityServiceResult<bool>.Success(true);
    }

    private static TokenResponse ToTokenResponse(IssuedToken issued) => new(
        issued.AccessToken,
        issued.AccessTokenExpiresAt,
        issued.RefreshToken,
        issued.RefreshTokenExpiresAt,
        ToCurrentUser(issued.User));

    private static CurrentUserResponse ToCurrentUser(UserAccessSnapshot snapshot) =>
        new(snapshot.Id, snapshot.Email, snapshot.FullName, snapshot.Roles, snapshot.Permissions);

    private async Task<UserSummaryResponse> MapUserAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await dbContext.UserRoleAssignments.AsNoTracking()
            .Where(assignment => assignment.UserId == user.Id && assignment.RevokedAt == null && assignment.Role.IsActive)
            .Select(assignment => assignment.Role.Name!)
            .OrderBy(name => name)
            .ToArrayAsync(cancellationToken);
        return new UserSummaryResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            user.IsActive,
            roles,
            user.CreatedAt,
            Convert.ToBase64String(user.RowVersion));
    }

    private async Task<RoleResponse> MapRoleAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        var permissions = await dbContext.RolePermissions.AsNoTracking()
            .Where(grant => grant.RoleId == role.Id && grant.RevokedAt == null && grant.Permission.IsActive)
            .Select(grant => grant.Permission.Name)
            .OrderBy(name => name)
            .ToArrayAsync(cancellationToken);
        return new RoleResponse(
            role.Id,
            role.Name ?? string.Empty,
            role.Description,
            role.IsActive,
            permissions,
            Convert.ToBase64String(role.RowVersion));
    }

    private async Task<IdentityServiceResult<ApplicationRole[]>> ResolveRolesAsync(
        IEnumerable<string> requestedRoles,
        CancellationToken cancellationToken)
    {
        var names = requestedRoles.Select(name => name.Trim()).Where(name => name.Length > 0).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (names.Count == 0)
        {
            return IdentityServiceResult<ApplicationRole[]>.Success([]);
        }

        var roles = await dbContext.Roles.Where(role => role.IsActive && role.Name != null && names.Contains(role.Name)).ToArrayAsync(cancellationToken);
        return roles.Length == names.Count
            ? IdentityServiceResult<ApplicationRole[]>.Success(roles)
            : IdentityServiceResult<ApplicationRole[]>.Failure(IdentityResultStatus.Validation, "One or more role names are invalid or inactive.");
    }

    private void AddRoleAssignments(Guid userId, IEnumerable<ApplicationRole> roles, string actor, DateTimeOffset now)
    {
        foreach (var role in roles)
        {
            dbContext.UserRoleAssignments.Add(new UserRoleAssignment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = role.Id,
                AssignedAt = now,
                AssignedBy = actor
            });
        }
    }

    private async Task RevokeAllTokensAsync(
        Guid userId,
        DateTimeOffset now,
        RequestMetadata metadata,
        string reason,
        CancellationToken cancellationToken)
    {
        var tokens = await dbContext.RefreshTokens.Where(token => token.UserId == userId && token.RevokedAt == null && token.ExpiresAt > now).ToArrayAsync(cancellationToken);
        foreach (var token in tokens)
        {
            token.Revoke(now, metadata.IpAddress, reason);
        }
    }

    private async Task RevokeTokenFamilyAsync(
        Guid userId,
        Guid familyId,
        DateTimeOffset now,
        RequestMetadata metadata,
        string reason,
        CancellationToken cancellationToken)
    {
        var tokens = await dbContext.RefreshTokens.Where(token => token.UserId == userId && token.FamilyId == familyId && token.RevokedAt == null).ToArrayAsync(cancellationToken);
        foreach (var token in tokens)
        {
            token.Revoke(now, metadata.IpAddress, reason);
        }
    }

    private async Task RevokeTokensForRoleAsync(
        Guid roleId,
        DateTimeOffset now,
        RequestMetadata metadata,
        string reason,
        CancellationToken cancellationToken)
    {
        var userIds = await dbContext.UserRoleAssignments
            .Where(assignment => assignment.RoleId == roleId && assignment.RevokedAt == null)
            .Select(assignment => assignment.UserId)
            .Distinct()
            .ToArrayAsync(cancellationToken);
        var tokens = await dbContext.RefreshTokens
            .Where(token => userIds.Contains(token.UserId) && token.RevokedAt == null && token.ExpiresAt > now)
            .ToArrayAsync(cancellationToken);
        foreach (var token in tokens)
        {
            token.Revoke(now, metadata.IpAddress, reason);
        }
    }

    private void AddAudit(
        Guid? userId,
        SecurityEventType eventType,
        bool succeeded,
        string actor,
        RequestMetadata metadata,
        string details)
    {
        dbContext.SecurityAuditEvents.Add(new SecurityAuditEvent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventType = eventType,
            Succeeded = succeeded,
            OccurredAt = timeProvider.GetUtcNow(),
            Actor = actor,
            IpAddress = metadata.IpAddress,
            UserAgent = metadata.UserAgent,
            Details = details
        });
    }

    private bool TryApplyVersion<TEntity>(TEntity entity, string version)
        where TEntity : class
    {
        if (string.IsNullOrEmpty(version))
        {
            return dbContext.Entry(entity).Property<byte[]>("RowVersion").CurrentValue.Length == 0;
        }

        try
        {
            dbContext.Entry(entity).Property<byte[]>("RowVersion").OriginalValue = Convert.FromBase64String(version);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    private static string[] ValidateUserInput(string email, string fullName)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(email) || email.Length > 256 || !IsValidEmail(email))
        {
            errors.Add("A valid email address of 256 characters or fewer is required.");
        }

        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length > 200)
        {
            errors.Add("A full name of 200 characters or fewer is required.");
        }

        return errors.ToArray();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            return new MailAddress(email).Address.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string[] IdentityErrors(IdentityResult result) =>
        result.Errors.Select(error => error.Description).Distinct(StringComparer.Ordinal).ToArray();
}
