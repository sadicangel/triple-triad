using TripleTriad.Data;

namespace TripleTriad.Bridge;

public sealed record CardSnapshot(
    string CardInstanceId,
    int CardNumber,
    string Name,
    Element Element,
    CardRanks Ranks,
    Seat Owner,
    bool IsFaceUp,
    bool IsPlayable);

public sealed record BoardCellSnapshot(
    int Index,
    Element Element,
    CardSnapshot? Card,
    bool CanDrop);

public sealed record HandSnapshot(
    Seat Seat,
    bool IsLocal,
    bool IsRevealed,
    IReadOnlyList<CardSnapshot> Cards);

public sealed record MatchSnapshot(
    Seat ActiveSeat,
    Seat LocalSeat,
    IReadOnlyList<string> Rules,
    int BlueScore,
    int RedScore,
    IReadOnlyList<BoardCellSnapshot> Board,
    IReadOnlyList<HandSnapshot> Hands,
    bool IsComplete);
