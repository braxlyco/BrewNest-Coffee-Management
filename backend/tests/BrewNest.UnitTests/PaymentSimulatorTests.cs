using BrewNest.Modules.Payments.Infrastructure.Integrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace BrewNest.UnitTests;

/// <summary>Ensures the replaceable wallet adapters remain development-only simulators.</summary>
public sealed class PaymentSimulatorTests
{
    private static readonly IConfiguration Settings = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Payments:Simulation:DeclinePayerReference"] = "+999000000000"
        }).Build();

    /// <summary>The configured QA number exercises a wallet decline without a real gateway.</summary>
    [Fact]
    public async Task DevelopmentWalletsCanSimulateApprovalAndDecline()
    {
        var environment = new TestEnvironment(Environments.Development);
        var gateways = new SimulatorPaymentGateway[]
        {
            new EasypaisaSimulatorGateway(Settings, environment),
            new JazzCashSimulatorGateway(Settings, environment)
        };
        foreach (var gateway in gateways)
        {
            var approved = await gateway.AuthorizeAsync("QA-PAY", 100m, "+923001234567", default);
            var declined = await gateway.AuthorizeAsync("QA-PAY", 100m, "+999000000000", default);
            Assert.True(approved.Succeeded);
            Assert.False(declined.Succeeded);
            Assert.Null(declined.Reference);
        }
    }

    /// <summary>Simulator adapters never represent real payments outside development.</summary>
    [Fact]
    public async Task WalletSimulatorRefusesNonDevelopmentEnvironment()
    {
        var gateway = new EasypaisaSimulatorGateway(Settings, new TestEnvironment(Environments.Production));
        Assert.False((await gateway.AuthorizeAsync("QA-PAY", 100m, "+923001234567", default)).Succeeded);
        Assert.False((await gateway.RefundAsync("QA-REFUND", 100m, "QA-SOURCE", default)).Succeeded);
    }

    private sealed class TestEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "BrewNest.UnitTests";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
