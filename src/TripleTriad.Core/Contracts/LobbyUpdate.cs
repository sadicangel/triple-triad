namespace TripleTriad.Contracts;

public abstract record LobbyUpdate(long Sequence);

public sealed record LobbySnapshotUpdate(
    long Sequence,
    LobbySnapshot Snapshot) : LobbyUpdate(Sequence);

public sealed record LobbyMatchStartedUpdate(
    long Sequence,
    MatchSetup Setup) : LobbyUpdate(Sequence);

public sealed record LobbyConnectionStateUpdate(
    long Sequence,
    TransportConnectionState State,
    string? Reason = null) : LobbyUpdate(Sequence);
