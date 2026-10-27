using BrewNest.BuildingBlocks.Authorization;

namespace BrewNest.Modules.Identity.Domain.BusinessRules;

internal sealed class IdentityPermissionDefinitionProvider : IPermissionDefinitionProvider
{
    public IReadOnlyCollection<PermissionDefinition> Permissions { get; } =
        IdentityPermissions.Definitions
            .Select(definition => new PermissionDefinition(
                definition.Key,
                definition.Value.Description,
                definition.Value.Group))
            .ToArray();
}
