using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.BuildingBlocks.Modules;

/// <summary>
/// Defines the composition boundary through which a BrewNest business module registers its services and endpoints.
/// </summary>
public interface IBrewNestModule
{
    /// <summary>Gets the stable display name of the module.</summary>
    string Name { get; }

    /// <summary>Registers services owned by the module.</summary>
    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>Maps HTTP endpoints owned by the module.</summary>
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
