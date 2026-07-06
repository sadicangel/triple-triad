namespace TripleTriad.Contracts;

public interface IGameSession
{
    GameRules Rules { get; }

    MatchSnapshot? CurrentSnapshot { get; }

    SessionConnectionState ConnectionState { get; }

    IAsyncEnumerable<GameSessionUpdate> ReadUpdatesAsync(
        CancellationToken cancellationToken = default);

    ValueTask<MatchSnapshot> StartAsync(CancellationToken cancellationToken = default);

    ValueTask SendCommandAsync(
        GameCommand command,
        CancellationToken cancellationToken = default);
}
