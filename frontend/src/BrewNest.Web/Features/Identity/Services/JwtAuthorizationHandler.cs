using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrewNest.Contracts.Identity;
using BrewNest.Web.Features.Identity.State;

namespace BrewNest.Web.Features.Identity.Services;

internal sealed class JwtAuthorizationHandler(IdentityTokenStore tokenStore) : DelegatingHandler
{
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!IsAuthenticationOperation(request.RequestUri))
        {
            await RefreshIfNeededAsync(request.RequestUri, cancellationToken);
        }

        var accessToken = await tokenStore.GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task RefreshIfNeededAsync(Uri? requestUri, CancellationToken cancellationToken)
    {
        var stored = await tokenStore.LoadAsync();
        if (stored is null || stored.AccessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return;
        }

        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            stored = await tokenStore.LoadAsync();
            if (stored is null || stored.AccessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return;
            }

            if (stored.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow || requestUri is null || !requestUri.IsAbsoluteUri)
            {
                await tokenStore.ClearAsync();
                return;
            }

            var refreshUri = new Uri(requestUri, "/api/identity/auth/refresh");
            using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, refreshUri)
            {
                Content = JsonContent.Create(new RefreshTokenRequest(stored.RefreshToken))
            };
            using var response = await base.SendAsync(refreshRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                await tokenStore.ClearAsync();
                return;
            }

            var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
            if (tokens is null)
            {
                await tokenStore.ClearAsync();
                return;
            }

            await tokenStore.SaveAsync(tokens);
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private static bool IsAuthenticationOperation(Uri? requestUri) =>
        requestUri?.AbsolutePath.StartsWith("/api/identity/auth/", StringComparison.OrdinalIgnoreCase) == true;
}
