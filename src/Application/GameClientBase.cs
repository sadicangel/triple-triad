using Microsoft.AspNetCore.SignalR.Client;
using TripleTriad.ServerState;

namespace TripleTriad;

public abstract class GameClientBase : IGameClientHandler, IGameHubHandler
{
    protected HubConnection Connection { get; }

    public GameClientBase(HubConnection connection)
    {
        Connection = connection;
        Connection.On<OnlineUser>(nameof(OnUserConnected), OnUserConnected);
        Connection.On<OnlineUser>(nameof(OnUserDisconnected), OnUserDisconnected);
    }

    public abstract Task OnUserConnected(OnlineUser user);
    public abstract Task OnUserDisconnected(OnlineUser user);

    public Task<ServerStateResponse> GetServerStateAsync() =>
        Connection.InvokeAsync<ServerStateResponse>(nameof(GetServerStateAsync));
}
