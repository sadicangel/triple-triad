using Microsoft.EntityFrameworkCore;

namespace TripleTriad;

internal sealed class ServerManager(IDbContextFactory<DataContext> dbContextFactory)
{
    private readonly Dictionary<string, OnlineUser> _users = [];
    private readonly Dictionary<string, Lobby> _lobbies = [];

    public async Task<OnlineUser> ConnectUser(string userId)
    {
        using var dataContext = dbContextFactory.CreateDbContext();
        var user = await dataContext.Users.FindAsync(userId) ?? throw new InvalidOperationException($"User {userId} is not valid");
        var onlineUser = user.ToOnlineUser();
        _users[userId] = onlineUser;
        return onlineUser;
    }

    public Task<OnlineUser> DisconnectUser(string userId)
    {
        return _users.Remove(userId, out var onlineUser) ? Task.FromResult(onlineUser) : throw new InvalidOperationException($"User {userId} is not valid");
    }

    public async Task<IReadOnlyCollection<OnlineUser>> GetUsersAsync() => await Task.FromResult(_users.Values);
    public async Task<IReadOnlyCollection<Lobby>> GetLobbiesAsync() => await Task.FromResult(_lobbies.Values);

    public Task<Lobby> CreateLobby(string userId, string name)
    {
        var lobby = new Lobby(Guid.NewGuid().ToString(), name, userId, [userId]);
        _lobbies[lobby.LobbyId] = lobby;
        return Task.FromResult(lobby);
    }

    public Task<Lobby> RemoveLobby(string userId, string lobbyId)
    {
        if (!_lobbies.TryGetValue(userId, out var lobby))
            throw new InvalidOperationException($"Lobby {lobbyId} is not valid");

        if (lobby.Owner != userId)
            throw new InvalidOperationException($"Unauthorized");

        _lobbies.Remove(lobbyId);

        return Task.FromResult(lobby);
    }
}
