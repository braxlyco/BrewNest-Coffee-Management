using BrewNest.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewNest.Modules.Identity.Infrastructure.EntityConfigurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FullName).HasMaxLength(200).IsRequired();
        builder.Property(user => user.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(user => user.ModifiedBy).HasMaxLength(100);
        builder.Property(user => user.RowVersion).IsRowVersion();
    }
}

internal sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property(role => role.Description).HasMaxLength(500).IsRequired();
        builder.Property(role => role.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(role => role.ModifiedBy).HasMaxLength(100);
        builder.Property(role => role.RowVersion).IsRowVersion();
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(token => token.CreatedByIp).HasMaxLength(64);
        builder.Property(token => token.RevokedByIp).HasMaxLength(64);
        builder.Property(token => token.RevocationReason).HasMaxLength(200);
        builder.Property(token => token.RowVersion).IsRowVersion();
        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => new { token.UserId, token.FamilyId });
        builder.HasOne(token => token.User)
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(permission => permission.Id);
        builder.Property(permission => permission.Name).HasMaxLength(150).IsRequired();
        builder.Property(permission => permission.Description).HasMaxLength(500).IsRequired();
        builder.Property(permission => permission.Group).HasMaxLength(100).IsRequired();
        builder.Property(permission => permission.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(permission => permission.ModifiedBy).HasMaxLength(100);
        builder.Property(permission => permission.RowVersion).IsRowVersion();
        builder.HasIndex(permission => permission.Name).IsUnique();
    }
}

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(grant => grant.Id);
        builder.Property(grant => grant.GrantedBy).HasMaxLength(100).IsRequired();
        builder.Property(grant => grant.RevokedBy).HasMaxLength(100);
        builder.Property(grant => grant.RowVersion).IsRowVersion();
        builder.HasIndex(grant => new { grant.RoleId, grant.PermissionId })
            .HasFilter("[RevokedAt] IS NULL")
            .IsUnique();
        builder.HasOne(grant => grant.Role).WithMany().HasForeignKey(grant => grant.RoleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(grant => grant.Permission).WithMany().HasForeignKey(grant => grant.PermissionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class UserRoleAssignmentConfiguration : IEntityTypeConfiguration<UserRoleAssignment>
{
    public void Configure(EntityTypeBuilder<UserRoleAssignment> builder)
    {
        builder.ToTable("UserRoleAssignments");
        builder.HasKey(assignment => assignment.Id);
        builder.Property(assignment => assignment.AssignedBy).HasMaxLength(100).IsRequired();
        builder.Property(assignment => assignment.RevokedBy).HasMaxLength(100);
        builder.Property(assignment => assignment.RowVersion).IsRowVersion();
        builder.HasIndex(assignment => new { assignment.UserId, assignment.RoleId })
            .HasFilter("[RevokedAt] IS NULL")
            .IsUnique();
        builder.HasOne(assignment => assignment.User).WithMany().HasForeignKey(assignment => assignment.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(assignment => assignment.Role).WithMany().HasForeignKey(assignment => assignment.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SecurityAuditEventConfiguration : IEntityTypeConfiguration<SecurityAuditEvent>
{
    public void Configure(EntityTypeBuilder<SecurityAuditEvent> builder)
    {
        builder.ToTable("SecurityAuditEvents");
        builder.HasKey(audit => audit.Id);
        builder.Property(audit => audit.EventType).HasConversion<string>().HasMaxLength(80).IsRequired();
        builder.Property(audit => audit.Actor).HasMaxLength(200).IsRequired();
        builder.Property(audit => audit.IpAddress).HasMaxLength(64);
        builder.Property(audit => audit.UserAgent).HasMaxLength(500);
        builder.Property(audit => audit.Details).HasMaxLength(1000).IsRequired();
        builder.HasIndex(audit => new { audit.UserId, audit.OccurredAt });
    }
}
