namespace TripleTriad.Contracts;

public static class NetworkMessageTypes
{
    public const string LobbyJoinRequested = "lobby_join_requested";
    public const string LobbyRulesChangeRequested = "lobby_rules_change_requested";
    public const string LobbyRulesChanged = "lobby_rules_changed";
    public const string LobbyReadyChangeRequested = "lobby_ready_change_requested";
    public const string LobbyReadyChanged = "lobby_ready_changed";
    public const string LobbyCardSelectionChangeRequested = "lobby_card_selection_change_requested";
    public const string LobbySnapshot = "lobby_snapshot";
    public const string MatchStarted = "match_started";
    public const string GameCommand = "game_command";
    public const string GameEvent = "game_event";
}

public sealed record NetworkMessage(string Type, object Payload)
{
    public static NetworkMessage Create(object payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return payload switch
        {
            LobbyJoinRequested => new(NetworkMessageTypes.LobbyJoinRequested, payload),
            LobbyRulesChangeRequested => new(NetworkMessageTypes.LobbyRulesChangeRequested, payload),
            LobbyRulesChanged => new(NetworkMessageTypes.LobbyRulesChanged, payload),
            LobbyReadyChangeRequested => new(NetworkMessageTypes.LobbyReadyChangeRequested, payload),
            LobbyReadyChanged => new(NetworkMessageTypes.LobbyReadyChanged, payload),
            LobbyCardSelectionChangeRequested => new(NetworkMessageTypes.LobbyCardSelectionChangeRequested, payload),
            LobbySnapshot => new(NetworkMessageTypes.LobbySnapshot, payload),
            MatchSetup => new(NetworkMessageTypes.MatchStarted, payload),
            GameCommand => new(NetworkMessageTypes.GameCommand, payload),
            GameEvent => new(NetworkMessageTypes.GameEvent, payload),
            _ => throw new ArgumentOutOfRangeException(
                nameof(payload),
                payload,
                $"Unsupported network payload type: {payload.GetType().Name}"),
        };
    }
}

public sealed record LobbyJoinRequested(string PlayerName);

public sealed record LobbyRulesChangeRequested(GameRules Rules);

public sealed record LobbyRulesChanged(GameRules Rules);

public sealed record LobbyReadyChangeRequested(bool IsReady);

public sealed record LobbyReadyChanged(Seat Seat, bool IsReady);

public sealed record LobbyCardSelectionChangeRequested(IReadOnlyList<int> CardNumbers);
