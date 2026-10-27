using System.ComponentModel.DataAnnotations;
using BrewNest.Contracts.Sales;

namespace BrewNest.Web.Features.Sales.Models;

internal static class SalesPermissionNames
{
    public const string OrdersRead="sales.orders.read";public const string OrdersManage="sales.orders.manage";public const string DiscountsRequest="sales.discounts.request";public const string DiscountsApprove="sales.discounts.approve";public const string BillingManage="sales.billing.manage";public const string OrdersClose="sales.orders.close";
}
internal sealed class CreateOrderForm
{
    public SalesOrderType OrderType{get;set;}=SalesOrderType.DineIn;public Guid? CustomerId{get;set;}public Guid? DiningTableId{get;set;}[StringLength(200)]public string CustomerName{get;set;}=string.Empty;[StringLength(16),RegularExpression(@"^\+[1-9][0-9]{7,14}$",ErrorMessage="Use international format, for example +923001234567 (8–15 digits after +). ")]public string CustomerPhone{get;set;}=string.Empty;[StringLength(500)]public string DeliveryAddress{get;set;}=string.Empty;[StringLength(1000)]public string Notes{get;set;}=string.Empty;
}
internal sealed class AddLineForm { [Required]public Guid? MenuItemId{get;set;}[Range(typeof(decimal),"0.001","999999999999999.999")]public decimal Quantity{get;set;}=1;[StringLength(500)]public string Instructions{get;set;}=string.Empty; }
internal sealed class DiscountForm { [Range(typeof(decimal),"0.01","100")]public decimal Percentage{get;set;}=5;[Required,StringLength(500)]public string Reason{get;set;}=string.Empty; }
internal sealed class StatusReasonForm { [StringLength(500)]public string Reason{get;set;}=string.Empty; }
