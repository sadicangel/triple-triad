using System.Threading.Channels;
using TripleTriad.Contracts;

namespace TripleTriad.Lobby;

public sealed class HostLobbySession : ILobbySession
{
    private readonly CancellationTokenSource _lobbyLifetime = new();
    private readonly Dictionary<Seat, LobbyPlayerSnapshot> _players = [];
    private readonly Dictionary<Seat, int[]> _cardSelections = [];
    private readonly TaskCompletionSource<MatchSetup> _matchStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Channel<LobbyUpdate> _updates = Channel.CreateUnbounded<LobbyUpdate>();
    private readonly IMatchTransport _transport;
    private GameRules _rules = GameRules.Default;
    private long _nextSequence;
    private bool _isMatchStarting;
    private bool _started;

    public HostLobbySession(IMatchTransport transport, string hostPlayerName = "Host")
    {
        _transport = transport;
        _players[Seat.Blue] = CreatePlayer(Seat.Blue, hostPlayerName);
        CurrentSnapshot = CreateSnapshot(Seat.Blue);
    }

    public LobbySnapshot CurrentSnapshot { get; private set; }

    public IAsyncEnumerable<LobbyUpdate> ReadUpdatesAsync(
        CancellationToken cancellationToken = default) =>
        _updates.Reader.ReadAllAsync(cancellationToken);

    public ValueTask<LobbySnapshot> StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_started)
            return ValueTask.FromResult(CurrentSnapshot);

        _started = true;
        PublishHostSnapshot();
        _ = PumpTransportMessagesAsync(_lobbyLifetime.Token);
        return ValueTask.FromResult(CurrentSnapshot);
    }

    public async ValueTask SetRulesAsync(
        GameRules rules,
        CancellationToken cancellationToken = default)
    {
        EnsureStarted();
        cancellationToken.ThrowIfCancellationRequested();

        if (_isMatchStarting)
            return;

        _rules = rules;
        await PublishAndBroadcastSnapshotAsync(cancellationToken);
        await _transport.SendAsync(new LobbyRulesChangedNetworkMessage(_rules), cancellationToken);
        await TryStartMatchAsync(cancellationToken);
    }

    public async ValueTask SetSelectedCardsAsync(
        IReadOnlyList<int> cardNumbers,
        CancellationToken cancellationToken = default)
    {
        EnsureStarted();
        cancellationToken.ThrowIfCancellationRequested();

        if (_isMatchStarting)
            return;

        _cardSelections[Seat.Blue] = LobbyCardSelectionRules.Validate(cardNumbers);
        await PublishAndBroadcastSnapshotAsync(cancellationToken);
    }

    public async ValueTask SetReadyAsync(
        bool isReady,
        CancellationToken cancellationToken = default)
    {
        EnsureStarted();
        cancellationToken.ThrowIfCancellationRequested();

        if (_isMatchStarting)
            return;

        var player = _players[Seat.Blue];
        _players[Seat.Blue] = player with { IsReady = isReady };

        await PublishAndBroadcastSnapshotAsync(cancellationToken);
        await _transport.SendAsync(new LobbyReadyChangedNetworkMessage(Seat.Blue, isReady), cancellationToken);
        await TryStartMatchAsync(cancellationToken);
    }

    public ValueTask TakeSeatAsync(
        Seat seat,
        CancellationToken cancellationToken = default)
    {
        EnsureStarted();
        cancellationToken.ThrowIfCancellationRequested();
        throw new NotSupportedException("Online lobby seat switching is not implemented yet.");
    }

    public async ValueTask<MatchSetup> WaitForMatchStartAsync(
        CancellationToken cancellationToken = default) =>
        await _matchStarted.Task.WaitAsync(cancellationToken);

    private async Task PumpTransportMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var message in _transport.ReadMessagesAsync(cancellationToken))
            {
                if (_isMatchStarting)
                    return;

                await HandleMessageAsync(message, cancellationToken);

                if (_isMatchStarting)
                    return;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex)
        {
            PublishUpdate(new LobbyConnectionStateUpdate(NextSequence(), TransportConnectionState.Failed, ex.Message));
            _updates.Writer.TryComplete(ex);
            _matchStarted.TrySetException(ex);
        }
    }

    private async ValueTask HandleMessageAsync(NetworkMessage message, CancellationToken cancellationToken)
    {
        switch (message)
        {
            case LobbyJoinRequestedNetworkMessage join:
                _players[Seat.Red] = CreatePlayer(Seat.Red, join.PlayerName, isReady: false);
                await PublishAndBroadcastSnapshotAsync(cancellationToken);
                break;
            case LobbyReadyChangeRequestedNetworkMessage ready:
                if (!_players.ContainsKey(Seat.Red))
                    _players[Seat.Red] = CreatePlayer(Seat.Red, "Guest", isReady: false);

                var player = _players[Seat.Red];
                _players[Seat.Red] = player with { IsReady = ready.IsReady };

                await PublishAndBroadcastSnapshotAsync(cancellationToken);
                await _transport.SendAsync(new LobbyReadyChangedNetworkMessage(Seat.Red, ready.IsReady), cancellationToken);
                await TryStartMatchAsync(cancellationToken);
                break;
            case LobbyCardSelectionChangeRequestedNetworkMessage selection:
                if (!_players.ContainsKey(Seat.Red))
                    _players[Seat.Red] = CreatePlayer(Seat.Red, "Guest", isReady: false);

                _cardSelections[Seat.Red] = LobbyCardSelectionRules.Validate(selection.CardNumbers);
                await PublishAndBroadcastSnapshotAsync(cancellationToken);
                break;
            case LobbyRulesChangeRequestedNetworkMessage:
                break;
        }
    }

    private async ValueTask PublishAndBroadcastSnapshotAsync(CancellationToken cancellationToken)
    {
        PublishHostSnapshot();

        if (!_players.ContainsKey(Seat.Red))
            return;

        await _transport.SendAsync(
            new LobbySnapshotNetworkMessage(CreateSnapshot(Seat.Red)),
            cancellationToken);
    }

    private async ValueTask TryStartMatchAsync(CancellationToken cancellationToken)
    {
        if (_isMatchStarting || !CanStart())
            return;

        _isMatchStarting = true;
        var setup = new MatchSetup(_rules, CreatePlayerList(), CreateMatchCardSelections());
        CurrentSnapshot = CreateSnapshot(Seat.Blue, isMatchStarting: true);

        PublishUpdate(new LobbySnapshotUpdate(NextSequence(), CurrentSnapshot));
        await _transport.SendAsync(
            new LobbySnapshotNetworkMessage(CreateSnapshot(Seat.Red, isMatchStarting: true)),
            cancellationToken);
        await _transport.SendAsync(new MatchStartedNetworkMessage(setup), cancellationToken);

        PublishUpdate(new LobbyMatchStartedUpdate(NextSequence(), setup));
        _matchStarted.TrySetResult(setup);
        _lobbyLifetime.Cancel();
        _updates.Writer.TryComplete();
    }

    private void PublishHostSnapshot()
    {
        CurrentSnapshot = CreateSnapshot(Seat.Blue);
        PublishUpdate(new LobbySnapshotUpdate(NextSequence(), CurrentSnapshot));
    }

    private LobbySnapshot CreateSnapshot(Seat localSeat, bool isMatchStarting = false)
    {
        var players = CreatePlayerList();
        var canStart = HasReadyPlayer(Seat.Blue) && HasReadyPlayer(Seat.Red);
        return new LobbySnapshot(localSeat, _rules, players, CreateVisibleCardSelections(localSeat), canStart, isMatchStarting);
    }

    private LobbyPlayerSnapshot[] CreatePlayerList() =>
        _players.Values
            .OrderBy(player => player.Seat)
            .ToArray();

    private LobbyCardSelectionSnapshot[] CreateVisibleCardSelections(Seat localSeat)
    {
        if (!_players.TryGetValue(localSeat, out var player)
            || player.Kind != LobbyPlayerKind.Human
            || !_cardSelections.TryGetValue(localSeat, out var cardNumbers))
        {
            return [];
        }

        return [new LobbyCardSelectionSnapshot(localSeat, cardNumbers.ToArray())];
    }

    private LobbyCardSelectionSnapshot[] CreateMatchCardSelections()
    {
        if (_rules.Contains(GameRules.Random))
            return [];

        return _cardSelections
            .Where(pair => _players.TryGetValue(pair.Key, out var player) && player.Kind == LobbyPlayerKind.Human)
            .OrderBy(pair => pair.Key)
            .Select(pair => new LobbyCardSelectionSnapshot(pair.Key, pair.Value.ToArray()))
            .ToArray();
    }

    private bool CanStart() => HasReadyPlayer(Seat.Blue) && HasReadyPlayer(Seat.Red);

    private bool HasReadyPlayer(Seat seat) =>
        _players.TryGetValue(seat, out var player) && player is { IsConnected: true, IsReady: true };

    private void PublishUpdate(LobbyUpdate update) =>
        _updates.Writer.TryWrite(update);

    private long NextSequence() => Interlocked.Increment(ref _nextSequence);

    private void EnsureStarted()
    {
        if (!_started)
            throw new InvalidOperationException("StartAsync must complete before using the lobby.");
    }

    private static LobbyPlayerSnapshot CreatePlayer(
        Seat seat,
        string playerName,
        bool isReady = false) =>
        new(seat, NormalizePlayerName(playerName, seat), isReady);

    private static string NormalizePlayerName(string playerName, Seat seat) =>
        string.IsNullOrWhiteSpace(playerName) ? seat.ToString() : playerName.Trim();
}
