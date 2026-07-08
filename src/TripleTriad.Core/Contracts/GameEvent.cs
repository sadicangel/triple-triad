namespace TripleTriad.Contracts;

public abstract record GameEvent(string? ClientRequestId);

public sealed record MatchStartedEvent(Seat StartingSeat, MatchSnapshot Snapshot)
    : GameEvent((string?)null);

public sealed record CardPlayedEvent(
    string CardInstanceId,
    CardSnapshot Card,
    Seat SourceSeat,
    int SourceHandIndex,
    int BoardSlotIndex,
    Seat Seat,
    string? ClientRequestId)
    : GameEvent(ClientRequestId);

public sealed record CardCapturedEvent(
    string CardInstanceId,
    CardSnapshot Card,
    int BoardSlotIndex,
    Seat PreviousOwner,
    Seat NewOwner,
    string? ClientRequestId)
    : GameEvent(ClientRequestId);

public sealed record TurnChangedEvent(
    Seat ActiveSeat,
    string? ClientRequestId)
    : GameEvent(ClientRequestId);

public sealed record MoveRejectedEvent(
    string Reason,
    string? CardInstanceId,
    int? BoardSlotIndex,
    string? ClientRequestId)
    : GameEvent(ClientRequestId);

public sealed record MatchEndedEvent(
    Seat? Winner,
    string? ClientRequestId)
    : GameEvent(ClientRequestId);
