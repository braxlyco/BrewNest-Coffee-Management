using Microsoft.Extensions.Configuration;

namespace BrewNest.Modules.Kitchen.Application.Interfaces;

internal interface IKitchenDocumentNumberPort
{
    string NextTicketNumber();
}

internal sealed class ConfiguredKitchenDocumentNumberProvider(IConfiguration configuration, TimeProvider timeProvider) : IKitchenDocumentNumberPort
{
    public string NextTicketNumber()
    {
        var prefix = configuration["Kitchen:DocumentNumbers:TicketPrefix"];
        var normalized = string.IsNullOrWhiteSpace(prefix) ? "KOT" : prefix.Trim().ToUpperInvariant();
        if (normalized.Length > 10) normalized = normalized[..10];
        return $"{normalized}-{timeProvider.GetUtcNow():yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
}
