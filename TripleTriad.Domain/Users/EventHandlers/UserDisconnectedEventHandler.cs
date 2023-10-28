using MediatR;
using Microsoft.AspNetCore.SignalR;
using TripleTriad.Services;
using TripleTriad.Users.Events;

namespace TripleTriad.Users.EventHandlers;

internal sealed class UserDisconnectedEventHandler : INotificationHandler<UserDisconnectedEvent>
{
    private readonly IHubContext<TripleTriadHub> _hubContext;

    public UserDisconnectedEventHandler(IHubContext<TripleTriadHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(UserDisconnectedEvent notification, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.All.SendAsync(notification, cancellationToken);
    }
}