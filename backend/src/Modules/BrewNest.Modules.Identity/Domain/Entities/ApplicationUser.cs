using Microsoft.AspNetCore.Identity;

namespace BrewNest.Modules.Identity.Domain.Entities;

/// <summary>Represents a BrewNest application user managed by ASP.NET Core Identity.</summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Gets or sets the user's display name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the user may authenticate.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the UTC creation time.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the actor that created the user.</summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC last-modified time.</summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>Gets or sets the actor that last modified the user.</summary>
    public string? ModifiedBy { get; set; }

    /// <summary>Gets or sets the SQL Server optimistic-concurrency row version.</summary>
    public byte[] RowVersion { get; set; } = [];
}
