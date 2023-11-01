using Microsoft.EntityFrameworkCore;
using TripleTriad.ServerState;

namespace TripleTriad;

internal sealed class ServerManager(IDbContextFactory<DataContext> dbContextFactory)
{
    private readonly Dictionary<string, OnlineUser> _onlineUsers = [];
    private readonly Dictionary<string, OnlineLobby> _onlineLobbies = [];

    public async Task<OnlineUser> AddOnlineUser(string userId)
    {
        using var dataContext = dbContextFactory.CreateDbContext();
        if (!Guid.TryParse(userId, out var id)) throw new InvalidOperationException($"User {userId} is not valid");
        var user = await dataContext.Users.FindAsync(id) ?? throw new InvalidOperationException($"User {userId} is not valid");
        var onlineUser = user.ToOnlineUser();
        _onlineUsers[userId] = onlineUser;
        return onlineUser;
    }

    public Task<OnlineUser> RemoveOnlineUser(string userId)
    {
        return _onlineUsers.Remove(userId, out var onlineUser) ? Task.FromResult(onlineUser) : throw new InvalidOperationException($"User {userId} is not valid");
    }

    public ServerStateResponse GetServerState() => new(_onlineUsers.Values, _onlineLobbies.Values);
}
