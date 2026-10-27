namespace BrewNest.BuildingBlocks.Authorization;

/// <summary>Describes one explicit permission contributed by a business module.</summary>
/// <param name="Name">Stable permission policy and claim name.</param>
/// <param name="Description">Administrator-facing purpose.</param>
/// <param name="Group">Administrator-facing grouping.</param>
public sealed record PermissionDefinition(string Name, string Description, string Group);

/// <summary>Allows a module to contribute permissions without accessing Identity persistence.</summary>
public interface IPermissionDefinitionProvider
{
    /// <summary>Gets the module's stable permission definitions.</summary>
    IReadOnlyCollection<PermissionDefinition> Permissions { get; }
}
