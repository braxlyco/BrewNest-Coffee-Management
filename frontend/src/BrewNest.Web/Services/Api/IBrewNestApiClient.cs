using BrewNest.Contracts;

namespace BrewNest.Web.Services.Api;

/// <summary>Defines typed access to cross-cutting BrewNest API operations.</summary>
public interface IBrewNestApiClient
{
    /// <summary>Gets the current API health status.</summary>
    Task<HealthStatusResponse?> GetHealthAsync(CancellationToken cancellationToken = default);
}
