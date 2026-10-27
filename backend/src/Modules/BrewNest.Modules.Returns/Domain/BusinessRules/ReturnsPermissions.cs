using BrewNest.BuildingBlocks.Authorization;
namespace BrewNest.Modules.Returns.Domain.BusinessRules;
/// <summary>Permission names owned by Returns.</summary>
public static class ReturnsPermissions
{
 /// <summary>Read returns.</summary>
 public const string Read="returns.read";
 /// <summary>Create returns.</summary>
 public const string Manage="returns.manage";
 /// <summary>Approve returns.</summary>
 public const string Approve="returns.approve";
 /// <summary>Inspect items.</summary>
 public const string Inspect="returns.inspect";
 /// <summary>Manage voids.</summary>
 public const string Voids="returns.voids.manage";
 /// <summary>Approve voids.</summary>
 public const string VoidsApprove="returns.voids.approve";
 /// <summary>Request refunds.</summary>
 public const string Refunds="returns.refunds.manage";
}
internal sealed class ReturnsPermissionProvider:IPermissionDefinitionProvider{public IReadOnlyCollection<PermissionDefinition> Permissions{get;}=[new(ReturnsPermissions.Read,"View returns and voids.","Returns"),new(ReturnsPermissions.Manage,"Create return requests.","Returns"),new(ReturnsPermissions.Approve,"Approve return requests.","Returns"),new(ReturnsPermissions.Inspect,"Inspect and disposition returned items.","Returns"),new(ReturnsPermissions.Voids,"Create void requests.","Returns"),new(ReturnsPermissions.VoidsApprove,"Approve void requests.","Returns"),new(ReturnsPermissions.Refunds,"Create refund requests.","Returns")];}
