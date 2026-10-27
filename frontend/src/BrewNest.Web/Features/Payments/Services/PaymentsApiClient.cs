using System.Net.Http.Json;
using System.Text.Json;
using BrewNest.Contracts.Payments;
using BrewNest.Web.Services;

namespace BrewNest.Web.Features.Payments.Services;

internal sealed class PaymentsApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    public Task<PaymentResponse[]> Payments() => Get<PaymentResponse[]>("api/payments/");
    public Task<RefundResponse[]> Refunds() => Get<RefundResponse[]>("api/payments/refunds");
    public Task<PaymentResponse> Create(CreatePaymentRequest request) => Post<PaymentResponse>("api/payments/", request);
    public Task<PaymentResponse> Verify(Guid id, VerifyPaymentRequest request) => Post<PaymentResponse>($"api/payments/{id}/verification", request);
    public Task<PaymentResponse> Settle(Guid id, SettlePaymentRequest request) => Post<PaymentResponse>($"api/payments/{id}/settlement", request);
    public Task<RefundResponse> Process(Guid id, ProcessRefundRequest request) => Post<RefundResponse>($"api/payments/refunds/{id}/processing", request);

    private async Task<T> Get<T>(string url) { using var response = await http.GetAsync(url); return await Read<T>(response); }
    private async Task<T> Post<T>(string url, object value) { using var response = await http.PostAsJsonAsync(url, value, Options); return await Read<T>(response); }
    private static async Task<T> Read<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var text = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(ApiProblemMessage.Read(text, response.StatusCode, "Payments"));
        }
        return await response.Content.ReadFromJsonAsync<T>(Options) ?? throw new InvalidOperationException("The Payments API returned no data.");
    }
}
