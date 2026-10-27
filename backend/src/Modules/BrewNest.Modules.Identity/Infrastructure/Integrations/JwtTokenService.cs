using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BrewNest.Modules.Identity.Application.DTOs;
using BrewNest.Modules.Identity.Application.Interfaces;
using BrewNest.Modules.Identity.Domain.Entities;
using BrewNest.Modules.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BrewNest.Modules.Identity.Infrastructure.Integrations;

internal sealed class JwtTokenService(
    IdentityDbContext dbContext,
    IOptions<JwtOptions> options,
    TimeProvider timeProvider) : IJwtTokenService
{
    private readonly JwtOptions jwtOptions = options.Value;

    public async Task<IssuedToken> CreateAsync(
        ApplicationUser user,
        Guid familyId,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        if (Encoding.UTF8.GetByteCount(jwtOptions.SigningKey) < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 bytes and must be supplied through user-secrets or an environment variable.");
        }

        var now = timeProvider.GetUtcNow();
        var accessExpiresAt = now.AddMinutes(jwtOptions.AccessTokenMinutes);
        var refreshExpiresAt = now.AddDays(jwtOptions.RefreshTokenDays);
        var snapshot = await GetAccessSnapshotAsync(user.Id, cancellationToken);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("security_stamp", user.SecurityStamp ?? string.Empty)
        };
        claims.AddRange(snapshot.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(snapshot.Permissions.Select(permission => new Claim("permission", permission)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            now.UtcDateTime,
            accessExpiresAt.UtcDateTime,
            credentials);

        var rawRefreshToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FamilyId = familyId,
            TokenHash = RefreshTokenHash.Compute(rawRefreshToken),
            CreatedAt = now,
            CreatedByIp = ipAddress,
            ExpiresAt = refreshExpiresAt
        };

        return new IssuedToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            accessExpiresAt,
            rawRefreshToken,
            refreshExpiresAt,
            refreshToken,
            snapshot);
    }

    public async Task<UserAccessSnapshot> GetAccessSnapshotAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking().SingleAsync(candidate => candidate.Id == userId, cancellationToken);
        var roles = await dbContext.UserRoleAssignments
            .AsNoTracking()
            .Where(assignment => assignment.UserId == userId && assignment.RevokedAt == null && assignment.Role.IsActive)
            .Select(assignment => assignment.Role.Name!)
            .Distinct()
            .OrderBy(name => name)
            .ToArrayAsync(cancellationToken);
        var permissions = await dbContext.RolePermissions
            .AsNoTracking()
            .Where(grant => grant.RevokedAt == null
                && grant.Permission.IsActive
                && dbContext.UserRoleAssignments.Any(assignment => assignment.UserId == userId
                    && assignment.RoleId == grant.RoleId
                    && assignment.RevokedAt == null
                    && assignment.Role.IsActive))
            .Select(grant => grant.Permission.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToArrayAsync(cancellationToken);

        return new UserAccessSnapshot(user.Id, user.Email ?? string.Empty, user.FullName, roles, permissions);
    }
}

internal static class RefreshTokenHash
{
    public static string Compute(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
