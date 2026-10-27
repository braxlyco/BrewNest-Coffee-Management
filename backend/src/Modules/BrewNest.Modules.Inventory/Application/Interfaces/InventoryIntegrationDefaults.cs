using BrewNest.Contracts.Inventory;
using Microsoft.Extensions.Configuration;
namespace BrewNest.Modules.Inventory.Application.Interfaces;
internal sealed class PermissiveCatalogReferenceValidator:IInventoryCatalogReferenceValidator
{
    public ValueTask<bool> AreValidAsync(Guid itemId,Guid warehouseId,CancellationToken cancellationToken)=>ValueTask.FromResult(itemId!=Guid.Empty&&warehouseId!=Guid.Empty);
}
internal sealed class ConfiguredExpiryPolicyProvider(IConfiguration configuration):IInventoryExpiryPolicyProvider
{
    public ValueTask<InventoryExpiryThresholds> GetAsync(CancellationToken cancellationToken)
    {
        var first=configuration.GetValue("Inventory:Expiry:FirstAlertDays",15);var escalated=configuration.GetValue("Inventory:Expiry:EscalatedAlertDays",7);var critical=configuration.GetValue("Inventory:Expiry:CriticalAlertDays",3);
        if(first<=escalated||escalated<=critical||critical<0)throw new InvalidOperationException("Inventory expiry thresholds must be ordered First > Escalated > Critical >= 0.");
        return ValueTask.FromResult(new InventoryExpiryThresholds(first,escalated,critical));
    }
}
