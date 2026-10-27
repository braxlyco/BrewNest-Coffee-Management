namespace BrewNest.Modules.Sales.Domain.Entities;

internal enum OrderType { DineIn, Takeaway, Delivery }
internal enum OrderStatus { Draft, Confirmed, Billed, Closed, Cancelled }
internal enum DiscountStatus { Pending, Approved, Rejected, Cancelled }

internal sealed class SalesOrder
{
    public Guid Id{get;set;} public string OrderNumber{get;set;}=string.Empty; public OrderType OrderType{get;set;} public OrderStatus Status{get;set;}
    public Guid? CustomerId{get;set;} public Guid? DiningTableId{get;set;} public string CustomerName{get;set;}=string.Empty; public string CustomerPhone{get;set;}=string.Empty; public string DeliveryAddress{get;set;}=string.Empty; public string Notes{get;set;}=string.Empty;
    public DateTimeOffset CreatedAt{get;set;} public string CreatedBy{get;set;}=string.Empty; public DateTimeOffset? ModifiedAt{get;set;} public string? ModifiedBy{get;set;} public byte[] RowVersion{get;set;}=[];
    public List<SalesOrderLine> Lines{get;set;}=[]; public List<SalesDiscount> Discounts{get;set;}=[]; public SalesBill? Bill{get;set;}
}
internal sealed class SalesOrderLine
{
    public Guid Id{get;set;} public Guid OrderId{get;set;} public SalesOrder Order{get;set;}=null!; public Guid MenuItemId{get;set;} public string ItemCode{get;set;}=string.Empty; public string ItemName{get;set;}=string.Empty;
    public decimal Quantity{get;set;} public decimal UnitPrice{get;set;} public string SpecialInstructions{get;set;}=string.Empty; public bool IsCancelled{get;set;} public string? CancellationReason{get;set;}
    public DateTimeOffset AddedAt{get;set;} public string AddedBy{get;set;}=string.Empty; public DateTimeOffset? ModifiedAt{get;set;} public string? ModifiedBy{get;set;} public byte[] RowVersion{get;set;}=[];
}
internal sealed class SalesDiscount
{
    public Guid Id{get;set;} public Guid OrderId{get;set;} public SalesOrder Order{get;set;}=null!; public decimal Percentage{get;set;} public decimal Amount{get;set;} public string Reason{get;set;}=string.Empty; public DiscountStatus Status{get;set;}
    public string RequestedBy{get;set;}=string.Empty; public DateTimeOffset RequestedAt{get;set;} public string? ApprovedBy{get;set;} public DateTimeOffset? ApprovedAt{get;set;} public string? DecisionRemarks{get;set;} public byte[] RowVersion{get;set;}=[];
}
internal sealed class SalesBill
{
    public Guid Id{get;set;} public Guid OrderId{get;set;} public SalesOrder Order{get;set;}=null!; public string BillNumber{get;set;}=string.Empty; public decimal GrossAmount{get;set;} public decimal DiscountAmount{get;set;} public decimal NetAmount{get;set;} public DateTimeOffset IssuedAt{get;set;} public string IssuedBy{get;set;}=string.Empty;
}
internal sealed class SalesStatusHistory
{
    public Guid Id{get;set;} public Guid OrderId{get;set;} public string PreviousStatus{get;set;}=string.Empty; public string NewStatus{get;set;}=string.Empty; public string Reason{get;set;}=string.Empty; public string ChangedBy{get;set;}=string.Empty; public DateTimeOffset ChangedAt{get;set;}
}
