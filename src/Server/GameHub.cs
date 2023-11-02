using Microsoft.AspNetCore.SignalR;

namespace TripleTriad;

internal sealed class GameHub(ILogger<GameHub> logger, ServerManager manager) : Hub<IGameClientHandler>, IGameHubHandler
{
    public override async Task OnConnectedAsync()
    {
        var onlineUser = await manager.ConnectUser(Context.User.GetSubjectId());
        logger.LogInformation("User {user} has connected", onlineUser);

        await Clients.Others.OnUserConnected(onlineUser);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var onlineUser = await manager.DisconnectUser(Context.User.GetSubjectId());
        logger.LogInformation("User {user} has disconnected", onlineUser);

        await Clients.Others.OnUserDisconnected(onlineUser);

        await base.OnDisconnectedAsync(exception);
    }

    public Task<IReadOnlyCollection<OnlineUser>> GetUsersAsync() => manager.GetUsersAsync();
    public Task<IReadOnlyCollection<Lobby>> GetLobbiesAsync() => manager.GetLobbiesAsync();

    public async Task CreateLobbyAsync(string name)
    {
        var lobby = await manager.CreateLobby(Context.User.GetSubjectId(), name);

        await Clients.All.OnLobbyCreated(lobby);
    }

    public async Task DeleteLobbyAsync(string lobbyId)
    {
        var lobby = await manager.RemoveLobby(Context.User.GetSubjectId(), lobbyId);

        await Clients.All.OnLobbyRemoved(lobby);
    }
}