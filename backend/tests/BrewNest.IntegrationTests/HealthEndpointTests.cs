using System.Net;
using System.Net.Http.Json;
using BrewNest.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrewNest.IntegrationTests;

/// <summary>Exercises the API host's foundation health contract.</summary>
public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    /// <summary>Creates a test instance using the shared in-memory API host.</summary>
    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    /// <summary>Confirms that the composed application starts and reports healthy.</summary>
    [Fact]
    public async Task HealthEndpointReportsHealthyFoundation()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        using var response = await client.GetAsync("/health", timeout.Token);
        var health = await response.Content.ReadFromJsonAsync<HealthStatusResponse>(timeout.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(health);
        Assert.Equal("Healthy", health.Status);
    }
}
