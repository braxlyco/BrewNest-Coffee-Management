using Microsoft.Extensions.Configuration;
namespace BrewNest.Modules.Production.Application.Interfaces;
internal interface IProductionDocumentNumberPort { string NextPlanNumber(); string NextOrderNumber(); }
internal sealed class ConfiguredProductionDocumentNumberProvider(IConfiguration configuration,TimeProvider time):IProductionDocumentNumberPort
{
    public string NextPlanNumber()=>Next(configuration["Production:DocumentNumbers:PlanPrefix"],"PP"); public string NextOrderNumber()=>Next(configuration["Production:DocumentNumbers:OrderPrefix"],"PO");
    private string Next(string? configured,string fallback){var prefix=string.IsNullOrWhiteSpace(configured)?fallback:configured.Trim().ToUpperInvariant();if(prefix.Length>10)prefix=prefix[..10];return $"{prefix}-{time.GetUtcNow():yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..Math.Min(40,prefix.Length+30)].ToUpperInvariant();}
}
