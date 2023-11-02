using Microsoft.AspNetCore.SignalR.Client;

namespace TripleTriad;

public abstract class GameClientBase : IGameClientHandler, IGameHubHandler, IAsyncDisposable
{
    protected HubConnection Connection { get; }

    public GameClientBase(HubConnection connection)
    {
        Connection = connection;
        Connection.On<OnlineUser>(nameof(OnUserConnected), OnUserConnected);
        Connection.On<OnlineUser>(nameof(OnUserDisconnected), OnUserDisconnected);
        Connection.On<Lobby>(nameof(OnLobbyCreated), OnLobbyCreated);
        Connection.On<Lobby>(nameof(OnLobbyRemoved), OnLobbyRemoved);
    }

    public Task StartAsync() => Connection.StartAsync();
    public Task StopAsync() => Connection.StopAsync();

    public abstract Task OnUserConnected(OnlineUser user);
    public abstract Task OnUserDisconnected(OnlineUser user);
    public abstract Task OnLobbyCreated(Lobby lobby);
    public abstract Task OnLobbyRemoved(Lobby lobby);

    public virtual Task<IReadOnlyCollection<OnlineUser>> GetUsersAsync() => Connection.InvokeAsync<IReadOnlyCollection<OnlineUser>>(nameof(GetUsersAsync));
    public virtual Task<IReadOnlyCollection<Lobby>> GetLobbiesAsync() => Connection.InvokeAsync<IReadOnlyCollection<Lobby>>(nameof(GetLobbiesAsync));
    public virtual Task CreateLobbyAsync(string name) => Connection.InvokeAsync<string>(nameof(CreateLobbyAsync), name);
    public virtual Task DeleteLobbyAsync(string lobbyId) => Connection.InvokeAsync(nameof(DeleteLobbyAsync), lobbyId);

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore() => await Connection.DisposeAsync().ConfigureAwait(false);
}
