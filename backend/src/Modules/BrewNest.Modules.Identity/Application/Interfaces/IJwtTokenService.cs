using BrewNest.Modules.Identity.Domain.Entities;

namespace BrewNest.Modules.Identity.Application.Interfaces;

internal interface IJwtTokenService
{
    Task<IssuedToken> CreateAsync(
        ApplicationUser user,
        Guid familyId,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task<UserAccessSnapshot> GetAccessSnapshotAsync(Guid userId, CancellationToken cancellationToken);
}

internal sealed record IssuedToken(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    RefreshToken RefreshTokenEntity,
    UserAccessSnapshot User);

internal sealed record UserAccessSnapshot(Guid Id, string Email, string FullName, string[] Roles, string[] Permissions);
