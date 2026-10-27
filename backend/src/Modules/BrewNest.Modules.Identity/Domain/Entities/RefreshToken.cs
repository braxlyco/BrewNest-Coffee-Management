namespace BrewNest.Modules.Identity.Domain.Entities;

internal sealed class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid FamilyId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? RevocationReason { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public byte[] RowVersion { get; set; } = [];

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now, string? ipAddress, string reason, Guid? replacementId = null)
    {
        if (RevokedAt is not null)
        {
            return;
        }

        RevokedAt = now;
        RevokedByIp = ipAddress;
        RevocationReason = reason;
        ReplacedByTokenId = replacementId;
    }
}
