using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TripleTriad.Games;
using TripleTriad.Lobbies.Dtos;
using TripleTriad.Lobbies.Events;
using TripleTriad.Server.Events;
using TripleTriad.Services;
using TripleTriad.Users.Commands;

namespace TripleTriad.CLI;
internal sealed class TripleTriadBot :
    IAsyncDisposable,
    IRecipient<ServerMessageSentEvent>,
    IRecipient<LobbyCreatedEvent>,
    IRecipient<LobbyUpdatedEvent>,
    IRecipient<LobbyDeletedEvent>,
    IRecipient<UserJoinedLobbyEvent>,
    IRecipient<UserLeftLobbyEvent>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        WriteIndented = true
    };
    private readonly string _userName;
    private readonly string _userId;
    private readonly ConsoleColor _color;
    private readonly IMessenger _messenger;
    private readonly HubConnection _connection;
    private readonly TripleTriadClient _client;
    private readonly ConcurrentDictionary<string, LobbyDto> _lobbies;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DateTimeOffset>> _events;

    public LobbyDto? Lobby { get; set; }

    private TripleTriadBot(string userName, string userId, ConsoleColor color, IMessenger messenger, HubConnection connection)
    {
        _userName = userName;
        _userId = userId;
        _color = color;
        _messenger = messenger;
        _connection = connection;
        _client = new TripleTriadClient(messenger, connection);
        _lobbies = new ConcurrentDictionary<string, LobbyDto>();
        _events = new ConcurrentDictionary<string, ConcurrentDictionary<string, DateTimeOffset>>();

        messenger.Register<ServerMessageSentEvent>(this);

        messenger.Register<LobbyCreatedEvent>(this);
        messenger.Register<LobbyUpdatedEvent>(this);
        messenger.Register<LobbyDeletedEvent>(this);
        messenger.Register<UserJoinedLobbyEvent>(this);
        messenger.Register<UserLeftLobbyEvent>(this);
    }

    public async static Task<TripleTriadBot> CreateAndConnectAsync(string userName, string password, ConsoleColor color, IMessenger messenger, HttpClient httpClient, string hubUrl)
    {
        var response = await httpClient.PostAsync("register", JsonContent.Create(new RegisterUserCommand
        {
            Username = userName,
            Password = password,
        }));
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            response = await httpClient.PostAsync("login", JsonContent.Create(new LoginUserCommand
            {
                Username = userName,
                Password = password,
            }));
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
            }
        }
        var accessToken = await response.Content.ReadFromJsonAsync<string>();
        var userId = new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Claims.Single(c => c.Type == "nameid").Value;

        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, opts => opts.AccessTokenProvider = () => Task.FromResult(accessToken))
            .Build();

        await connection.StartAsync();

        return new TripleTriadBot(userName, userId, color, messenger, connection);
    }

    public async Task HostLobby(string lobbyDisplayName)
    {
        Lobby = await _client.HostLobby(lobbyDisplayName);
    }

    public async Task JoinLobby(string lobbyId)
    {
        Lobby = await _client.JoinLobby(lobbyId);
    }

    public async Task UpdateLobby(bool isReady, string? displayName = null, Ruleset? rules = null)
    {
        await _client.UpdateLobby(Lobby!.Id, isReady, displayName, rules);
    }

    public async Task LeaveLobby()
    {
        await _client.LeaveLobby(Lobby!.Id);
    }

    public ValueTask DisposeAsync()
    {
        _messenger.UnregisterAll(this);
        return _connection.DisposeAsync();
    }

    private static void WriteToConsole(ConsoleColor color, string message)
    {
        lock (Console.Out)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    // Because we're using the same host for both clients, need this to prevent processing the same message multiple times.
    private bool IsRepeatedEvent(DomainEvent @event)
    {
        var now = DateTimeOffset.UtcNow;
        var events = _events.GetOrAdd(@event.Type, _ => new ConcurrentDictionary<string, DateTimeOffset>());
        if (!events.TryAdd(@event.Id, now))
        {
            events[@event.Id] = now;
            return false;
        }
        // Purge old events?
        lock (events)
        {
            // Remove old events to keep the list small.
            foreach (var key in events.Keys.ToList())
            {
                if (now - events[key] > TimeSpan.FromSeconds(30))
                    events.Remove(key, out _);
            }
        }
        return true;
    }

    public void Receive(ServerMessageSentEvent message)
    {
        if (IsRepeatedEvent(message))
            return;

        WriteToConsole(ConsoleColor.Red, $"{_userName} received event {message.Type}: {JsonSerializer.Serialize(message, JsonSerializerOptions)}");
    }

    public void Receive(LobbyCreatedEvent message)
    {
        if (IsRepeatedEvent(message))
            return;

        _lobbies.TryAdd(message.Data.Id, message.Data);

        WriteToConsole(_color, $"{_userName} received event {message.Type}: {JsonSerializer.Serialize(message, JsonSerializerOptions)}");
    }

    public void Receive(LobbyUpdatedEvent message)
    {
        if (IsRepeatedEvent(message))
            return;

        Debug.Assert(Lobby?.Id == message.Data.Id);
        Lobby = _lobbies.AddOrUpdate(message.Data.Id, message.Data, (_, _) => message.Data);
        WriteToConsole(_color, $"{_userName} received event {message.Type}: {JsonSerializer.Serialize(message, JsonSerializerOptions)}");
    }

    public void Receive(LobbyDeletedEvent message)
    {
        if (IsRepeatedEvent(message))
            return;

        if (_lobbies.TryRemove(message.Data.Id, out var lobby) && lobby.Id == Lobby?.Id)
            Lobby = null;
        WriteToConsole(_color, $"{_userName} received event {message.Type}: {JsonSerializer.Serialize(message, JsonSerializerOptions)}");
    }

    public void Receive(UserJoinedLobbyEvent message)
    {
        if (IsRepeatedEvent(message))
            return;

        var lobby = message.Data.Lobby;
        _lobbies.AddOrUpdate(lobby.Id, lobby, (_, _) => lobby);
        WriteToConsole(_color, $"{_userName} received event {message.Type}: {JsonSerializer.Serialize(message, JsonSerializerOptions)}");
    }

    public void Receive(UserLeftLobbyEvent message)
    {
        if (IsRepeatedEvent(message))
            return;

        lock (_lobbies)
        {
            if (message.Data.User.Id == _userId)
                Lobby = null;
            else
                Lobby = message.Data.Lobby;
        }
        WriteToConsole(_color, $"{_userName} received event {message.Type}: {JsonSerializer.Serialize(message, JsonSerializerOptions)}");
    }
}
