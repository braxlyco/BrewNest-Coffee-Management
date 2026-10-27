using Microsoft.AspNetCore.Identity;

namespace BrewNest.Modules.Identity.Domain.Entities;

/// <summary>Represents a named collection of BrewNest permissions.</summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>Initializes a role for Entity Framework.</summary>
    public ApplicationRole()
    {
    }

    /// <summary>Initializes a role with a name.</summary>
    /// <param name="name">The unique role name.</param>
    public ApplicationRole(string name)
        : base(name)
    {
    }

    /// <summary>Gets or sets the role description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the role can grant access.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the UTC creation time.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the actor that created the role.</summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC last-modified time.</summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>Gets or sets the actor that last modified the role.</summary>
    public string? ModifiedBy { get; set; }

    /// <summary>Gets or sets the SQL Server optimistic-concurrency row version.</summary>
    public byte[] RowVersion { get; set; } = [];
}
