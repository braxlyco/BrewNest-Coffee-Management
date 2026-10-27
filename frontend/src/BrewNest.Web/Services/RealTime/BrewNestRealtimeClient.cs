using Microsoft.AspNetCore.SignalR.Client;
using BrewNest.Web.Features.Identity.State;

namespace BrewNest.Web.Services.RealTime;

/// <summary>Owns the browser connection used for module-specific real-time notifications.</summary>
internal sealed class BrewNestRealtimeClient(HttpClient httpClient, IdentityTokenStore tokenStore) : IAsyncDisposable
{
    private HubConnection? connection;

    /// <summary>Gets the current SignalR connection state.</summary>
    public HubConnectionState State => connection?.State ?? HubConnectionState.Disconnected;

    /// <summary>Starts the notification connection if it is disconnected.</summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        connection ??= new HubConnectionBuilder()
            .WithUrl(new Uri(httpClient.BaseAddress!, "hubs/notifications"), options =>
                options.AccessTokenProvider = async () => await tokenStore.GetAccessTokenAsync())
            .WithAutomaticReconnect()
            .Build();

        if (connection.State == HubConnectionState.Disconnected)
        {
            await connection.StartAsync(cancellationToken);
        }
    }

    /// <summary>Registers a handler for a module-specific notification.</summary>
    public IDisposable On<T>(string methodName, Action<T> handler)
    {
        connection ??= new HubConnectionBuilder()
            .WithUrl(new Uri(httpClient.BaseAddress!, "hubs/notifications"), options =>
                options.AccessTokenProvider = async () => await tokenStore.GetAccessTokenAsync())
            .WithAutomaticReconnect()
            .Build();
        return connection.On(methodName, handler);
    }

    /// <summary>Releases the underlying SignalR connection.</summary>
    public async ValueTask DisposeAsync()
    {
        if (connection is not null)
        {
            await connection.DisposeAsync();
        }
    }
}
