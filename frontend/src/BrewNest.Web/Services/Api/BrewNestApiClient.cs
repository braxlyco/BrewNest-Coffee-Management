using System.Net.Http.Json;
using BrewNest.Contracts;

namespace BrewNest.Web.Services.Api;

internal sealed class BrewNestApiClient(HttpClient httpClient) : IBrewNestApiClient
{
    public Task<HealthStatusResponse?> GetHealthAsync(CancellationToken cancellationToken = default) =>
        httpClient.GetFromJsonAsync<HealthStatusResponse>("health", cancellationToken);
}
