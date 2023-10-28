using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TripleTriad.Games;
using TripleTriad.Lobbies.Commands;
using TripleTriad.Lobbies.Dtos;
using TripleTriad.Services;
using TripleTriad.Users.Commands;

namespace TripleTriad.Services;

public sealed class TripleTriadHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, TripleTriadGame> _games = new();
    private readonly ISender _mediator;

    public TripleTriadHub(ISender mediator)
    {
        _mediator = mediator;
    }

    public override async Task OnConnectedAsync()
    {
        await _mediator.Send(new ConnectUserCommand { UserId = Context.UserIdentifier! });
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _mediator.Send(new DisconnectUserCommand { UserId = Context.UserIdentifier! });
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<LobbyDto> HostLobby(string lobbyDisplayName)
    {
        return await _mediator.Send(new HostLobbyCommand
        {
            UserId = Context.UserIdentifier!,
            LobbyDisplayName = lobbyDisplayName
        });
    }

    public async Task<LobbyDto> JoinLobby(string lobbyId)
    {
        return await _mediator.Send(new JoinLobbyCommand
        {
            UserId = Context.UserIdentifier!,
            LobbyId = lobbyId,
        });
    }

    public async Task UpdateLobby(string lobbyId, bool isReady, string? displayName = null, Ruleset? rules = null)
    {
        await _mediator.Send(new UpdateLobbyCommand
        {
            LobbyId = lobbyId,
            UserId = Context.UserIdentifier!,
            DiplayName = displayName,
            Rules = rules,
            IsReady = isReady
        });
    }

    public async Task LeaveLobby(string lobbyId)
    {
        await _mediator.Send(new LeaveLobbyCommand
        {
            LobbyId = lobbyId,
            UserId = Context.UserIdentifier!
        });
    }
}

internal static class HubResponseExtensions
{
    public static Task SendAsync<T>(this IClientProxy proxy, T message, CancellationToken cancellationToken = default)
    {
        return proxy.SendAsync(typeof(T).Name, message, cancellationToken);
    }
}