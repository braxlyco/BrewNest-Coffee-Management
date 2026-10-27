using Microsoft.Extensions.Configuration;
namespace BrewNest.Modules.Procurement.Application.Interfaces;
internal interface IProcurementDocumentNumbers { string Requisition();string Rfq();string PurchaseOrder();string GoodsReceipt();string VendorReturn(); }
internal sealed class ProcurementDocumentNumbers(IConfiguration configuration): IProcurementDocumentNumbers
{
    private long value=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();private string Next(string prefix)=>$"{prefix}-{Interlocked.Increment(ref value)}";
    public string Requisition()=>Next(configuration["Procurement:DocumentNumbers:RequisitionPrefix"]??"PR");public string Rfq()=>Next(configuration["Procurement:DocumentNumbers:RfqPrefix"]??"RFQ");public string PurchaseOrder()=>Next(configuration["Procurement:DocumentNumbers:OrderPrefix"]??"PO");public string GoodsReceipt()=>Next(configuration["Procurement:DocumentNumbers:ReceiptPrefix"]??"GRN");public string VendorReturn()=>Next(configuration["Procurement:DocumentNumbers:ReturnPrefix"]??"VRN");
}
