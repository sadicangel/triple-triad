namespace TripleTriad.Messaging;
public abstract class GameEvent<T>
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public string Type { get; init; } = null!
}
