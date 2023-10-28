using MediatR;
using Microsoft.AspNetCore.SignalR;
using TripleTriad.Services;
using TripleTriad.Users.Events;

namespace TripleTriad.Users.EventHandlers;
internal sealed class UserConnectedEventHandler : INotificationHandler<UserConnectedEvent>
{
    private readonly IHubContext<TripleTriadHub> _hubContext;

    public UserConnectedEventHandler(IHubContext<TripleTriadHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(UserConnectedEvent notification, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.All.SendAsync(notification, cancellationToken);
    }
}
