using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.BuildingBlocks.Modules;

/// <summary>Discovers module composition roots from the assemblies supplied by the API host.</summary>
public static class ModuleRegistrationExtensions
{
    /// <summary>Discovers module composition roots and invokes their service registration.</summary>
    /// <param name="services">The host service collection.</param>
    /// <param name="configuration">The host configuration.</param>
    /// <param name="assemblies">Assemblies containing module composition roots.</param>
    /// <returns>The same service collection for fluent composition.</returns>
    public static IServiceCollection AddBrewNestModules(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var modules = assemblies
            .Distinct()
            .SelectMany(assembly => assembly.DefinedTypes)
            .Where(type => !type.IsAbstract && !type.IsInterface && typeof(IBrewNestModule).IsAssignableFrom(type))
            .Select(type => (IBrewNestModule?)Activator.CreateInstance(type.AsType()))
            .Where(module => module is not null)
            .Cast<IBrewNestModule>()
            .OrderBy(module => module.Name, StringComparer.Ordinal)
            .ToArray();

        foreach (var module in modules)
        {
            module.RegisterServices(services, configuration);
            services.AddSingleton(module);
        }

        return services;
    }

    /// <summary>Maps endpoints for all module composition roots registered with the host.</summary>
    /// <param name="app">The configured web application.</param>
    /// <returns>The same application for fluent composition.</returns>
    public static WebApplication MapBrewNestModules(this WebApplication app)
    {
        foreach (var module in app.Services.GetServices<IBrewNestModule>())
        {
            module.MapEndpoints(app);
        }

        return app;
    }
}
