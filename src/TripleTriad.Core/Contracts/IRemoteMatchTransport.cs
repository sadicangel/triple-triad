namespace TripleTriad.Contracts;

public interface IRemoteMatchTransport
{
    IAsyncEnumerable<GameSessionUpdate> ReadUpdatesAsync(
        CancellationToken cancellationToken = default);

    ValueTask SendCommandAsync(
        GameCommand command,
        CancellationToken cancellationToken = default);
}
