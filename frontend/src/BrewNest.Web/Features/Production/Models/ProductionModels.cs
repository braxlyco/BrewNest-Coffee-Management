using System.ComponentModel.DataAnnotations;
namespace BrewNest.Web.Features.Production.Models;
internal static class ProductionPermissionNames { public const string Read="production.read";public const string PlansManage="production.plans.manage";public const string OrdersRelease="production.orders.release";public const string MaterialsIssue="production.materials.issue";public const string CompletionManage="production.completion.manage"; }
internal sealed class CreateProductionPlanForm { [Required,StringLength(200)]public string Name{get;set;}=string.Empty;public DateTime PlannedDate{get;set;}=DateTime.Today;[StringLength(1000)]public string Notes{get;set;}=string.Empty;[Required]public Guid? MenuItemId{get;set;}[Range(typeof(decimal),"0.001","999999999999999.999")]public decimal Quantity{get;set;}=1m; }
internal sealed class ProductionUsageForm { public Guid ItemId{get;set;}public decimal Consumption{get;set;}public decimal Wastage{get;set;} }
