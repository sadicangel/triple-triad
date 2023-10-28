using MediatR;
using Microsoft.AspNetCore.SignalR;
using TripleTriad.Lobbies.Events;
using TripleTriad.Services;

namespace TripleTriad.Lobbies.EventHandlers;

internal sealed class UserLeftEventHandler : INotificationHandler<UserLeftLobbyEvent>
{
    private readonly IHubContext<TripleTriadHub> _hubContext;

    public UserLeftEventHandler(IHubContext<TripleTriadHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(UserLeftLobbyEvent notification, CancellationToken cancellationToken)
    {
        var userIds = notification.Data.Lobby.Users
            .Where(u => u.UserId != notification.Data.User.Id)
            .Select(u => u.UserId);

        await _hubContext.Clients.Users(userIds).SendAsync(notification, cancellationToken);
    }
}