using System.Threading.Channels;
using TripleTriad.Contracts;

namespace TripleTriad.Lobby;

public enum LocalLobbyMode
{
    Solo,
    Host,
}

public sealed class LocalLobbySession : ILobbySession
{
    private readonly TaskCompletionSource<MatchSetup> _matchStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Dictionary<Seat, LobbyPlayerSnapshot> _players = [];
    private readonly Dictionary<Seat, int[]> _cardSelections = [];
    private readonly Channel<LobbyUpdate> _updates = Channel.CreateUnbounded<LobbyUpdate>();
    private readonly LocalLobbyMode _mode;
    private readonly string _localPlayerName;
    private GameRules _rules = GameRules.Default;
    private long _nextSequence;
    private bool _isMatchStarting;
    private bool _started;

    public LocalLobbySession(
        LocalLobbyMode mode,
        string localPlayerName = "Player",
        Seat localSeat = Seat.Blue)
    {
        _mode = mode;
        _localPlayerName = NormalizePlayerName(localPlayerName, "Player");
        _players[localSeat] = CreatePlayer(localSeat, _localPlayerName, LobbyPlayerKind.Human, isReady: false);

        if (_mode == LocalLobbyMode.Solo)
            EnsureSoloAi(localSeat.Opponent());

        CurrentSnapshot = CreateSnapshot(localSeat);
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
        PublishSnapshot();
        return ValueTask.FromResult(CurrentSnapshot);
    }

    public ValueTask SetRulesAsync(
        GameRules rules,
        CancellationToken cancellationToken = default)
    {
        EnsureStarted();
        cancellationToken.ThrowIfCancellationRequested();

        if (_isMatchStarting)
            return ValueTask.CompletedTask;

        _rules = rules;
        PublishSnapshot();
        return ValueTask.CompletedTask;
    }

    public ValueTask TakeSeatAsync(
        Seat seat,
        CancellationToken cancellationToken = default)
    {
        EnsureStarted();
        cancellationToken.ThrowIfCancellationRequested();

        if (_isMatchStarting || !CanTakeSeat(seat))
            return ValueTask.CompletedTask;

        var currentSeat = CurrentSnapshot.LocalSeat;
        var localPlayer = _players[currentSeat] with { Seat = seat, IsReady = false };
        _players.Remove(currentSeat);
        _players[seat] = localPlayer;

        if (_cardSelections.Remove(currentSeat, out var selectedCards))
            _cardSelections[seat] = selectedCards;

        if (_mode == LocalLobbyMode.Solo)
            EnsureSoloAi(seat.Opponent());

        CurrentSnapshot = CreateSnapshot(seat);
        PublishUpdate(new LobbySnapshotUpdate(NextSequence(), CurrentSnapshot));
        return ValueTask.CompletedTask;
    }

    public ValueTask SetSelectedCardsAsync(
        IReadOnlyList<int> cardNumbers,
        CancellationToken cancellationToken = default)
    {
        EnsureStarted();
        cancellationToken.ThrowIfCancellationRequested();

        if (_isMatchStarting)
            return ValueTask.CompletedTask;

        _cardSelections[CurrentSnapshot.LocalSeat] = LobbyCardSelectionRules.Validate(cardNumbers);
        PublishSnapshot();
        return ValueTask.CompletedTask;
    }

    public ValueTask SetReadyAsync(
        bool isReady,
        CancellationToken cancellationToken = default)
    {
        EnsureStarted();
        cancellationToken.ThrowIfCancellationRequested();

        if (_isMatchStarting)
            return ValueTask.CompletedTask;

        var localSeat = CurrentSnapshot.LocalSeat;
        _players[localSeat] = _players[localSeat] with { IsReady = isReady };
        PublishSnapshot();

        if (isReady && CanStart(localSeat))
            StartMatch();

        return ValueTask.CompletedTask;
    }

    public async ValueTask<MatchSetup> WaitForMatchStartAsync(
        CancellationToken cancellationToken = default) =>
        await _matchStarted.Task.WaitAsync(cancellationToken);

    public bool CanTakeSeat(Seat seat)
    {
        if (_isMatchStarting)
            return false;

        if (!_players.TryGetValue(seat, out var player))
            return true;

        return player.Kind == LobbyPlayerKind.AI;
    }

    private void StartMatch()
    {
        if (_isMatchStarting)
            return;

        _isMatchStarting = true;
        var setup = new MatchSetup(_rules, CreatePlayerList(), CreateMatchCardSelections());
        CurrentSnapshot = CreateSnapshot(CurrentSnapshot.LocalSeat, isMatchStarting: true);

        PublishUpdate(new LobbySnapshotUpdate(NextSequence(), CurrentSnapshot));
        PublishUpdate(new LobbyMatchStartedUpdate(NextSequence(), setup));
        _matchStarted.TrySetResult(setup);
        _updates.Writer.TryComplete();
    }

    private bool CanStart(Seat localSeat) =>
        _mode == LocalLobbyMode.Solo
            ? HasPlayer(localSeat, LobbyPlayerKind.Human)
              && HasPlayer(localSeat.Opponent(), LobbyPlayerKind.AI)
            : HasReadyHuman(Seat.Blue) && HasReadyHuman(Seat.Red);

    private bool HasReadyHuman(Seat seat) =>
        _players.TryGetValue(seat, out var player)
        && player is { Kind: LobbyPlayerKind.Human, IsConnected: true, IsReady: true };

    private bool HasPlayer(Seat seat, LobbyPlayerKind kind) =>
        _players.TryGetValue(seat, out var player)
        && player.Kind == kind
        && player.IsConnected;

    private void EnsureSoloAi(Seat aiSeat)
    {
        foreach (var ai in _players.Where(pair => pair.Value.Kind == LobbyPlayerKind.AI).Select(pair => pair.Key).ToArray())
            _players.Remove(ai);

        _players[aiSeat] = CreatePlayer(aiSeat, "AI", LobbyPlayerKind.AI, isReady: true);
    }

    private void PublishSnapshot()
    {
        CurrentSnapshot = CreateSnapshot(CurrentSnapshot.LocalSeat);
        PublishUpdate(new LobbySnapshotUpdate(NextSequence(), CurrentSnapshot));
    }

    private LobbySnapshot CreateSnapshot(Seat localSeat, bool isMatchStarting = false) =>
        new(localSeat, _rules, CreatePlayerList(), CreateVisibleCardSelections(localSeat), CanStart(localSeat), isMatchStarting);

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
        LobbyPlayerKind kind,
        bool isReady) =>
        new(seat, NormalizePlayerName(playerName, seat.ToString()), isReady, kind);

    private static string NormalizePlayerName(string playerName, string fallback) =>
        string.IsNullOrWhiteSpace(playerName) ? fallback : playerName.Trim();
}
