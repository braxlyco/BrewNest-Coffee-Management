using System.Net.Http.Json;
using System.Text.Json;
using BrewNest.Web.Services;
using BrewNest.Contracts.Identity;

namespace BrewNest.Web.Features.Identity.Services;

internal sealed class IdentityApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<TokenResponse>("api/identity/auth/login", request, cancellationToken);

    public Task<TokenResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<TokenResponse>("api/identity/auth/refresh", request, cancellationToken);

    public Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Post, "api/identity/auth/logout", request, cancellationToken);

    public Task RevokeAllAsync(CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Post, "api/identity/auth/revoke-all", content: null, cancellationToken);

    public Task<CurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default) =>
        GetAsync<CurrentUserResponse>("api/identity/auth/me", cancellationToken);

    public Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Post, "api/identity/auth/change-password", request, cancellationToken);

    public Task<UserSummaryResponse[]> GetUsersAsync(CancellationToken cancellationToken = default) =>
        GetAsync<UserSummaryResponse[]>("api/identity/users", cancellationToken);

    public Task<UserSummaryResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<UserSummaryResponse>("api/identity/users", request, cancellationToken);

    public Task<UserSummaryResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default) =>
        PutAsync<UserSummaryResponse>($"api/identity/users/{userId}", request, cancellationToken);

    public Task SetUserRolesAsync(Guid userId, SetUserRolesRequest request, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Put, $"api/identity/users/{userId}/roles", request, cancellationToken);

    public Task ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Post, $"api/identity/users/{userId}/reset-password", request, cancellationToken);

    public Task<RoleResponse[]> GetRolesAsync(CancellationToken cancellationToken = default) =>
        GetAsync<RoleResponse[]>("api/identity/roles", cancellationToken);

    public Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<RoleResponse>("api/identity/roles", request, cancellationToken);

    public Task<RoleResponse> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default) =>
        PutAsync<RoleResponse>($"api/identity/roles/{roleId}", request, cancellationToken);

    public Task<PermissionResponse[]> GetPermissionsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<PermissionResponse[]>("api/identity/permissions", cancellationToken);

    public Task SetRolePermissionsAsync(Guid roleId, SetRolePermissionsRequest request, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Put, $"api/identity/roles/{roleId}/permissions", request, cancellationToken);

    private async Task<T> GetAsync<T>(string uri, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(uri, cancellationToken);
            return await ReadAsync<T>(response, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw ApiUnavailable(exception);
        }
    }

    private async Task<T> PostAsync<T>(string uri, object content, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(uri, content, JsonOptions, cancellationToken);
            return await ReadAsync<T>(response, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw ApiUnavailable(exception);
        }
    }

    private async Task<T> PutAsync<T>(string uri, object content, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PutAsJsonAsync(uri, content, JsonOptions, cancellationToken);
            return await ReadAsync<T>(response, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw ApiUnavailable(exception);
        }
    }

    private async Task SendWithoutResponseAsync(HttpMethod method, string uri, object? content, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(method, uri)
            {
                Content = content is null ? null : JsonContent.Create(content, options: JsonOptions)
            };
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw await CreateExceptionAsync(response, cancellationToken);
            }
        }
        catch (HttpRequestException exception)
        {
            throw ApiUnavailable(exception);
        }
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new IdentityApiException("The Identity API returned an empty response.");
    }

    private static async Task<IdentityApiException> CreateExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return new IdentityApiException(ApiProblemMessage.Read(await response.Content.ReadAsStringAsync(cancellationToken), response.StatusCode, "Identity"));
    }

    private static IdentityApiException ApiUnavailable(HttpRequestException exception) =>
        new("The BrewNest API is unavailable. Start the backend on https://localhost:7241 and try again.", exception);
}

internal sealed class IdentityApiException : Exception
{
    public IdentityApiException(string message) : base(message) { }
    public IdentityApiException(string message, Exception innerException) : base(message, innerException) { }
}
