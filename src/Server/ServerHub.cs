using Domain;
using Microsoft.AspNetCore.SignalR;

internal sealed class ServerHub : Hub<IGameClient>
{
    private readonly ILogger<ServerHub> _logger;

    public ServerHub(ILogger<ServerHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("User '{connectionId}' has connected", Context.ConnectionId);
        Clients.Others.OnUserConnected(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User '{connectionId}' has disconnected", Context.ConnectionId);
        Clients.Others.OnUserDisconnected(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}