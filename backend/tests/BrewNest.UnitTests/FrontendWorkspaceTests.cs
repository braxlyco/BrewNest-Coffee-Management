using System.Net;
using BrewNest.Web.Services;
using BrewNest.Web.Shared;

namespace BrewNest.UnitTests;

/// <summary>Regression checks for workspace navigation and safe API error display.</summary>
public sealed class FrontendWorkspaceTests
{
    /// <summary>Implemented cards must open real pages, not the old module placeholders.</summary>
    [Fact]
    public async Task EveryAvailableWorkspaceHasAnImplementedRoute()
    {
        var files = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "FrontendPages"), "*.razor", SearchOption.AllDirectories);
        Assert.NotEmpty(files);
        var sources = await Task.WhenAll(files.Select(file => File.ReadAllTextAsync(file)));
        foreach (var workspace in WorkspaceDirectory.All.Where(workspace => workspace.Available))
        {
            Assert.Equal(workspace.Slug, workspace.Url);
            Assert.Contains(sources, source => source.Contains($"@page \"/{workspace.Url}\"", StringComparison.Ordinal));
        }
        Assert.Equal(13, WorkspaceDirectory.All.Select(workspace => workspace.Slug).Distinct(StringComparer.Ordinal).Count());
    }

    /// <summary>Future modules are visibly marked and route to an honest availability page.</summary>
    [Fact]
    public void FutureWorkspacesAreExplicitlyMarked()
    {
        var planned = WorkspaceDirectory.All.Where(workspace => !workspace.Available).ToArray();
        Assert.Equal(3, planned.Length);
        Assert.All(planned, workspace =>
        {
            Assert.Equal("Coming soon", workspace.Category);
            Assert.Equal($"module/{workspace.Slug}", workspace.Url);
        });
    }

    /// <summary>Unexpected response shapes must not crash the error UI or leak HTML.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("<html>Proxy unavailable</html>")]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{\"detail\":42}")]
    [InlineData("{\"detail\":null}")]
    [InlineData("{\"detail\":\" \"}")]
    public void InvalidProblemBodiesHaveSafeFallbacks(string body)
    {
        Assert.Equal("We couldn't complete the Payments action right now. Please try again or ask a manager for help.", ApiProblemMessage.Read(body, HttpStatusCode.InternalServerError, "Payments"));
    }

    /// <summary>Problem Details retains the backend's actionable business message.</summary>
    [Fact]
    public void ProblemDetailIsPreserved()
    {
        Assert.Equal("Stock is reserved.", ApiProblemMessage.Read("{\"detail\":\"Stock is reserved.\"}", HttpStatusCode.Conflict, "Inventory"));
    }

    /// <summary>Technical database IDs are not meaningful or useful to café staff.</summary>
    [Fact]
    public void TechnicalIdentifiersAreHiddenFromProblemDetails()
    {
        var body="{\"detail\":\"Insufficient available stock for ingredient d3000000-0000-0000-0000-000000000004.\"}";
        var message=ApiProblemMessage.Read(body,HttpStatusCode.Conflict,"Kitchen");
        Assert.Contains("Inventory",message,StringComparison.Ordinal);
        Assert.DoesNotContain("d3000000",message,StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Authentication and authorization failures should explain the next step.</summary>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "Your session has expired. Please sign in again.")]
    [InlineData(HttpStatusCode.Forbidden, "Your account does not have permission for this action.")]
    public void AccessFailuresHaveActionableMessages(HttpStatusCode statusCode, string expected)
    {
        Assert.Equal(expected, ApiProblemMessage.Read("{}", statusCode, "Payments"));
    }
}
