using System.Text.Json;
using BrewNest.Contracts.Identity;
using Microsoft.JSInterop;

namespace BrewNest.Web.Features.Identity.State;

internal sealed class IdentityTokenStore(IJSRuntime jsRuntime)
{
    private const string StorageKey = "brewnest.identity.session";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ValueTask<string?> GetAccessTokenAsync() => GetAccessTokenCoreAsync();

    public async ValueTask<StoredIdentitySession?> LoadAsync()
    {
        var json = await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<StoredIdentitySession>(json, JsonOptions);
    }

    public async ValueTask SaveAsync(TokenResponse response)
    {
        var session = new StoredIdentitySession(
            response.AccessToken,
            response.AccessTokenExpiresAt,
            response.RefreshToken,
            response.RefreshTokenExpiresAt,
            response.User);
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, JsonSerializer.Serialize(session, JsonOptions));
    }

    public ValueTask ClearAsync() => jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);

    private async ValueTask<string?> GetAccessTokenCoreAsync()
    {
        var session = await LoadAsync();
        return session?.AccessToken;
    }
}

internal sealed record StoredIdentitySession(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    CurrentUserResponse User);
