namespace BrewNest.Modules.Identity.Domain.Entities;

internal sealed class UserRoleAssignment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid RoleId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
    public DateTimeOffset AssignedAt { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
