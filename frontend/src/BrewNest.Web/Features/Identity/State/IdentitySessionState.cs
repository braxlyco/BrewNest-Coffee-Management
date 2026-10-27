using System.Security.Claims;
using BrewNest.Contracts.Identity;
using BrewNest.Web.Features.Identity.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace BrewNest.Web.Features.Identity.State;

internal sealed class IdentitySessionState(
    IdentityTokenStore tokenStore,
    IdentityApiClient apiClient) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());
    private CurrentUserResponse? currentUser;
    private bool initialized;

    public CurrentUserResponse? CurrentUser => currentUser;
    public bool IsAuthenticated => currentUser is not null;

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(CreatePrincipal(currentUser)));

    public async Task InitializeAsync()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        var stored = await tokenStore.LoadAsync();
        if (stored is null || stored.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow)
        {
            await ClearAsync();
            return;
        }

        try
        {
            if (stored.AccessTokenExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(1))
            {
                var refreshed = await apiClient.RefreshAsync(new RefreshTokenRequest(stored.RefreshToken));
                await ApplyAsync(refreshed);
            }
            else
            {
                currentUser = stored.User;
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
        }
        catch (IdentityApiException)
        {
            await ClearAsync();
        }
    }

    public async Task LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var response = await apiClient.LoginAsync(new LoginRequest(email, password), cancellationToken);
        await ApplyAsync(response);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var stored = await tokenStore.LoadAsync();
        try
        {
            if (stored is not null)
            {
                await apiClient.LogoutAsync(new LogoutRequest(stored.RefreshToken), cancellationToken);
            }
        }
        finally
        {
            await ClearAsync();
        }
    }

    public async Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        await apiClient.ChangePasswordAsync(new ChangePasswordRequest(currentPassword, newPassword), cancellationToken);
        await ClearAsync();
    }

    private async Task ApplyAsync(TokenResponse response)
    {
        await tokenStore.SaveAsync(response);
        currentUser = response.User;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private async Task ClearAsync()
    {
        await tokenStore.ClearAsync();
        currentUser = null;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    private static ClaimsPrincipal CreatePrincipal(CurrentUserResponse? user)
    {
        if (user is null)
        {
            return Anonymous;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email)
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.Permissions.Select(permission => new Claim("permission", permission)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "BrewNestJwt"));
    }
}
