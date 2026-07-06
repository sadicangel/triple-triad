namespace TripleTriad.Contracts;

public sealed record LobbyPlayerSnapshot(
    Seat Seat,
    string PlayerName,
    bool IsReady,
    bool IsConnected = true);

public sealed record LobbySnapshot(
    Seat LocalSeat,
    GameRules Rules,
    IReadOnlyList<LobbyPlayerSnapshot> Players,
    bool CanStart,
    bool IsMatchStarting);

public sealed record MatchSetup(
    GameRules Rules,
    IReadOnlyList<LobbyPlayerSnapshot> Players);
