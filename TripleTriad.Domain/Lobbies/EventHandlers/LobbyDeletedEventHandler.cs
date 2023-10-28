using MediatR;
using Microsoft.AspNetCore.SignalR;
using TripleTriad.Lobbies.Events;
using TripleTriad.Services;

namespace TripleTriad.Lobbies.EventHandlers;

internal sealed class LobbyDeletedEventHandler : INotificationHandler<LobbyDeletedEvent>
{
    private readonly IHubContext<TripleTriadHub> _hubContext;

    public LobbyDeletedEventHandler(IHubContext<TripleTriadHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(LobbyDeletedEvent notification, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.All.SendAsync(notification, cancellationToken);
    }
}
