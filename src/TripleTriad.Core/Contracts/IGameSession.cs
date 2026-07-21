namespace TripleTriad.Contracts;

public interface IGameSession
{
    GameRules Rules { get; }

    MatchSnapshot? CurrentSnapshot { get; }

    SessionConnectionState ConnectionState { get; }

    /// <summary>
    /// Subscribes to future session events. Each caller receives its own event stream.
    /// </summary>
    IAsyncEnumerable<GameEvent> SubscribeEventsAsync(
        CancellationToken cancellationToken = default);

    ValueTask<MatchSnapshot> StartAsync(CancellationToken cancellationToken = default);

    ValueTask SendCommandAsync(
        GameCommand command,
        CancellationToken cancellationToken = default);
}
