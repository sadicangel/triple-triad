using System.Collections.Concurrent;
using Godot;
using TripleTriad.Contracts;
using TripleTriad.Data;
using TripleTriad.Lobby;
using TripleTriad.Mock;
using GodotArray = Godot.Collections.Array;
using GodotDictionary = Godot.Collections.Dictionary;

namespace TripleTriad.Bridge;

public partial class GameFlowBridge : Node
{
    private readonly ConcurrentQueue<LobbyUpdate> _pendingLobbyUpdates = new();
    private CancellationTokenSource? _lobbyLifetime;
    private IGameSession? _activeGameSession;
    private bool _isExiting;
    private ILobbySession? _lobby;
    private LocalLobbyMode? _lobbyMode;
    private CardCatalog? _cardCatalog;

    [Signal] public delegate void lobby_snapshot_changedEventHandler(GodotDictionary snapshot);

    [Signal] public delegate void match_start_failedEventHandler(string reason);

    public GodotDictionary CurrentLobbySnapshot { get; private set; } = [];

    public override void _ExitTree()
    {
        _isExiting = true;
        _lobbyLifetime?.Cancel();
        _lobbyLifetime?.Dispose();
    }

    public void start_solo_lobby() =>
        StartLocalLobby(LocalLobbyMode.Solo);

    public void start_host_lobby() =>
        StartLocalLobby(LocalLobbyMode.Host);

    public GodotDictionary get_lobby_snapshot() => CurrentLobbySnapshot;

    public GodotArray get_lobby_card_catalog()
    {
        var cards = new GodotArray();
        var owner = _lobby?.CurrentSnapshot.LocalSeat ?? Seat.Blue;
        foreach (var card in GetCardCatalog().Cards)
            cards.Add(Serialize(card, owner));

        return cards;
    }

    public GodotArray get_rule_options()
    {
        var rules = _lobby?.CurrentSnapshot.Rules ?? GameRules.Default;
        var options = new GodotArray();

        foreach (var rule in GameRulesExtensions.SelectableRules)
        {
            options.Add(new GodotDictionary
            {
                ["name"] = rule.ToDisplayName(),
                ["enabled"] = rules.Contains(rule),
            });
        }

        return options;
    }

    public void set_rule_enabled(string ruleName, bool enabled)
    {
        if (_lobby is null || !Enum.TryParse(ruleName, ignoreCase: true, out GameRules rule))
            return;

        var rules = _lobby.CurrentSnapshot.Rules;
        rules = enabled ? rules | rule : rules & ~rule;
        _ = SetRulesAsync(rules);
    }

    public void set_lobby_card_selection(GodotArray cardNumbers)
    {
        try
        {
            var numbers = new List<int>(cardNumbers.Count);
            for (var index = 0; index < cardNumbers.Count; index++)
                numbers.Add(cardNumbers[index].AsInt32());

            _ = SetSelectedCardsAsync(numbers);
        }
        catch (Exception ex)
        {
            EmitSignal(SignalName.match_start_failed, ex.Message);
        }
    }

    public void take_seat(string seatName)
    {
        if (_lobby is null || !Enum.TryParse(seatName, ignoreCase: true, out Seat seat))
            return;

        _ = TakeSeatAsync(seat);
    }

    public void start_match()
    {
        if (_lobby is null || !_lobby.CurrentSnapshot.CanStart)
            return;

        _ = StartMatchAsync();
    }

    public IGameSession? GetActiveGameSession() => _activeGameSession;

    private void StartLocalLobby(LocalLobbyMode mode)
    {
        _lobbyLifetime?.Cancel();
        _lobbyLifetime?.Dispose();
        _lobbyLifetime = new CancellationTokenSource();
        _pendingLobbyUpdates.Clear();
        _activeGameSession = null;
        _lobbyMode = mode;
        _lobby = new LocalLobbySession(mode);
        _ = PumpLobbyUpdatesAsync(_lobby, _lobbyLifetime.Token);

        try
        {
            var snapshot = _lobby.StartAsync(_lobbyLifetime.Token).AsTask().GetAwaiter().GetResult();
            ApplyLobbySnapshot(snapshot);
        }
        catch (Exception ex)
        {
            EmitSignal(SignalName.match_start_failed, ex.Message);
        }
    }

    private async Task SetRulesAsync(GameRules rules)
    {
        if (_lobby is null || _lobbyLifetime is null)
            return;

        try
        {
            await _lobby.SetRulesAsync(rules, _lobbyLifetime.Token);
        }
        catch (Exception ex)
        {
            EmitSignal(SignalName.match_start_failed, ex.Message);
        }
    }

    private async Task SetSelectedCardsAsync(IReadOnlyList<int> cardNumbers)
    {
        if (_lobby is null || _lobbyLifetime is null)
            return;

        try
        {
            var normalized = LobbyCardSelectionRules.Validate(cardNumbers);
            foreach (var cardNumber in normalized)
                GetCardCatalog().Get(cardNumber);

            await _lobby.SetSelectedCardsAsync(normalized, _lobbyLifetime.Token);
        }
        catch (Exception ex)
        {
            EmitSignal(SignalName.match_start_failed, ex.Message);
        }
    }

    private async Task TakeSeatAsync(Seat seat)
    {
        if (_lobby is null || _lobbyLifetime is null)
            return;

        try
        {
            await _lobby.TakeSeatAsync(seat, _lobbyLifetime.Token);
        }
        catch (Exception ex)
        {
            EmitSignal(SignalName.match_start_failed, ex.Message);
        }
    }

    private async Task StartMatchAsync()
    {
        if (_lobby is null || _lobbyLifetime is null)
            return;

        try
        {
            await _lobby.SetReadyAsync(true, _lobbyLifetime.Token);
            var setup = await _lobby.WaitForMatchStartAsync(_lobbyLifetime.Token);
            var snapshot = _lobby.CurrentSnapshot;
            var localSeat = snapshot.LocalSeat;
            var hasAiOpponent = setup.Players.Any(player => player.Seat != localSeat && player.Kind == LobbyPlayerKind.AI);
            var catalog = GetCardCatalog();
            _activeGameSession = new MockGameSession(
                catalog,
                localSeat,
                setup.Rules.Contains(GameRules.Open),
                hasAiOpponent,
                setup.Rules,
                CreateSelectedHands(setup));

            CallDeferred(nameof(ChangeToGameScene));
        }
        catch (Exception ex)
        {
            EmitSignal(SignalName.match_start_failed, ex.Message);
        }
    }

    private void ChangeToGameScene() =>
        GetTree().ChangeSceneToFile("res://scenes/GameScene.tscn");

    private async Task PumpLobbyUpdatesAsync(ILobbySession lobby, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var update in lobby.ReadUpdatesAsync(cancellationToken))
                EnqueueLobbyUpdate(update);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex)
        {
            EmitSignal(SignalName.match_start_failed, ex.Message);
        }
    }

    private void EnqueueLobbyUpdate(LobbyUpdate update)
    {
        if (_isExiting)
            return;

        _pendingLobbyUpdates.Enqueue(update);
        CallDeferred(nameof(DrainLobbyUpdates));
    }

    public void DrainLobbyUpdates()
    {
        while (_pendingLobbyUpdates.TryDequeue(out var update))
        {
            if (update is LobbySnapshotUpdate snapshotUpdate)
                ApplyLobbySnapshot(snapshotUpdate.Snapshot);
        }
    }

    private void ApplyLobbySnapshot(LobbySnapshot snapshot)
    {
        CurrentLobbySnapshot = Serialize(snapshot);
        EmitSignal(SignalName.lobby_snapshot_changed, CurrentLobbySnapshot);
    }

    private GodotDictionary Serialize(LobbySnapshot snapshot)
    {
        var seats = new GodotArray
        {
            SerializeSeat(snapshot, Seat.Blue),
            SerializeSeat(snapshot, Seat.Red),
        };

        var rules = new GodotArray();
        foreach (var rule in snapshot.Rules.ToDisplayNames())
            rules.Add(rule);

        return new GodotDictionary
        {
            ["has_lobby"] = true,
            ["mode"] = _lobbyMode?.ToString() ?? string.Empty,
            ["local_seat"] = snapshot.LocalSeat.ToString(),
            ["rules"] = rules,
            ["can_start"] = snapshot.CanStart && !snapshot.IsMatchStarting,
            ["can_select_cards"] = CanSelectCards(snapshot),
            ["selected_cards"] = SerializeSelectedCards(snapshot),
            ["is_match_starting"] = snapshot.IsMatchStarting,
            ["status"] = FormatStatus(snapshot),
            ["seats"] = seats,
        };
    }

    private GodotArray SerializeSelectedCards(LobbySnapshot snapshot)
    {
        var cards = new GodotArray();
        var selection = snapshot.CardSelections.FirstOrDefault(candidate => candidate.Seat == snapshot.LocalSeat);
        if (selection is null)
            return cards;

        foreach (var cardNumber in selection.CardNumbers)
            cards.Add(Serialize(GetCardCatalog().Get(cardNumber), snapshot.LocalSeat));

        return cards;
    }

    private GodotDictionary SerializeSeat(LobbySnapshot snapshot, Seat seat)
    {
        var player = snapshot.Players.FirstOrDefault(candidate => candidate.Seat == seat);
        var serialized = new GodotDictionary
        {
            ["seat"] = seat.ToString(),
            ["is_local"] = seat == snapshot.LocalSeat,
            ["occupied"] = player is not null,
            ["can_take"] = CanTakeSeat(snapshot, seat),
        };

        if (player is null)
        {
            serialized["name"] = "OPEN";
            serialized["kind"] = "Empty";
            serialized["ready"] = false;
            serialized["connected"] = false;
            return serialized;
        }

        serialized["name"] = player.PlayerName;
        serialized["kind"] = player.Kind.ToString();
        serialized["ready"] = player.IsReady;
        serialized["connected"] = player.IsConnected;
        return serialized;
    }

    private static bool CanTakeSeat(LobbySnapshot snapshot, Seat seat)
    {
        if (snapshot.IsMatchStarting || snapshot.LocalSeat == seat)
            return false;

        var currentPlayer = snapshot.Players.FirstOrDefault(player => player.Seat == snapshot.LocalSeat);
        if (currentPlayer is not { Kind: LobbyPlayerKind.Human })
            return false;

        var targetPlayer = snapshot.Players.FirstOrDefault(player => player.Seat == seat);
        return targetPlayer is null || targetPlayer.Kind == LobbyPlayerKind.AI;
    }

    private static bool CanSelectCards(LobbySnapshot snapshot)
    {
        if (snapshot.IsMatchStarting || snapshot.Rules.Contains(GameRules.Random))
            return false;

        var localPlayer = snapshot.Players.FirstOrDefault(player => player.Seat == snapshot.LocalSeat);
        return localPlayer is { Kind: LobbyPlayerKind.Human, IsConnected: true };
    }

    private static string FormatStatus(LobbySnapshot snapshot)
    {
        if (snapshot.IsMatchStarting)
            return "STARTING";

        if (snapshot.CanStart)
            return "READY";

        return "WAITING FOR PLAYER";
    }

    private CardCatalog GetCardCatalog() =>
        _cardCatalog ??= CardCatalog.Load(ProjectSettings.GlobalizePath("res://assets/triple_triad/cards.json"));

    private static IReadOnlyDictionary<Seat, IReadOnlyList<int>> CreateSelectedHands(MatchSetup setup) =>
        setup.CardSelections.ToDictionary(
            selection => selection.Seat,
            selection => (IReadOnlyList<int>)selection.CardNumbers.ToArray());

    private static GodotDictionary Serialize(CardDefinition card, Seat owner) =>
        new()
        {
            ["id"] = $"catalog-{card.Number}",
            ["number"] = card.Number,
            ["name"] = card.Name,
            ["element"] = card.Element.ToString(),
            ["owner"] = owner.ToString(),
            ["face_up"] = true,
            ["playable"] = false,
            ["w"] = card.Ranks.West,
            ["n"] = card.Ranks.North,
            ["e"] = card.Ranks.East,
            ["s"] = card.Ranks.South,
        };
}
