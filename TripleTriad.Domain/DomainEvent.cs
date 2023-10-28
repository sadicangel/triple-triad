using MediatR;

namespace TripleTriad;

public abstract class DomainEvent : INotification
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public abstract string Type { get; init; }
}

public abstract class DomainEvent<TData> : DomainEvent
{
    public required TData Data { get; init; } = default!;
}