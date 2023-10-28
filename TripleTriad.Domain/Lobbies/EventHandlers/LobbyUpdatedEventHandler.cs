using MediatR;
using Microsoft.AspNetCore.SignalR;
using TripleTriad.Lobbies.Events;
using TripleTriad.Services;

namespace TripleTriad.Lobbies.EventHandlers;

internal sealed class LobbyUpdatedEventHandler : INotificationHandler<LobbyUpdatedEvent>
{
    private readonly IHubContext<TripleTriadHub> _hubContext;

    public LobbyUpdatedEventHandler(IHubContext<TripleTriadHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(LobbyUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var userIds = notification.Data.Users.Select(u => u.UserId);
        await _hubContext.Clients.Users(userIds).SendAsync(notification, cancellationToken);
    }
}