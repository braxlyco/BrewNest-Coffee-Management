using BrewNest.Modules.Identity.Domain.BusinessRules;
using BrewNest.Modules.Identity.Domain.Entities;
using BrewNest.Modules.Identity.Infrastructure.Integrations;

namespace BrewNest.UnitTests;

/// <summary>Verifies security primitives without infrastructure dependencies.</summary>
public sealed class IdentitySecurityTests
{
    /// <summary>Confirms refresh-token hashes are deterministic without retaining plaintext.</summary>
    [Fact]
    public void RefreshTokenHashIsDeterministicAndDoesNotExposePlaintext()
    {
        const string token = "an-opaque-refresh-token";

        var first = RefreshTokenHash.Compute(token);
        var second = RefreshTokenHash.Compute(token);

        Assert.Equal(first, second);
        Assert.NotEqual(token, first);
        Assert.Equal(64, first.Length);
    }

    /// <summary>Confirms a revoked refresh token cannot become active again.</summary>
    [Fact]
    public void RevokedRefreshTokenRemainsInactive()
    {
        var now = DateTimeOffset.UtcNow;
        var token = new RefreshToken { ExpiresAt = now.AddDays(1) };

        Assert.True(token.IsActive(now));
        token.Revoke(now, "127.0.0.1", "Test revocation.");
        token.Revoke(now.AddMinutes(1), "127.0.0.2", "Second revocation.");

        Assert.False(token.IsActive(now.AddSeconds(1)));
        Assert.Equal("Test revocation.", token.RevocationReason);
        Assert.Equal("127.0.0.1", token.RevokedByIp);
    }

    /// <summary>Confirms policy names are explicit and unique.</summary>
    [Fact]
    public void PermissionCatalogContainsUniqueExplicitPolicies()
    {
        var names = IdentityPermissions.Definitions.Keys.ToArray();

        Assert.NotEmpty(names);
        Assert.Equal(names.Length, names.Distinct(StringComparer.Ordinal).Count());
        Assert.All(names, name => Assert.StartsWith("identity.", name, StringComparison.Ordinal));
    }
}
