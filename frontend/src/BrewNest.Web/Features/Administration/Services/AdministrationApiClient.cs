using System.Net.Http.Json;
using System.Text.Json;
using BrewNest.Web.Services;
using BrewNest.Contracts.Administration;

namespace BrewNest.Web.Features.Administration.Services;

internal sealed class AdministrationApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<CompanySettingsResponse> GetCompanyAsync(CancellationToken token = default) => GetAsync<CompanySettingsResponse>("api/administration/company", token);
    public Task<CompanySettingsResponse> UpdateCompanyAsync(UpdateCompanySettingsRequest request, CancellationToken token = default) => PutAsync<CompanySettingsResponse>("api/administration/company", request, token);
    public Task<BranchResponse[]> GetBranchesAsync(CancellationToken token = default) => GetAsync<BranchResponse[]>("api/administration/branches", token);
    public Task<BranchResponse> CreateBranchAsync(CreateBranchRequest request, CancellationToken token = default) => PostAsync<BranchResponse>("api/administration/branches", request, token);
    public Task<FiscalYearResponse[]> GetFiscalYearsAsync(CancellationToken token = default) => GetAsync<FiscalYearResponse[]>("api/administration/fiscal-years", token);
    public Task<FiscalYearResponse> CreateFiscalYearAsync(CreateFiscalYearRequest request, CancellationToken token = default) => PostAsync<FiscalYearResponse>("api/administration/fiscal-years", request, token);
    public Task<DocumentNumberSequenceResponse[]> GetDocumentNumbersAsync(CancellationToken token = default) => GetAsync<DocumentNumberSequenceResponse[]>("api/administration/document-numbers", token);
    public Task<DocumentNumberSequenceResponse> CreateDocumentNumberAsync(CreateDocumentNumberSequenceRequest request, CancellationToken token = default) => PostAsync<DocumentNumberSequenceResponse>("api/administration/document-numbers", request, token);
    public Task<ApprovalPolicyResponse[]> GetApprovalsAsync(CancellationToken token = default) => GetAsync<ApprovalPolicyResponse[]>("api/administration/approvals", token);
    public Task<ApprovalPolicyResponse> CreateApprovalAsync(CreateApprovalPolicyRequest request, CancellationToken token = default) => PostAsync<ApprovalPolicyResponse>("api/administration/approvals", request, token);
    public Task<ApprovalPolicyResponse> UpdateApprovalAsync(Guid id, UpdateApprovalPolicyRequest request, CancellationToken token = default) => PutAsync<ApprovalPolicyResponse>($"api/administration/approvals/{id}", request, token);
    public Task<AlertSettingResponse[]> GetAlertsAsync(CancellationToken token = default) => GetAsync<AlertSettingResponse[]>("api/administration/alerts", token);
    public Task<AlertSettingResponse> UpdateAlertAsync(Guid id, UpdateAlertSettingRequest request, CancellationToken token = default) => PutAsync<AlertSettingResponse>($"api/administration/alerts/{id}", request, token);
    public Task<ExpenseAllocationSettingResponse[]> GetExpenseAllocationsAsync(CancellationToken token = default) => GetAsync<ExpenseAllocationSettingResponse[]>("api/administration/expense-allocations", token);
    public Task<ExpenseAllocationSettingResponse> CreateExpenseAllocationAsync(CreateExpenseAllocationSettingRequest request, CancellationToken token = default) => PostAsync<ExpenseAllocationSettingResponse>("api/administration/expense-allocations", request, token);
    public Task<AdministrationAuditResponse[]> GetAuditAsync(int take = 100, CancellationToken token = default) => GetAsync<AdministrationAuditResponse[]>($"api/administration/audit?take={take}", token);

    private async Task<T> GetAsync<T>(string uri, CancellationToken token)
    {
        try
        {
            using var response = await httpClient.GetAsync(uri, token);
            return await ReadAsync<T>(response, token);
        }
        catch (HttpRequestException exception)
        {
            throw ApiUnavailable(exception);
        }
    }

    private async Task<T> PostAsync<T>(string uri, object content, CancellationToken token)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(uri, content, JsonOptions, token);
            return await ReadAsync<T>(response, token);
        }
        catch (HttpRequestException exception)
        {
            throw ApiUnavailable(exception);
        }
    }

    private async Task<T> PutAsync<T>(string uri, object content, CancellationToken token)
    {
        try
        {
            using var response = await httpClient.PutAsJsonAsync(uri, content, JsonOptions, token);
            return await ReadAsync<T>(response, token);
        }
        catch (HttpRequestException exception)
        {
            throw ApiUnavailable(exception);
        }
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken token)
    {
        if (!response.IsSuccessStatusCode) throw await CreateExceptionAsync(response, token);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, token) ?? throw new AdministrationApiException("The Administration API returned an empty response.");
    }

    private static async Task<AdministrationApiException> CreateExceptionAsync(HttpResponseMessage response, CancellationToken token)
    {
        return new AdministrationApiException(ApiProblemMessage.Read(await response.Content.ReadAsStringAsync(token), response.StatusCode, "Administration"));
    }

    private static AdministrationApiException ApiUnavailable(HttpRequestException exception) =>
        new("The BrewNest API is unavailable. Start the backend on https://localhost:7241 and try again.", exception);
}

internal sealed class AdministrationApiException : Exception
{
    public AdministrationApiException(string message) : base(message) { }
    public AdministrationApiException(string message, Exception innerException) : base(message, innerException) { }
}
