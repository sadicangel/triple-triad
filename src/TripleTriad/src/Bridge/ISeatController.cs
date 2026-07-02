namespace TripleTriad.Bridge;

public interface ISeatController
{
    Seat Seat { get; }

    ValueTask<GameCommand?> GetNextCommandAsync(
        MatchSnapshot snapshot,
        CancellationToken cancellationToken = default);
}

public sealed class HumanSeatController(Seat seat) : ISeatController
{
    public Seat Seat { get; } = seat;

    public ValueTask<GameCommand?> GetNextCommandAsync(
        MatchSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        _ = snapshot;
        _ = cancellationToken;
        return ValueTask.FromResult<GameCommand?>(null);
    }
}

public sealed class AiSeatController(Seat seat) : ISeatController
{
    public Seat Seat { get; } = seat;

    public ValueTask<GameCommand?> GetNextCommandAsync(
        MatchSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        _ = snapshot;
        _ = cancellationToken;
        return ValueTask.FromResult<GameCommand?>(null);
    }
}

public sealed class RemoteSeatController(Seat seat) : ISeatController
{
    public Seat Seat { get; } = seat;

    public ValueTask<GameCommand?> GetNextCommandAsync(
        MatchSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        _ = snapshot;
        _ = cancellationToken;
        return ValueTask.FromResult<GameCommand?>(null);
    }
}
