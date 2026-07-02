using Godot;
using TripleTriad.Data;
using TripleTriad.Mock;
using GodotArray = Godot.Collections.Array;
using GodotDictionary = Godot.Collections.Dictionary;

namespace TripleTriad.Bridge;

public partial class GameSessionBridge : Node
{
    private IGameSession _session = null!;

    [Signal]
    public delegate void snapshot_changedEventHandler(GodotDictionary snapshot);

    [Signal]
    public delegate void game_event_raisedEventHandler(GodotDictionary gameEvent);

    [Export]
    public bool RevealOpponentHand { get; set; }

    [Export]
    public bool AutoPlayOpponent { get; set; } = true;

    public GodotDictionary CurrentSnapshot { get; private set; } = [];

    public override void _Ready()
    {
        var catalog = CardCatalog.Load(ProjectSettings.GlobalizePath("res://assets/triple_triad/cards.json"));
        _session = new MockGameSession(
            catalog,
            Seat.Blue,
            RevealOpponentHand,
            AutoPlayOpponent);
        _session.SnapshotChanged += HandleSnapshotChanged;
        _session.EventRaised += HandleGameEventRaised;
        CurrentSnapshot = Serialize(_session.CurrentSnapshot);
    }

    public override void _ExitTree()
    {
        if (_session is null)
            return;

        _session.SnapshotChanged -= HandleSnapshotChanged;
        _session.EventRaised -= HandleGameEventRaised;
    }

    public GodotDictionary get_current_snapshot() => CurrentSnapshot;

    public void submit_play_card(string cardInstanceId, int boardSlotIndex, string clientRequestId)
    {
        if (_session is null)
            return;

        _ = _session.SubmitAsync(new PlayCardCommand(
            cardInstanceId,
            boardSlotIndex,
            clientRequestId));
    }

    private void HandleSnapshotChanged(MatchSnapshot snapshot)
    {
        CurrentSnapshot = Serialize(snapshot);
        EmitSignal(SignalName.snapshot_changed, CurrentSnapshot);
    }

    private void HandleGameEventRaised(GameEvent gameEvent) =>
        EmitSignal(SignalName.game_event_raised, Serialize(gameEvent));

    private static GodotDictionary Serialize(MatchSnapshot snapshot)
    {
        var board = new GodotArray();
        foreach (var cell in snapshot.Board)
            board.Add(Serialize(cell));

        var hands = new GodotArray();
        foreach (var hand in snapshot.Hands)
            hands.Add(Serialize(hand));

        var rules = new GodotArray();
        foreach (var rule in snapshot.Rules)
            rules.Add(rule);

        return new GodotDictionary
        {
            ["active_seat"] = snapshot.ActiveSeat.ToString(),
            ["local_seat"] = snapshot.LocalSeat.ToString(),
            ["rules"] = rules,
            ["blue_score"] = snapshot.BlueScore,
            ["red_score"] = snapshot.RedScore,
            ["board"] = board,
            ["hands"] = hands,
            ["is_complete"] = snapshot.IsComplete,
        };
    }

    private static GodotDictionary Serialize(BoardCellSnapshot cell)
    {
        var serialized = new GodotDictionary
        {
            ["index"] = cell.Index,
            ["element"] = cell.Element.ToString(),
            ["can_drop"] = cell.CanDrop,
            ["has_card"] = cell.Card is not null,
        };

        if (cell.Card is not null)
            serialized["card"] = Serialize(cell.Card);

        return serialized;
    }

    private static GodotDictionary Serialize(HandSnapshot hand)
    {
        var cards = new GodotArray();
        foreach (var card in hand.Cards)
            cards.Add(Serialize(card));

        return new GodotDictionary
        {
            ["seat"] = hand.Seat.ToString(),
            ["is_local"] = hand.IsLocal,
            ["is_revealed"] = hand.IsRevealed,
            ["cards"] = cards,
        };
    }

    private static GodotDictionary Serialize(CardSnapshot card) =>
        new()
        {
            ["id"] = card.CardInstanceId,
            ["number"] = card.CardNumber,
            ["name"] = card.Name,
            ["element"] = card.Element.ToString(),
            ["owner"] = card.Owner.ToString(),
            ["face_up"] = card.IsFaceUp,
            ["playable"] = card.IsPlayable,
            ["w"] = card.Ranks.West,
            ["n"] = card.Ranks.North,
            ["e"] = card.Ranks.East,
            ["s"] = card.Ranks.South,
        };

    private static GodotDictionary Serialize(GameEvent gameEvent)
    {
        var serialized = new GodotDictionary
        {
            ["client_request_id"] = gameEvent.ClientRequestId ?? string.Empty,
        };

        switch (gameEvent)
        {
            case MatchStartedEvent started:
                serialized["type"] = "match_started";
                serialized["snapshot"] = Serialize(started.Snapshot);
                break;
            case CardPlayedEvent played:
                serialized["type"] = "card_played";
                serialized["card_id"] = played.CardInstanceId;
                serialized["board_slot_index"] = played.BoardSlotIndex;
                serialized["seat"] = played.Seat.ToString();
                break;
            case CardCapturedEvent captured:
                serialized["type"] = "card_captured";
                serialized["card_id"] = captured.CardInstanceId;
                serialized["board_slot_index"] = captured.BoardSlotIndex;
                serialized["new_owner"] = captured.NewOwner.ToString();
                break;
            case TurnChangedEvent turnChanged:
                serialized["type"] = "turn_changed";
                serialized["active_seat"] = turnChanged.ActiveSeat.ToString();
                break;
            case MoveRejectedEvent rejected:
                serialized["type"] = "move_rejected";
                serialized["reason"] = rejected.Reason;
                serialized["card_id"] = rejected.CardInstanceId ?? string.Empty;
                serialized["board_slot_index"] = rejected.BoardSlotIndex ?? -1;
                break;
            case MatchEndedEvent ended:
                serialized["type"] = "match_ended";
                serialized["winner"] = ended.Winner?.ToString() ?? string.Empty;
                break;
        }

        return serialized;
    }
}
