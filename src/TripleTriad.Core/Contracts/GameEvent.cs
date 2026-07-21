namespace TripleTriad.Contracts;

public static class GameEventTypes
{
    public const string MatchStarted = "match_started";
    public const string CardPlayed = "card_played";
    public const string CardCaptured = "card_captured";
    public const string TurnChanged = "turn_changed";
    public const string MoveRejected = "move_rejected";
    public const string MatchEnded = "match_ended";
}

public abstract record GameEvent(
    long Sequence,
    MatchSnapshot Snapshot,
    string? ClientRequestId,
    string Type);

public sealed record MatchStartedEvent(
    long Sequence,
    MatchSnapshot Snapshot,
    Seat StartingSeat)
    : GameEvent(Sequence, Snapshot, null, GameEventTypes.MatchStarted);

public sealed record CardPlayedEvent(
    long Sequence,
    MatchSnapshot Snapshot,
    string CardInstanceId,
    CardSnapshot Card,
    Seat SourceSeat,
    int SourceHandIndex,
    int BoardSlotIndex,
    Seat Seat,
    string? ClientRequestId)
    : GameEvent(Sequence, Snapshot, ClientRequestId, GameEventTypes.CardPlayed);

public sealed record CardCapturedEvent(
    long Sequence,
    MatchSnapshot Snapshot,
    string CardInstanceId,
    CardSnapshot Card,
    int BoardSlotIndex,
    Seat PreviousOwner,
    Seat NewOwner,
    string? ClientRequestId)
    : GameEvent(Sequence, Snapshot, ClientRequestId, GameEventTypes.CardCaptured);

public sealed record TurnChangedEvent(
    long Sequence,
    MatchSnapshot Snapshot,
    Seat ActiveSeat,
    string? ClientRequestId)
    : GameEvent(Sequence, Snapshot, ClientRequestId, GameEventTypes.TurnChanged);

public sealed record MoveRejectedEvent(
    long Sequence,
    MatchSnapshot Snapshot,
    string Reason,
    string? CardInstanceId,
    int? BoardSlotIndex,
    string? ClientRequestId)
    : GameEvent(Sequence, Snapshot, ClientRequestId, GameEventTypes.MoveRejected);

public sealed record MatchEndedEvent(
    long Sequence,
    MatchSnapshot Snapshot,
    Seat? Winner,
    string? ClientRequestId)
    : GameEvent(Sequence, Snapshot, ClientRequestId, GameEventTypes.MatchEnded);
