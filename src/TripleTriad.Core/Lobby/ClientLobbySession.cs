using System.Threading.Channels;
using TripleTriad.Contracts;

namespace TripleTriad.Lobby;

public sealed class ClientLobbySession : ILobbySession
{
    private readonly CancellationTokenSource _lobbyLifetime = new();
    private readonly TaskCompletionSource<MatchSetup> _matchStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Channel<LobbyUpdate> _updates = Channel.CreateUnbounded<LobbyUpdate>();
    private readonly string _playerName;
    private readonly IMatchTransport _transport;
    private long _nextSequence;
    private bool _started;

    public ClientLobbySession(IMatchTransport transport, string playerName = "Guest")
    {
        _transport = transport;
        _playerName = NormalizePlayerName(playerName);
        CurrentSnapshot = new LobbySnapshot(Seat.Red, GameRules.Default, [], CanStart: false, IsMatchStarting: false);
    }

    public LobbySnapshot CurrentSnapshot { get; private set; }

    public IAsyncEnumerable<LobbyUpdate> ReadUpdatesAsync(
        CancellationToken cancellationToken = default) =>
        _updates.Reader.ReadAllAsync(cancellationToken);

    public async ValueTask<LobbySnapshot> StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_started)
            return CurrentSnapshot;

        _started = true;
        _ = PumpTransportMessagesAsync(_lobbyLifetime.Token);
        await _transport.SendAsync(new LobbyJoinRequestedNetworkMessage(_playerName), cancellationToken);
        return CurrentSnapshot;
    }

    public async ValueTask SetRulesAsync(
        GameRules rules,
        CancellationToken cancellationToken = default)
    {
        EnsureStarted();
        await _transport.SendAsync(new LobbyRulesChangeRequestedNetworkMessage(rules), cancellationToken);
    }

    public async ValueTask SetReadyAsync(
        bool isReady,
        CancellationToken cancellationToken = default)
    {
        EnsureStarted();
        await _transport.SendAsync(new LobbyReadyChangeRequestedNetworkMessage(isReady), cancellationToken);
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
                switch (message)
                {
                    case LobbySnapshotNetworkMessage snapshot:
                        CurrentSnapshot = Localize(snapshot.Snapshot);
                        PublishUpdate(new LobbySnapshotUpdate(NextSequence(), CurrentSnapshot));
                        break;
                    case MatchStartedNetworkMessage started:
                        PublishMatchStarted(started.Setup);
                        return;
                }
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

    private void PublishMatchStarted(MatchSetup setup)
    {
        _matchStarted.TrySetResult(setup);
        PublishUpdate(new LobbyMatchStartedUpdate(NextSequence(), setup));
        _lobbyLifetime.Cancel();
        _updates.Writer.TryComplete();
    }

    private LobbySnapshot Localize(LobbySnapshot snapshot) =>
        snapshot with { LocalSeat = Seat.Red };

    private void PublishUpdate(LobbyUpdate update) =>
        _updates.Writer.TryWrite(update);

    private long NextSequence() => Interlocked.Increment(ref _nextSequence);

    private void EnsureStarted()
    {
        if (!_started)
            throw new InvalidOperationException("StartAsync must complete before using the lobby.");
    }

    private static string NormalizePlayerName(string playerName) =>
        string.IsNullOrWhiteSpace(playerName) ? "Guest" : playerName.Trim();
}
