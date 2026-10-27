using BrewNest.BuildingBlocks.Authorization;
using BrewNest.Modules.Identity.Application.DTOs;
using BrewNest.Modules.Identity.Domain.Entities;
using BrewNest.Modules.Identity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BrewNest.Modules.Identity.Infrastructure.Persistence.Seed;

internal sealed partial class IdentityDevelopmentSeeder(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    IOptions<IdentitySeedOptions> options,
    ILogger<IdentityDevelopmentSeeder> logger,
    TimeProvider timeProvider,
    IEnumerable<IPermissionDefinitionProvider> permissionProviders) : IHostedService
{
    [LoggerMessage(EventId = 100, Level = LogLevel.Warning, Message = "Identity development seeding is enabled but administrator credentials are missing from secure configuration.")]
    private static partial void LogMissingSeedConfiguration(ILogger logger);

    [LoggerMessage(EventId = 101, Level = LogLevel.Error, Message = "Identity development administrator creation failed: {Errors}")]
    private static partial void LogSeedFailure(ILogger logger, string errors);

    [LoggerMessage(EventId = 102, Level = LogLevel.Information, Message = "Identity development data is ready for administrator {Email}.")]
    private static partial void LogSeedCompleted(ILogger logger, string email);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var seed = options.Value;
        if (!environment.IsDevelopment() || !seed.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(seed.AdministratorEmail) || string.IsNullOrWhiteSpace(seed.AdministratorPassword))
        {
            LogMissingSeedConfiguration(logger);
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        var now = timeProvider.GetUtcNow();
        var permissions = await SeedPermissionsAsync(dbContext, now, permissionProviders, cancellationToken);
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var role = await roleManager.FindByNameAsync("Owner/Admin");
        if (role is null)
        {
            role = new ApplicationRole("Owner/Admin")
            {
                Id = Guid.NewGuid(),
                Description = "Development administrator with all Identity permissions.",
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "development-seed",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            var roleResult = await roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
            {
                LogSeedFailure(logger, string.Join("; ", roleResult.Errors.Select(error => error.Description)));
                return;
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var administrator = await userManager.FindByEmailAsync(seed.AdministratorEmail.Trim());
        if (administrator is null)
        {
            administrator = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = seed.AdministratorEmail.Trim(),
                UserName = seed.AdministratorEmail.Trim(),
                FullName = seed.AdministratorFullName.Trim(),
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = now,
                CreatedBy = "development-seed",
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var userResult = await userManager.CreateAsync(administrator, seed.AdministratorPassword);
            if (!userResult.Succeeded)
            {
                LogSeedFailure(logger, string.Join("; ", userResult.Errors.Select(error => error.Description)));
                return;
            }
        }

        if (!await dbContext.UserRoleAssignments.AnyAsync(
                assignment => assignment.UserId == administrator.Id && assignment.RoleId == role.Id && assignment.RevokedAt == null,
                cancellationToken))
        {
            dbContext.UserRoleAssignments.Add(new UserRoleAssignment
            {
                Id = Guid.NewGuid(),
                UserId = administrator.Id,
                RoleId = role.Id,
                AssignedAt = now,
                AssignedBy = "development-seed"
            });
        }

        var activePermissionIds = await dbContext.RolePermissions
            .Where(grant => grant.RoleId == role.Id && grant.RevokedAt == null)
            .Select(grant => grant.PermissionId)
            .ToArrayAsync(cancellationToken);
        foreach (var permission in permissions.Where(permission => !activePermissionIds.Contains(permission.Id)))
        {
            dbContext.RolePermissions.Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = role.Id,
                PermissionId = permission.Id,
                GrantedAt = now,
                GrantedBy = "development-seed"
            });
        }

        dbContext.SecurityAuditEvents.Add(new SecurityAuditEvent
        {
            Id = Guid.NewGuid(),
            UserId = administrator.Id,
            EventType = SecurityEventType.UserUpdated,
            Succeeded = true,
            OccurredAt = now,
            Actor = "development-seed",
            Details = "Development Identity seed verified."
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        var administratorEmail = seed.AdministratorEmail.Trim();
        LogSeedCompleted(logger, administratorEmail);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task<Permission[]> SeedPermissionsAsync(
        IdentityDbContext dbContext,
        DateTimeOffset now,
        IEnumerable<IPermissionDefinitionProvider> permissionProviders,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.Permissions.ToDictionaryAsync(permission => permission.Name, StringComparer.Ordinal, cancellationToken);
        var definitions = permissionProviders
            .SelectMany(provider => provider.Permissions)
            .GroupBy(definition => definition.Name, StringComparer.Ordinal)
            .Select(group => group.Single())
            .OrderBy(definition => definition.Name, StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            if (existing.TryGetValue(definition.Name, out var permission))
            {
                permission.Description = definition.Description;
                permission.Group = definition.Group;
                permission.IsActive = true;
                permission.ModifiedAt = now;
                permission.ModifiedBy = "development-seed";
                continue;
            }

            permission = new Permission
            {
                Id = Guid.NewGuid(),
                Name = definition.Name,
                Description = definition.Description,
                Group = definition.Group,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "development-seed"
            };
            dbContext.Permissions.Add(permission);
            existing.Add(permission.Name, permission);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return existing.Values.ToArray();
    }
}
