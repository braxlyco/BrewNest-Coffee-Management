using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.BuildingBlocks.Modules;

/// <summary>Provides safe defaults for a module that has not yet added application services or endpoints.</summary>
public abstract class BrewNestModule(string name) : IBrewNestModule
{
    /// <inheritdoc />
    public string Name { get; } = name;

    /// <inheritdoc />
    public virtual void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    /// <inheritdoc />
    public virtual void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
    }
}
