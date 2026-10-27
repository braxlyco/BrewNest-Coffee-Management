using System.Net.Http.Json;using System.Text.Json;using BrewNest.Contracts.Returns;
using BrewNest.Web.Services;
namespace BrewNest.Web.Features.Returns.Services;
internal sealed class ReturnsApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    public Task<ReturnResponse[]> Returns() => Get<ReturnResponse[]>("api/returns/requests");
    public Task<VoidResponse[]> Voids() => Get<VoidResponse[]>("api/returns/voids");
    public Task<ReturnResponse> Create(CreateReturnRequest request) => Post<ReturnResponse>("api/returns/requests", request);
    public Task<ReturnResponse> Decide(Guid id, ReturnDecisionRequest request) => Post<ReturnResponse>($"api/returns/requests/{id}/decision", request);
    public Task<ReturnResponse> Inspect(Guid id, Guid line, InspectReturnLineRequest request) => Post<ReturnResponse>($"api/returns/requests/{id}/lines/{line}/inspection", request);
    public Task<VoidResponse> CreateVoid(CreateVoidRequest request) => Post<VoidResponse>("api/returns/voids", request);
    public Task<VoidResponse> DecideVoid(Guid id, ReturnDecisionRequest request) => Post<VoidResponse>($"api/returns/voids/{id}/decision", request);

    private async Task<T> Get<T>(string url) { using var response = await http.GetAsync(url); return await Read<T>(response); }
    private async Task<T> Post<T>(string url, object value) { using var response = await http.PostAsJsonAsync(url, value, Options); return await Read<T>(response); }
    private static async Task<T> Read<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ApiProblemMessage.Read(await response.Content.ReadAsStringAsync(), response.StatusCode, "Returns"));
        }
        return await response.Content.ReadFromJsonAsync<T>(Options) ?? throw new InvalidOperationException("The Returns API returned no data.");
    }
}
