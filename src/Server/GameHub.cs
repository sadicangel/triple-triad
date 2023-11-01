using Microsoft.AspNetCore.SignalR;
using TripleTriad.ServerState;

namespace TripleTriad;

internal sealed class GameHub(ILogger<GameHub> logger, ServerManager manager) : Hub<IGameClientHandler>, IGameHubHandler
{
    public override async Task OnConnectedAsync()
    {
        var onlineUser = await manager.AddOnlineUser(Context.User.GetSubjectId());
        logger.LogInformation("User {user} has connected", onlineUser);

        await Clients.Others.OnUserConnected(onlineUser);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var onlineUser = await manager.RemoveOnlineUser(Context.User.GetSubjectId());
        logger.LogInformation("User {user} has disconnected", onlineUser);

        await Clients.Others.OnUserDisconnected(onlineUser);

        await base.OnDisconnectedAsync(exception);
    }

    public Task<ServerStateResponse> GetServerStateAsync() => Task.FromResult(manager.GetServerState());
}