using System.Net.Http.Json;
using System.Text.Json;
using BrewNest.Contracts.Inventory;
using BrewNest.Web.Services;

namespace BrewNest.Web.Features.Inventory.Services;

internal sealed class InventoryApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions=new(JsonSerializerDefaults.Web);
    public Task<InventoryCatalogReferenceData> GetReferencesAsync()=>GetAsync<InventoryCatalogReferenceData>("api/inventory/references");
    public Task<StockLotResponse[]> GetLotsAsync()=>GetAsync<StockLotResponse[]>("api/inventory/lots");
    public Task<StockLotResponse> ReceiveAsync(GoodsReceiptRequest request)=>PostAsync<StockLotResponse>("api/inventory/receipts",request);
    public Task<ReservationResponse[]> GetReservationsAsync()=>GetAsync<ReservationResponse[]>("api/inventory/reservations");
    public Task<ReservationResponse> ReserveAsync(CreateReservationRequest request)=>PostAsync<ReservationResponse>("api/inventory/reservations",request);
    public Task<ReservationResponse> ReleaseAsync(Guid id,ReleaseReservationRequest request)=>PostAsync<ReservationResponse>($"api/inventory/reservations/{id}/release",request);
    public Task<StockLotResponse> IssueAsync(GoodsIssueRequest request)=>PostAsync<StockLotResponse>("api/inventory/issues",request);
    public Task<StockLotResponse> AdjustAsync(StockAdjustmentRequest request)=>PostAsync<StockLotResponse>("api/inventory/adjustments",request);
    public Task<StockLotResponse> WasteAsync(WastageRequest request)=>PostAsync<StockLotResponse>("api/inventory/wastage",request);
    public Task<StockLotResponse> SetBlockAsync(Guid id,SetLotBlockRequest request)=>PutAsync<StockLotResponse>($"api/inventory/lots/{id}/block",request);
    public Task<StockMovementResponse[]> GetMovementsAsync()=>GetAsync<StockMovementResponse[]>("api/inventory/movements");
    public Task<ExpiryAlertResponse[]> GetExpiryAlertsAsync()=>GetAsync<ExpiryAlertResponse[]>("api/inventory/expiry-alerts");
    private async Task<T> GetAsync<T>(string uri){try{using var response=await httpClient.GetAsync(uri);return await ReadAsync<T>(response);}catch(HttpRequestException ex){throw Unavailable(ex);}}
    private async Task<T> PostAsync<T>(string uri,object value){try{using var response=await httpClient.PostAsJsonAsync(uri,value,JsonOptions);return await ReadAsync<T>(response);}catch(HttpRequestException ex){throw Unavailable(ex);}}
    private async Task<T> PutAsync<T>(string uri,object value){try{using var response=await httpClient.PutAsJsonAsync(uri,value,JsonOptions);return await ReadAsync<T>(response);}catch(HttpRequestException ex){throw Unavailable(ex);}}
    private static async Task<T> ReadAsync<T>(HttpResponseMessage response){if(!response.IsSuccessStatusCode)throw await ErrorAsync(response);return await response.Content.ReadFromJsonAsync<T>(JsonOptions)??throw new InventoryApiException("The Inventory API returned an empty response.");}
    private static async Task<InventoryApiException> ErrorAsync(HttpResponseMessage response)=>new(ApiProblemMessage.Read(await response.Content.ReadAsStringAsync(),response.StatusCode,"Inventory"));
    private static InventoryApiException Unavailable(HttpRequestException ex)=>new("The BrewNest API is unavailable. Start the backend and try again.",ex);
}
internal sealed class InventoryApiException:Exception { public InventoryApiException(string message):base(message){} public InventoryApiException(string message,Exception inner):base(message,inner){} }
