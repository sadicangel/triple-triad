namespace TripleTriad.Bridge;

public interface IGameSession
{
    MatchSnapshot CurrentSnapshot { get; }

    event Action<MatchSnapshot>? SnapshotChanged;

    event Action<GameEvent>? EventRaised;

    ValueTask SubmitAsync(GameCommand command, CancellationToken cancellationToken = default);
}
