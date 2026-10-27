using System.Net.Http.Json;
using System.Text.Json;
using BrewNest.Contracts.Procurement;
using BrewNest.Web.Services;
namespace BrewNest.Web.Features.Procurement.Services;
internal sealed class ProcurementApiClient(HttpClient http)
{
 private static readonly JsonSerializerOptions Options=new(JsonSerializerDefaults.Web);
 public Task<ProcurementReferenceData> References()=>Get<ProcurementReferenceData>("api/procurement/references");public Task<RequisitionResponse[]> Requisitions()=>Get<RequisitionResponse[]>("api/procurement/requisitions");public Task<RfqResponse[]> Rfqs()=>Get<RfqResponse[]>("api/procurement/rfqs");public Task<QuotationResponse[]> Quotations()=>Get<QuotationResponse[]>("api/procurement/quotations");public Task<PurchaseOrderResponse[]> Orders()=>Get<PurchaseOrderResponse[]>("api/procurement/orders");public Task<GoodsReceiptResponse[]> Receipts()=>Get<GoodsReceiptResponse[]>("api/procurement/receipts");public Task<RequisitionResponse> Create(CreateRequisitionRequest r)=>Post<RequisitionResponse>("api/procurement/requisitions",r);public Task<RequisitionResponse> Change(Guid id,string action,ChangeProcurementStatusRequest r)=>Post<RequisitionResponse>($"api/procurement/requisitions/{id}/{action}",r);public Task<PurchaseOrderResponse> Decide(Guid id,bool approve,ChangeProcurementStatusRequest r)=>Post<PurchaseOrderResponse>($"api/procurement/orders/{id}/{(approve?"approve":"reject")}",r);
 public Task<RfqResponse> CreateRfq(CreateRfqRequest request)=>Post<RfqResponse>("api/procurement/rfqs",request);
 public Task<QuotationResponse> SubmitQuotation(Guid rfqId,SubmitQuotationRequest request)=>Post<QuotationResponse>($"api/procurement/rfqs/{rfqId}/quotations",request);
 public Task<PurchaseOrderResponse> SelectQuotation(Guid quotationId,ChangeProcurementStatusRequest request)=>Post<PurchaseOrderResponse>($"api/procurement/quotations/{quotationId}/select",request);
 public Task<GoodsReceiptResponse> CreateReceipt(Guid orderId,CreateGoodsReceiptRequest request)=>Post<GoodsReceiptResponse>($"api/procurement/orders/{orderId}/receipts",request);
 public Task<GoodsReceiptResponse> Inspect(Guid receiptId,Guid lineId,CompleteInspectionRequest request)=>Post<GoodsReceiptResponse>($"api/procurement/receipts/{receiptId}/lines/{lineId}/inspection",request);
 public Task<VendorReturnResponse> Return(CreateVendorReturnRequest request)=>Post<VendorReturnResponse>("api/procurement/vendor-returns",request);
 private async Task<T> Get<T>(string uri){using var response=await http.GetAsync(uri);return await Read<T>(response);}private async Task<T> Post<T>(string uri,object value){using var response=await http.PostAsJsonAsync(uri,value,Options);return await Read<T>(response);}private static async Task<T> Read<T>(HttpResponseMessage r){if(!r.IsSuccessStatusCode){throw new InvalidOperationException(ApiProblemMessage.Read(await r.Content.ReadAsStringAsync(),r.StatusCode,"Procurement"));}return await r.Content.ReadFromJsonAsync<T>(Options)??throw new InvalidOperationException("The Procurement API returned no data.");}
}
