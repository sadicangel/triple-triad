using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.SignalR.Client;
using TripleTriad.Games;
using TripleTriad.Interfaces;
using TripleTriad.Lobbies.Dtos;
using TripleTriad.Lobbies.Events;
using TripleTriad.Server.Events;
using TripleTriad.Services;
using TripleTriad.Users.Events;

namespace TripleTriad.Services;
public sealed class TripleTriadClient : ITripleTriadClient
{
    private readonly IMessenger _messenger;
    private readonly HubConnection _connection;
    private readonly List<IDisposable> _subscriptions;

    public TripleTriadClient(IMessenger messenger, HubConnection connection)
    {
        _messenger = messenger;
        _connection = connection;
        _subscriptions = new List<IDisposable>
        {
            _connection.On<ServerMessageSentEvent>(Dispatch),

            _connection.On<UserConnectedEvent>(Dispatch),
            _connection.On<UserDisconnectedEvent>(Dispatch),

            _connection.On<LobbyCreatedEvent>(Dispatch),
            _connection.On<LobbyDeletedEvent>(Dispatch),
            _connection.On<LobbyUpdatedEvent>(Dispatch),
            _connection.On<UserJoinedLobbyEvent>(Dispatch),
            _connection.On<UserLeftLobbyEvent>(Dispatch),
        };
        void Dispatch<T>(T @event) where T : class => _messenger.Send<T>(@event);
    }

    public void Dispose() => _subscriptions.ForEach(s => s.Dispose());

    public async Task<LobbyDto> HostLobby(string lobbyDisplayName) =>
        await _connection.InvokeAsync<LobbyDto>(nameof(HostLobby), lobbyDisplayName);

    public async Task<LobbyDto> JoinLobby(string lobbyId) =>
        await _connection.InvokeAsync<LobbyDto>(nameof(JoinLobby), lobbyId);

    public async Task UpdateLobby(string lobbyId, bool isReady, string? displayName = null, Ruleset? rules = null) =>
        await _connection.InvokeAsync(nameof(UpdateLobby), lobbyId, isReady, displayName, rules);

    public async Task LeaveLobby(string lobbyId) =>
        await _connection.InvokeAsync(nameof(LeaveLobby), lobbyId);
}

internal static class HubRequestExtensions
{
    public static IDisposable On<T>(this HubConnection connection, Action<T> handler)
    {
        return connection.On(typeof(T).Name, handler);
    }
}
