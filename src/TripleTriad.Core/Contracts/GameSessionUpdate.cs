namespace TripleTriad.Contracts;

public enum SessionConnectionState
{
    NotStarted,
    Connecting,
    Connected,
    Reconnecting,
    Disconnected,
    Failed,
    Closed,
}

public abstract record GameSessionUpdate(long Sequence);

public sealed record GameSessionSnapshotUpdate(
    long Sequence,
    MatchSnapshot Snapshot) : GameSessionUpdate(Sequence);

public sealed record GameSessionEventUpdate(
    long Sequence,
    GameEvent GameEvent) : GameSessionUpdate(Sequence);

public sealed record GameSessionConnectionStateUpdate(
    long Sequence,
    SessionConnectionState State,
    string? Reason = null) : GameSessionUpdate(Sequence);
