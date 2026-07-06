namespace TripleTriad.Contracts;

public abstract record NetworkMessage;

public abstract record LobbyNetworkMessage : NetworkMessage;

public sealed record LobbyJoinRequestedNetworkMessage(
    string PlayerName) : LobbyNetworkMessage;

public sealed record LobbyRulesChangeRequestedNetworkMessage(
    GameRules Rules) : LobbyNetworkMessage;

public sealed record LobbyRulesChangedNetworkMessage(
    GameRules Rules) : LobbyNetworkMessage;

public sealed record LobbyReadyChangeRequestedNetworkMessage(
    bool IsReady) : LobbyNetworkMessage;

public sealed record LobbyReadyChangedNetworkMessage(
    Seat Seat,
    bool IsReady) : LobbyNetworkMessage;

public sealed record LobbySnapshotNetworkMessage(
    LobbySnapshot Snapshot) : LobbyNetworkMessage;

public sealed record MatchStartedNetworkMessage(
    MatchSetup Setup) : LobbyNetworkMessage;

public abstract record GameNetworkMessage : NetworkMessage;

public sealed record GameCommandNetworkMessage(
    GameCommand Command) : GameNetworkMessage;

public sealed record GameUpdateNetworkMessage(
    GameSessionUpdate Update) : GameNetworkMessage;
