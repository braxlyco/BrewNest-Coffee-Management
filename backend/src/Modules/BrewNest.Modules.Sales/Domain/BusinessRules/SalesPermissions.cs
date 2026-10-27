using BrewNest.BuildingBlocks.Authorization;

namespace BrewNest.Modules.Sales.Domain.BusinessRules;

internal static class SalesPermissions
{
    public const string OrdersRead="sales.orders.read"; public const string OrdersManage="sales.orders.manage"; public const string DiscountsRequest="sales.discounts.request"; public const string DiscountsApprove="sales.discounts.approve"; public const string BillingManage="sales.billing.manage"; public const string OrdersClose="sales.orders.close";
}
internal sealed class SalesPermissionDefinitionProvider:IPermissionDefinitionProvider
{
    public IReadOnlyCollection<PermissionDefinition> Permissions{get;}=[new(SalesPermissions.OrdersRead,"View sales orders, bills, and status history.","Sales"),new(SalesPermissions.OrdersManage,"Create and amend dine-in, takeaway, and delivery orders.","Sales"),new(SalesPermissions.DiscountsRequest,"Request a discount for manager approval.","Sales"),new(SalesPermissions.DiscountsApprove,"Approve or reject requested discounts.","Sales"),new(SalesPermissions.BillingManage,"Confirm orders and issue final bills.","Sales"),new(SalesPermissions.OrdersClose,"Close or cancel sales orders.","Sales")];
}
