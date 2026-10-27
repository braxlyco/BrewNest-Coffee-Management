using System.Net.Http.Json;
using System.Text.Json;
using BrewNest.Contracts.Catalog;
using BrewNest.Web.Services;

namespace BrewNest.Web.Features.Catalog.Services;

internal sealed class CatalogApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions=new(JsonSerializerDefaults.Web);
    public Task<UnitOfMeasureResponse[]> GetUnitsAsync()=>GetAsync<UnitOfMeasureResponse[]>("api/catalog/units");
    public Task<UnitOfMeasureResponse> CreateUnitAsync(CreateUnitOfMeasureRequest x)=>PostAsync<UnitOfMeasureResponse>("api/catalog/units",x);
    public Task<UnitOfMeasureResponse> UpdateUnitAsync(Guid id,UpdateUnitOfMeasureRequest x)=>PutAsync<UnitOfMeasureResponse>($"api/catalog/units/{id}",x);
    public Task<ItemGroupResponse[]> GetGroupsAsync()=>GetAsync<ItemGroupResponse[]>("api/catalog/item-groups");
    public Task<ItemGroupResponse> CreateGroupAsync(CreateItemGroupRequest x)=>PostAsync<ItemGroupResponse>("api/catalog/item-groups",x);
    public Task<ItemGroupResponse> UpdateGroupAsync(Guid id,UpdateItemGroupRequest x)=>PutAsync<ItemGroupResponse>($"api/catalog/item-groups/{id}",x);
    public Task<CatalogItemResponse[]> GetItemsAsync(string? type=null)=>GetAsync<CatalogItemResponse[]>($"api/catalog/items{(type is null?string.Empty:$"?type={Uri.EscapeDataString(type)}")}");
    public Task<CatalogItemResponse> CreateItemAsync(CreateCatalogItemRequest x)=>PostAsync<CatalogItemResponse>("api/catalog/items",x);
    public Task<CatalogItemResponse> UpdateItemAsync(Guid id,UpdateCatalogItemRequest x)=>PutAsync<CatalogItemResponse>($"api/catalog/items/{id}",x);
    public Task<CustomerResponse[]> GetCustomersAsync()=>GetAsync<CustomerResponse[]>("api/catalog/customers");
    public Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest x)=>PostAsync<CustomerResponse>("api/catalog/customers",x);
    public Task<CustomerResponse> UpdateCustomerAsync(Guid id,UpdateCustomerRequest x)=>PutAsync<CustomerResponse>($"api/catalog/customers/{id}",x);
    public Task<VendorResponse[]> GetVendorsAsync()=>GetAsync<VendorResponse[]>("api/catalog/vendors");
    public Task<VendorResponse> CreateVendorAsync(CreateVendorRequest x)=>PostAsync<VendorResponse>("api/catalog/vendors",x);
    public Task<VendorResponse> UpdateVendorAsync(Guid id,UpdateVendorRequest x)=>PutAsync<VendorResponse>($"api/catalog/vendors/{id}",x);
    public Task<DiningTableResponse[]> GetTablesAsync()=>GetAsync<DiningTableResponse[]>("api/catalog/dining-tables");
    public Task<DiningTableResponse> CreateTableAsync(CreateDiningTableRequest x)=>PostAsync<DiningTableResponse>("api/catalog/dining-tables",x);
    public Task<DiningTableResponse> UpdateTableAsync(Guid id,UpdateDiningTableRequest x)=>PutAsync<DiningTableResponse>($"api/catalog/dining-tables/{id}",x);
    public Task<WarehouseResponse[]> GetWarehousesAsync()=>GetAsync<WarehouseResponse[]>("api/catalog/warehouses");
    public Task<WarehouseResponse> CreateWarehouseAsync(CreateWarehouseRequest x)=>PostAsync<WarehouseResponse>("api/catalog/warehouses",x);
    public Task<WarehouseResponse> UpdateWarehouseAsync(Guid id,UpdateWarehouseRequest x)=>PutAsync<WarehouseResponse>($"api/catalog/warehouses/{id}",x);
    public Task<RiderResponse[]> GetRidersAsync()=>GetAsync<RiderResponse[]>("api/catalog/riders");
    public Task<RiderResponse> CreateRiderAsync(CreateRiderRequest x)=>PostAsync<RiderResponse>("api/catalog/riders",x);
    public Task<RiderResponse> UpdateRiderAsync(Guid id,UpdateRiderRequest x)=>PutAsync<RiderResponse>($"api/catalog/riders/{id}",x);
    public Task<RecipeResponse[]> GetRecipesAsync()=>GetAsync<RecipeResponse[]>("api/catalog/recipes");
    public Task<RecipeResponse> CreateRecipeAsync(CreateRecipeRequest x)=>PostAsync<RecipeResponse>("api/catalog/recipes",x);
    public Task<RecipeResponse> UpdateRecipeAsync(Guid id,UpdateRecipeRequest x)=>PutAsync<RecipeResponse>($"api/catalog/recipes/{id}",x);
    public Task<InspectionRequirementResponse[]> GetInspectionsAsync()=>GetAsync<InspectionRequirementResponse[]>("api/catalog/inspection-requirements");
    public Task<InspectionRequirementResponse> CreateInspectionAsync(CreateInspectionRequirementRequest x)=>PostAsync<InspectionRequirementResponse>("api/catalog/inspection-requirements",x);
    public Task<InspectionRequirementResponse> UpdateInspectionAsync(Guid id,UpdateInspectionRequirementRequest x)=>PutAsync<InspectionRequirementResponse>($"api/catalog/inspection-requirements/{id}",x);
    public Task<CatalogStatusHistoryResponse[]> GetHistoryAsync()=>GetAsync<CatalogStatusHistoryResponse[]>("api/catalog/status-history");
    private async Task<T> GetAsync<T>(string uri){try{using var response=await httpClient.GetAsync(uri);return await ReadAsync<T>(response);}catch(HttpRequestException ex){throw Unavailable(ex);}}
    private async Task<T> PostAsync<T>(string uri,object value){try{using var response=await httpClient.PostAsJsonAsync(uri,value,JsonOptions);return await ReadAsync<T>(response);}catch(HttpRequestException ex){throw Unavailable(ex);}}
    private async Task<T> PutAsync<T>(string uri,object value){try{using var response=await httpClient.PutAsJsonAsync(uri,value,JsonOptions);return await ReadAsync<T>(response);}catch(HttpRequestException ex){throw Unavailable(ex);}}
    private static async Task<T> ReadAsync<T>(HttpResponseMessage response){if(!response.IsSuccessStatusCode)throw await ErrorAsync(response);return await response.Content.ReadFromJsonAsync<T>(JsonOptions)??throw new CatalogApiException("The Catalog API returned an empty response.");}
    private static async Task<CatalogApiException> ErrorAsync(HttpResponseMessage response)=>new(ApiProblemMessage.Read(await response.Content.ReadAsStringAsync(),response.StatusCode,"Catalog"));
    private static CatalogApiException Unavailable(HttpRequestException ex)=>new("The BrewNest API is unavailable. Start the backend on https://localhost:7241 and try again.",ex);
}
internal sealed class CatalogApiException:Exception { public CatalogApiException(string message):base(message){} public CatalogApiException(string message,Exception inner):base(message,inner){} }
