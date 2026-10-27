using BrewNest.Contracts.Sales;
using Microsoft.Extensions.Configuration;

namespace BrewNest.Modules.Sales.Application.Interfaces;

internal sealed class ConfiguredSalesDocumentNumberProvider(IConfiguration configuration,TimeProvider timeProvider):ISalesDocumentNumberPort
{
    public string NextOrderNumber()=>Create(configuration["Sales:DocumentNumbers:OrderPrefix"]??"SO");
    public string NextBillNumber()=>Create(configuration["Sales:DocumentNumbers:BillPrefix"]??"INV");
    private string Create(string prefix)=>$"{Normalize(prefix)}-{timeProvider.GetUtcNow():yyyyMMdd}-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
    private static string Normalize(string prefix){var value=new string(prefix.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());return string.IsNullOrEmpty(value)?"DOC":value[..Math.Min(value.Length,6)];}
}
