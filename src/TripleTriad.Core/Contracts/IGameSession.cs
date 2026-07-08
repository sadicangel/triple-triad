namespace TripleTriad.Contracts;

public interface IGameSession
{
    GameRules Rules { get; }

    MatchSnapshot? CurrentSnapshot { get; }

    SessionConnectionState ConnectionState { get; }

    /// <summary>
    /// Subscribes to future session updates. Each caller receives its own update stream.
    /// </summary>
    IAsyncEnumerable<GameSessionUpdate> SubscribeUpdatesAsync(
        CancellationToken cancellationToken = default);

    ValueTask<MatchSnapshot> StartAsync(CancellationToken cancellationToken = default);

    ValueTask SendCommandAsync(
        GameCommand command,
        CancellationToken cancellationToken = default);
}
