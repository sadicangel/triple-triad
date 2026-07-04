namespace TripleTriad.Contracts;

public interface IRemoteMatchTransport
{
    event Action<GameCommand>? CommandReceived;

    event Action<GameEvent>? EventReceived;

    ValueTask SendCommandAsync(GameCommand command, CancellationToken cancellationToken = default);

    ValueTask SendEventAsync(GameEvent gameEvent, CancellationToken cancellationToken = default);
}
