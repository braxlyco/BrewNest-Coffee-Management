using System.ComponentModel.DataAnnotations;

namespace BrewNest.Web.Features.Inventory.Models;

internal static class InventoryPermissionNames
{
    public const string StockRead="inventory.stock.read"; public const string StockManage="inventory.stock.manage";
    public const string ReservationsManage="inventory.reservations.manage"; public const string AdjustmentsManage="inventory.adjustments.manage";
    public const string WastageManage="inventory.wastage.manage"; public const string ExpiryRead="inventory.expiry.read";
}

internal sealed class ReceiptForm
{
    [Required] public string ItemId { get; set; }=string.Empty; [Required] public string WarehouseId { get; set; }=string.Empty;
    [Required,StringLength(100)] public string LotNumber { get; set; }=string.Empty; public DateOnly? ManufacturingDate { get; set; } public DateOnly? ExpiryDate { get; set; }
    [Range(typeof(decimal),"0.001","999999999999999.999")] public decimal Quantity { get; set; }=1;
    [Required] public string ReferenceType { get; set; }="ManualReceipt"; [Required] public string ReferenceNumber { get; set; }=string.Empty; public string Remarks { get; set; }=string.Empty;
}
internal sealed class ReservationForm { [Required] public string LotId{get;set;}=string.Empty; [Range(typeof(decimal),"0.001","999999999999999.999")] public decimal Quantity{get;set;}=1; [Required] public string ReferenceType{get;set;}="FutureWorkflow"; [Required] public string ReferenceId{get;set;}=string.Empty; public DateTimeOffset? ExpiresAt{get;set;} public string Remarks{get;set;}=string.Empty; }
internal sealed class ReleaseForm { [Required] public string ReservationId{get;set;}=string.Empty; [Range(typeof(decimal),"0.001","999999999999999.999")] public decimal Quantity{get;set;}=1; [Required] public string Reason{get;set;}=string.Empty; }
internal sealed class IssueForm { [Required] public string LotId{get;set;}=string.Empty; public string ReservationId{get;set;}=string.Empty; [Range(typeof(decimal),"0.001","999999999999999.999")] public decimal Quantity{get;set;}=1; [Required] public string ReferenceType{get;set;}="ManualIssue"; [Required] public string ReferenceNumber{get;set;}=string.Empty; public string Remarks{get;set;}=string.Empty; }
internal sealed class AdjustmentForm { [Required] public string LotId{get;set;}=string.Empty; [Required] public decimal QuantityDelta{get;set;} [Required] public string Reason{get;set;}=string.Empty; [Required] public string ReferenceNumber{get;set;}=string.Empty; }
internal sealed class WastageForm { [Required] public string LotId{get;set;}=string.Empty; [Range(typeof(decimal),"0.001","999999999999999.999")] public decimal Quantity{get;set;}=1; [Required] public string Reason{get;set;}=string.Empty; [Required] public string ReferenceNumber{get;set;}=string.Empty; }
