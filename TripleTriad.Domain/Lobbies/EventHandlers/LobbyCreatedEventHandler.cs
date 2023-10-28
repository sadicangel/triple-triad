using MediatR;
using Microsoft.AspNetCore.SignalR;
using TripleTriad.Lobbies.Events;
using TripleTriad.Services;

namespace TripleTriad.Lobbies.EventHandlers;
internal sealed class LobbyCreatedEventHandler : INotificationHandler<LobbyCreatedEvent>
{
    private readonly IHubContext<TripleTriadHub> _hubContext;

    public LobbyCreatedEventHandler(IHubContext<TripleTriadHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(LobbyCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.All.SendAsync(notification, cancellationToken);
    }
}