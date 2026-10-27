using BrewNest.Modules.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Identity.Infrastructure.Persistence;

/// <summary>Owns all ASP.NET Core Identity and BrewNest authorization persistence in the identity schema.</summary>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    internal DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    internal DbSet<Permission> Permissions => Set<Permission>();
    internal DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    internal DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();
    internal DbSet<SecurityAuditEvent> SecurityAuditEvents => Set<SecurityAuditEvent>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("identity");

        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        builder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
