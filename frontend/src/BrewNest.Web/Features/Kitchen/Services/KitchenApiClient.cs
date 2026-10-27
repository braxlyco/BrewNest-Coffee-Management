using System.Net.Http.Json;
using System.Text.Json;
using BrewNest.Contracts.Kitchen;
using BrewNest.Web.Services;

namespace BrewNest.Web.Features.Kitchen.Services;

internal sealed class KitchenApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<KitchenConfirmedOrderReference[]> GetConfirmedOrdersAsync() => GetAsync<KitchenConfirmedOrderReference[]>("api/kitchen/references/confirmed-orders");
    public Task<KitchenTicketResponse[]> GetQueueAsync(string? status = null) => GetAsync<KitchenTicketResponse[]>(string.IsNullOrWhiteSpace(status) ? "api/kitchen/tickets" : $"api/kitchen/tickets?status={Uri.EscapeDataString(status)}");
    public Task<KitchenTicketResponse> GetAsync(Guid id) => GetAsync<KitchenTicketResponse>($"api/kitchen/tickets/{id}");
    public Task<KitchenStatusHistoryResponse[]> GetHistoryAsync(Guid? ticketId = null) => GetAsync<KitchenStatusHistoryResponse[]>(ticketId is null ? "api/kitchen/status-history" : $"api/kitchen/status-history?ticketId={ticketId}");
    public Task<KitchenTicketResponse> GenerateAsync(Guid salesOrderId) => PostAsync<KitchenTicketResponse>("api/kitchen/tickets", new GenerateKitchenTicketRequest(salesOrderId));
    public Task<KitchenTicketResponse> ChangeStatusAsync(Guid id, ChangeKitchenTicketStatusRequest request) => PostAsync<KitchenTicketResponse>($"api/kitchen/tickets/{id}/status", request);
    public Task<KitchenTicketResponse> ChangeLineStatusAsync(Guid ticketId, Guid lineId, ChangeKitchenLineStatusRequest request) => PostAsync<KitchenTicketResponse>($"api/kitchen/tickets/{ticketId}/lines/{lineId}/status", request);
    public Task<KitchenTicketResponse> CancelAsync(Guid id, string reason, string version) => PostAsync<KitchenTicketResponse>($"api/kitchen/tickets/{id}/cancel", new ChangeKitchenTicketStatusRequest(KitchenTicketStatus.Cancelled, reason, version));

    private async Task<T> GetAsync<T>(string uri)
    {
        try { using var response = await httpClient.GetAsync(uri); return await ReadAsync<T>(response); }
        catch (HttpRequestException exception) { throw Unavailable(exception); }
    }

    private async Task<T> PostAsync<T>(string uri, object value)
    {
        try { using var response = await httpClient.PostAsJsonAsync(uri, value, JsonOptions); return await ReadAsync<T>(response); }
        catch (HttpRequestException exception) { throw Unavailable(exception); }
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode) throw await ErrorAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions) ?? throw new KitchenApiException("The Kitchen API returned an empty response.");
    }

    private static async Task<KitchenApiException> ErrorAsync(HttpResponseMessage response)
    {
        return new(ApiProblemMessage.Read(await response.Content.ReadAsStringAsync(), response.StatusCode, "Kitchen"));
    }

    private static KitchenApiException Unavailable(HttpRequestException exception) => new("The BrewNest API is unavailable. Start the backend and try again.", exception);
}

internal sealed class KitchenApiException : Exception
{
    public KitchenApiException(string message) : base(message) { }
    public KitchenApiException(string message, Exception innerException) : base(message, innerException) { }
}
