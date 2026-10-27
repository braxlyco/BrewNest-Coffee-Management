using BrewNest.BuildingBlocks.Authorization;
namespace BrewNest.Modules.Inventory.Domain.BusinessRules;
internal static class InventoryPermissions
{
    public const string StockRead="inventory.stock.read"; public const string StockManage="inventory.stock.manage"; public const string ReservationsManage="inventory.reservations.manage"; public const string AdjustmentsManage="inventory.adjustments.manage"; public const string WastageManage="inventory.wastage.manage"; public const string ExpiryRead="inventory.expiry.read";
}
internal sealed class InventoryPermissionDefinitionProvider:IPermissionDefinitionProvider
{
    public IReadOnlyCollection<PermissionDefinition> Permissions{get;}=[new(InventoryPermissions.StockRead,"View lots, balances, and stock movements.","Inventory"),new(InventoryPermissions.StockManage,"Receive, issue, block, and unblock stock.","Inventory"),new(InventoryPermissions.ReservationsManage,"Create and release stock reservations.","Inventory"),new(InventoryPermissions.AdjustmentsManage,"Create stock adjustments.","Inventory"),new(InventoryPermissions.WastageManage,"Record stock wastage.","Inventory"),new(InventoryPermissions.ExpiryRead,"View expiry alerts.","Inventory")];
}
