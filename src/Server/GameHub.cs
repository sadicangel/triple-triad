using Domain;
using Microsoft.AspNetCore.SignalR;

internal sealed class GameHub : Hub<IGameClient>
{
    private readonly ILogger<GameHub> _logger;

    public GameHub(ILogger<GameHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("User '{connectionId}' has connected", Context.ConnectionId);
        await Clients.Others.OnUserConnected(Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User '{connectionId}' has disconnected", Context.ConnectionId);
        await Clients.Others.OnUserDisconnected(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string username, string message)
    {
        _logger.LogInformation("User '{username}' sent message {message}", username, message);
        await Clients.Others.OnMessageReceived(username, message);
    }
}