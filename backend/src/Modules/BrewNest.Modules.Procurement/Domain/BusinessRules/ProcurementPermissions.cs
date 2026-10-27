using BrewNest.BuildingBlocks.Authorization;
namespace BrewNest.Modules.Procurement.Domain.BusinessRules;
/// <summary>Permission names owned by Procurement.</summary>
public static class ProcurementPermissions
{
    /// <summary>Read procurement data.</summary>
    public const string Read="procurement.read";
    /// <summary>Manage requisitions.</summary>
    public const string RequisitionsManage="procurement.requisitions.manage";
    /// <summary>Approve requisitions.</summary>
    public const string RequisitionsApprove="procurement.requisitions.approve";
    /// <summary>Manage sourcing.</summary>
    public const string SourcingManage="procurement.sourcing.manage";
    /// <summary>Manage orders.</summary>
    public const string OrdersManage="procurement.orders.manage";
    /// <summary>Approve orders.</summary>
    public const string OrdersApprove="procurement.orders.approve";
    /// <summary>Perform inspections.</summary>
    public const string InspectionsManage="procurement.inspections.manage";
    /// <summary>Manage receipts.</summary>
    public const string ReceiptsManage="procurement.receipts.manage";
    /// <summary>Manage vendor returns.</summary>
    public const string ReturnsManage="procurement.returns.manage";
}
internal sealed class ProcurementPermissionDefinitionProvider:IPermissionDefinitionProvider { public IReadOnlyCollection<PermissionDefinition> Permissions {get;}=[new(ProcurementPermissions.Read,"View procurement","Procurement"),new(ProcurementPermissions.RequisitionsManage,"Manage requisitions","Procurement"),new(ProcurementPermissions.RequisitionsApprove,"Approve requisitions","Procurement"),new(ProcurementPermissions.SourcingManage,"Manage sourcing","Procurement"),new(ProcurementPermissions.OrdersManage,"Manage purchase orders","Procurement"),new(ProcurementPermissions.OrdersApprove,"Approve purchase orders","Procurement"),new(ProcurementPermissions.InspectionsManage,"Perform inspections","Procurement"),new(ProcurementPermissions.ReceiptsManage,"Accept goods receipts","Procurement"),new(ProcurementPermissions.ReturnsManage,"Manage vendor returns","Procurement")]; }
