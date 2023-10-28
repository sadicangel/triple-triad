using MediatR;
using Microsoft.AspNetCore.SignalR;
using TripleTriad.Lobbies.Events;
using TripleTriad.Services;

namespace TripleTriad.Lobbies.EventHandlers;

internal sealed class UserJoinedEventHandler : INotificationHandler<UserJoinedLobbyEvent>
{
    private readonly IHubContext<TripleTriadHub> _hubContext;

    public UserJoinedEventHandler(IHubContext<TripleTriadHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(UserJoinedLobbyEvent notification, CancellationToken cancellationToken)
    {
        var userIds = notification.Data.Lobby.Users
            .Where(u => u.UserId != notification.Data.User.Id)
            .Select(u => u.UserId);

        await _hubContext.Clients.Users(userIds).SendAsync(notification, cancellationToken);
    }
}