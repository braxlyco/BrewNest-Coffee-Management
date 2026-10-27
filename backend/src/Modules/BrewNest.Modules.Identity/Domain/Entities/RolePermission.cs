namespace BrewNest.Modules.Identity.Domain.Entities;

internal sealed class RolePermission
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
    public DateTimeOffset GrantedAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
