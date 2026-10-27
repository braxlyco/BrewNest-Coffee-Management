using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrewNest.Contracts.Catalog;
using BrewNest.Contracts.Identity;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BrewNest.IntegrationTests;

/// <summary>Exercises Catalog authorization, masters, recipes, quality rules, history, and concurrency.</summary>
public sealed class CatalogEndpointTests:IClassFixture<IdentityWebApplicationFactory>,IAsyncLifetime
{
    private readonly IdentityWebApplicationFactory factory;
    /// <summary>Creates the Catalog HTTP test suite.</summary>
    public CatalogEndpointTests(IdentityWebApplicationFactory factory)=>this.factory=factory;
    /// <inheritdoc />
    public async Task InitializeAsync(){await using var scope=factory.Services.CreateAsyncScope();await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.EnsureCreatedAsync();}
    /// <inheritdoc />
    public Task DisposeAsync()=>Task.CompletedTask;

    /// <summary>Confirms Catalog APIs require both authentication and explicit permission.</summary>
    [Fact]
    public async Task CatalogRequiresExplicitPermission()
    {
        using var client=Client();using var anonymous=await client.GetAsync("/api/catalog/items");Assert.Equal(HttpStatusCode.Unauthorized,anonymous.StatusCode);
        var admin=await LoginAdmin(client);Bearer(client,admin.AccessToken);var email=$"catalog-denied-{Guid.NewGuid():N}@test.local";var password=IdentityWebApplicationFactory.CreateValidPassword();using var created=await client.PostAsJsonAsync("/api/identity/users",new CreateUserRequest(email,"Catalog Denied",password,[]));Assert.Equal(HttpStatusCode.OK,created.StatusCode);client.DefaultRequestHeaders.Authorization=null;using var login=await client.PostAsJsonAsync("/api/identity/auth/login",new LoginRequest(email,password));var token=Assert.IsType<TokenResponse>(await login.Content.ReadFromJsonAsync<TokenResponse>());Bearer(client,token.AccessToken);using var forbidden=await client.GetAsync("/api/catalog/items");Assert.Equal(HttpStatusCode.Forbidden,forbidden.StatusCode);
    }

    /// <summary>Confirms all non-transactional master families can be created and duplicate codes conflict.</summary>
    [Fact]
    public async Task CreatesMasterDataAndRejectsDuplicateCodes()
    {
        using var client=await AdminClient();var suffix=Guid.NewGuid().ToString("N")[..6];
        var unit=await Post<UnitOfMeasureResponse>(client,"/api/catalog/units",new CreateUnitOfMeasureRequest($"kg-{suffix}","Kilogram","kg","Mass",3));
        var group=await Post<ItemGroupResponse>(client,"/api/catalog/item-groups",new CreateItemGroupRequest($"raw-{suffix}","Raw materials","RawMaterial",true));
        var item=await Post<CatalogItemResponse>(client,"/api/catalog/items",new CreateCatalogItemRequest($"flour-{suffix}","Flour","Ingredient","RawMaterial",unit.Id,group.Id,null,200m,180,true));
        Assert.Equal("RawMaterial",item.ItemType);Assert.True(item.RequiresInspection);
        Assert.NotNull(await Post<CustomerResponse>(client,"/api/catalog/customers",new CreateCustomerRequest($"cus-{suffix}","Customer","+923001234567","customer@test.local","Address")));
        Assert.NotNull(await Post<VendorResponse>(client,"/api/catalog/vendors",new CreateVendorRequest($"ven-{suffix}","Vendor","Contact","+923011234567","vendor@test.local","Address","NTN")));
        Assert.NotNull(await Post<DiningTableResponse>(client,"/api/catalog/dining-tables",new CreateDiningTableRequest($"tbl-{suffix}","Table",4,"Main")));
        Assert.NotNull(await Post<WarehouseResponse>(client,"/api/catalog/warehouses",new CreateWarehouseRequest($"wh-{suffix}","Store","Address")));
        Assert.NotNull(await Post<RiderResponse>(client,"/api/catalog/riders",new CreateRiderRequest($"rid-{suffix}","Rider","+923021234567","ABC-1",true)));
        using var duplicate=await client.PostAsJsonAsync("/api/catalog/units",new CreateUnitOfMeasureRequest($"KG-{suffix}","Duplicate","kg","Mass",3));Assert.Equal(HttpStatusCode.Conflict,duplicate.StatusCode);
    }

    /// <summary>Confirms recipes accept materials, reject menu items as ingredients, and preserve decimal quantities.</summary>
    [Fact]
    public async Task RecipeEnforcesMaterialLines()
    {
        using var client=await AdminClient();var suffix=Guid.NewGuid().ToString("N")[..6];var unit=await Post<UnitOfMeasureResponse>(client,"/api/catalog/units",new CreateUnitOfMeasureRequest($"u-{suffix}","Unit","ea","Count",3));
        var menu=await Post<CatalogItemResponse>(client,"/api/catalog/items",new CreateCatalogItemRequest($"menu-{suffix}","Coffee","","MenuItem",unit.Id,null,500m,100m,null,false));
        var raw=await Post<CatalogItemResponse>(client,"/api/catalog/items",new CreateCatalogItemRequest($"raw-{suffix}","Beans","","RawMaterial",unit.Id,null,null,50m,90,false));
        var recipe=await Post<RecipeResponse>(client,"/api/catalog/recipes",new CreateRecipeRequest($"rec-{suffix}","Coffee BOM",menu.Id,1.000m,unit.Id,[new(raw.Id,0.125m,2.5m,1)]));Assert.Equal(0.125m,Assert.Single(recipe.Lines).Quantity);
        using var invalid=await client.PostAsJsonAsync("/api/catalog/recipes",new CreateRecipeRequest($"bad-{suffix}","Bad BOM",menu.Id,1m,unit.Id,[new(menu.Id,1m,0m,1)]));Assert.Equal(HttpStatusCode.BadRequest,invalid.StatusCode);
    }

    /// <summary>Confirms quality requirements are configurable per item and duplicate active targets are rejected.</summary>
    [Fact]
    public async Task QualityRequirementIsItemScopedAndUnique()
    {
        using var client=await AdminClient();var suffix=Guid.NewGuid().ToString("N")[..6];var unit=await Post<UnitOfMeasureResponse>(client,"/api/catalog/units",new CreateUnitOfMeasureRequest($"q-{suffix}","Quality unit","kg","Mass",3));var item=await Post<CatalogItemResponse>(client,"/api/catalog/items",new CreateCatalogItemRequest($"qi-{suffix}","Milk","","RawMaterial",unit.Id,null,null,250m,5,true));var request=new CreateInspectionRequirementRequest(item.Id,null,"Milk receiving",true,[new("Temperature","Numeric","1","5","C",true,1)]);var requirement=await Post<InspectionRequirementResponse>(client,"/api/catalog/inspection-requirements",request);Assert.Equal("Numeric",Assert.Single(requirement.Criteria).ResultType);using var duplicate=await client.PostAsJsonAsync("/api/catalog/inspection-requirements",request);Assert.Equal(HttpStatusCode.BadRequest,duplicate.StatusCode);
    }

    /// <summary>Confirms invalid concurrency tokens conflict and soft deactivation creates status history.</summary>
    [Fact]
    public async Task UpdateUsesConcurrencyAndRecordsSoftDeactivation()
    {
        using var client=await AdminClient();var suffix=Guid.NewGuid().ToString("N")[..6];var unit=await Post<UnitOfMeasureResponse>(client,"/api/catalog/units",new CreateUnitOfMeasureRequest($"h-{suffix}","History unit","ea","Count",0));using var stale=await client.PutAsJsonAsync($"/api/catalog/units/{unit.Id}",new UpdateUnitOfMeasureRequest(unit.Code,unit.Name,unit.Symbol,unit.Category,unit.DecimalPlaces,false,"invalid","retired"));Assert.Equal(HttpStatusCode.Conflict,stale.StatusCode);using var updated=await client.PutAsJsonAsync($"/api/catalog/units/{unit.Id}",new UpdateUnitOfMeasureRequest(unit.Code,unit.Name,unit.Symbol,unit.Category,unit.DecimalPlaces,false,unit.Version,"retired"));Assert.Equal(HttpStatusCode.OK,updated.StatusCode);var history=Assert.IsType<CatalogStatusHistoryResponse[]>(await client.GetFromJsonAsync<CatalogStatusHistoryResponse[]>($"/api/catalog/status-history?entityId={unit.Id}"));Assert.Contains(history,x=>x.NewStatus=="Inactive"&&x.Reason=="retired");
    }

    private HttpClient Client()=>factory.CreateClient(new(){BaseAddress=new Uri("https://localhost")});
    private async Task<HttpClient> AdminClient(){var client=Client();var tokens=await LoginAdmin(client);Bearer(client,tokens.AccessToken);return client;}
    private async Task<TokenResponse> LoginAdmin(HttpClient client){client.DefaultRequestHeaders.Authorization=null;using var response=await client.PostAsJsonAsync("/api/identity/auth/login",new LoginRequest(factory.AdministratorEmail,factory.AdministratorPassword));Assert.Equal(HttpStatusCode.OK,response.StatusCode);return Assert.IsType<TokenResponse>(await response.Content.ReadFromJsonAsync<TokenResponse>());}
    private static void Bearer(HttpClient client,string token)=>client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",token);
    private static async Task<T> Post<T>(HttpClient client,string uri,object request){using var response=await client.PostAsJsonAsync(uri,request);Assert.Equal(HttpStatusCode.OK,response.StatusCode);return Assert.IsType<T>(await response.Content.ReadFromJsonAsync<T>());}
}
