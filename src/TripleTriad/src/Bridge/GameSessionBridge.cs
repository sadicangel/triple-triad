using System.Collections.Concurrent;
using Godot;
using TripleTriad.Contracts;
using TripleTriad.Data;
using TripleTriad.Mock;
using GodotArray = Godot.Collections.Array;
using GodotDictionary = Godot.Collections.Dictionary;

namespace TripleTriad.Bridge;

public partial class GameSessionBridge : Node
{
    private readonly ConcurrentQueue<GameSessionUpdate> _pendingUpdates = new();
    private readonly CancellationTokenSource _sessionLifetime = new();
    private bool _isExiting;
    private IGameSession _session = null!;

    [Signal] public delegate void snapshot_changedEventHandler(GodotDictionary snapshot);

    [Signal] public delegate void game_event_raisedEventHandler(GodotDictionary gameEvent);

    [Signal] public delegate void connection_state_changedEventHandler(GodotDictionary connectionState);

    [Export] public bool RevealOpponentHand { get; set; } = true;

    [Export] public bool AutoPlayOpponent { get; set; } = true;

    public GodotDictionary CurrentSnapshot { get; private set; } = [];

    public override async void _Ready()
    {
        var catalog = CardCatalog.Load(ProjectSettings.GlobalizePath("res://assets/triple_triad/cards.json"));
        _session = new MockGameSession(
            catalog,
            Seat.Blue,
            RevealOpponentHand,
            AutoPlayOpponent);
        _ = PumpSessionUpdatesAsync(_sessionLifetime.Token);

        try
        {
            var snapshot = await _session.StartAsync(_sessionLifetime.Token);
            if (CurrentSnapshot.Count == 0)
                CurrentSnapshot = Serialize(snapshot);
        }
        catch (OperationCanceledException) when (_sessionLifetime.IsCancellationRequested) { }
        catch (Exception ex)
        {
            EnqueueSessionUpdate(new GameSessionConnectionStateUpdate(0, SessionConnectionState.Failed, ex.Message));
        }
    }

    public override void _ExitTree()
    {
        _isExiting = true;
        _sessionLifetime.Cancel();

        _sessionLifetime.Dispose();
    }

    public GodotDictionary get_current_snapshot() => CurrentSnapshot;

    public void submit_play_card(string cardInstanceId, int boardSlotIndex, string clientRequestId)
    {
        if (_session is null)
            return;

        _ = SendPlayCardAsync(cardInstanceId, boardSlotIndex, clientRequestId);
    }

    private async Task SendPlayCardAsync(string cardInstanceId, int boardSlotIndex, string clientRequestId)
    {
        try
        {
            await _session.SendCommandAsync(
                new PlayCardCommand(
                    cardInstanceId,
                    boardSlotIndex,
                    clientRequestId),
                _sessionLifetime.Token);
        }
        catch (OperationCanceledException) when (_sessionLifetime.IsCancellationRequested) { }
        catch (Exception ex)
        {
            EnqueueSessionUpdate(new GameSessionConnectionStateUpdate(0, SessionConnectionState.Failed, ex.Message));
        }
    }

    private async Task PumpSessionUpdatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var update in _session.ReadUpdatesAsync(cancellationToken))
                EnqueueSessionUpdate(update);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex)
        {
            EnqueueSessionUpdate(new GameSessionConnectionStateUpdate(0, SessionConnectionState.Failed, ex.Message));
        }
    }

    private void EnqueueSessionUpdate(GameSessionUpdate update)
    {
        if (_isExiting)
            return;

        _pendingUpdates.Enqueue(update);

        CallDeferred(nameof(DrainSessionUpdates));
    }

    public void DrainSessionUpdates()
    {
        while (_pendingUpdates.TryDequeue(out var update))
            ApplySessionUpdate(update);
    }

    private void ApplySessionUpdate(GameSessionUpdate update)
    {
        switch (update)
        {
            case GameSessionSnapshotUpdate snapshotUpdate:
                CurrentSnapshot = Serialize(snapshotUpdate.Snapshot);
                CurrentSnapshot["sequence"] = snapshotUpdate.Sequence;
                EmitSignal(SignalName.snapshot_changed, CurrentSnapshot);
                break;
            case GameSessionEventUpdate eventUpdate:
                var serializedEvent = Serialize(eventUpdate.GameEvent);
                serializedEvent["sequence"] = eventUpdate.Sequence;
                EmitSignal(SignalName.game_event_raised, serializedEvent);
                break;
            case GameSessionConnectionStateUpdate stateUpdate:
                EmitConnectionState(stateUpdate.State, stateUpdate.Reason, stateUpdate.Sequence);
                break;
        }
    }

    private void EmitConnectionState(SessionConnectionState state, string? reason, long sequence)
    {
        var serialized = new GodotDictionary
        {
            ["state"] = state.ToString(),
            ["reason"] = reason ?? string.Empty,
            ["sequence"] = sequence,
        };

        EmitSignal(SignalName.connection_state_changed, serialized);
    }

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
                serialized["card"] = Serialize(played.Card);
                serialized["source_seat"] = played.SourceSeat.ToString();
                serialized["source_hand_index"] = played.SourceHandIndex;
                serialized["board_slot_index"] = played.BoardSlotIndex;
                serialized["seat"] = played.Seat.ToString();
                break;
            case CardCapturedEvent captured:
                serialized["type"] = "card_captured";
                serialized["card_id"] = captured.CardInstanceId;
                serialized["card"] = Serialize(captured.Card);
                serialized["board_slot_index"] = captured.BoardSlotIndex;
                serialized["previous_owner"] = captured.PreviousOwner.ToString();
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
