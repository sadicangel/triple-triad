using System.Threading.Channels;
using System.Collections.Concurrent;
using TripleTriad.Contracts;
using TripleTriad.Data;

namespace TripleTriad.Sessions;

public sealed class LocalGameSession : IGameSession
{
    private readonly CardState?[] _board = new CardState?[9];
    private readonly CardCatalog _catalog;
    private readonly Dictionary<Seat, List<CardState>> _hands;
    private readonly Seat _localSeat;
    private readonly bool _revealOpponentHand;
    private readonly ConcurrentDictionary<long, ChannelWriter<GameSessionUpdate>> _subscribers = [];
    private readonly SemaphoreSlim _stateGate = new(1, 1);
    private long _nextSequence;
    private long _nextSubscriberId;
    private bool _isComplete;
    private Seat _activeSeat;

    public LocalGameSession(
        CardCatalog catalog,
        Seat localSeat = Seat.Blue,
        bool revealOpponentHand = false,
        GameRules rules = GameRules.Default,
        IReadOnlyDictionary<Seat, IReadOnlyList<int>>? selectedCardNumbers = null)
    {
        _catalog = catalog;
        _localSeat = localSeat;
        _revealOpponentHand = revealOpponentHand;
        Rules = revealOpponentHand ? rules | GameRules.Open : rules;
        _activeSeat = Seat.Blue;
        _hands = new Dictionary<Seat, List<CardState>>
        {
            [Seat.Red] = CreateHand(
                Seat.Red,
                ResolveHand(Seat.Red, selectedCardNumbers, [101, 103, 105, 107, 109])),
            [Seat.Blue] = CreateHand(
                Seat.Blue,
                ResolveHand(Seat.Blue, selectedCardNumbers, [102, 104, 106, 108, 110])),
        };
    }

    public MatchSnapshot? CurrentSnapshot { get; private set; }

    public GameRules Rules { get; }

    public SessionConnectionState ConnectionState { get; private set; } = SessionConnectionState.NotStarted;

    public IAsyncEnumerable<GameSessionUpdate> SubscribeUpdatesAsync(
        CancellationToken cancellationToken = default) =>
        new GameSessionUpdateSubscription(this, cancellationToken);

    public async ValueTask<MatchSnapshot> StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _stateGate.WaitAsync(cancellationToken);

        try
        {
            if (CurrentSnapshot is not null && ConnectionState == SessionConnectionState.Connected)
                return CurrentSnapshot;

            PublishConnectionState(SessionConnectionState.Connecting);
            var snapshot = BuildSnapshot();
            PublishSnapshot(snapshot);
            PublishConnectionState(SessionConnectionState.Connected);
            PublishEvent(new MatchStartedEvent(snapshot.ActiveSeat, snapshot));

            return snapshot;
        }
        finally
        {
            _stateGate.Release();
        }
    }

    public async ValueTask SendCommandAsync(
        GameCommand command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _stateGate.WaitAsync(cancellationToken);

        try
        {
            EnsureCanSubmit();

            switch (command)
            {
                case PlayCardCommand playCard:
                    PlayCard(playCard);
                    break;
                default:
                    RejectUnknownCommand(command);
                    break;
            }
        }
        finally
        {
            _stateGate.Release();
        }
    }

    private int[] ResolveHand(
        Seat seat,
        IReadOnlyDictionary<Seat, IReadOnlyList<int>>? selectedCardNumbers,
        int[] fallbackCardNumbers)
    {
        if (Rules.Contains(GameRules.Random))
            return CreateRandomHandNumbers();

        if (selectedCardNumbers is null)
            return fallbackCardNumbers;

        return selectedCardNumbers.TryGetValue(seat, out var cardNumbers)
            ? ValidateHand(cardNumbers)
            : CreateRandomHandNumbers();
    }

    private int[] ValidateHand(IReadOnlyList<int> cardNumbers)
    {
        var normalized = LobbyCardSelectionRules.Validate(cardNumbers);
        foreach (var cardNumber in normalized)
            _catalog.Get(cardNumber);

        return normalized;
    }

    private int[] CreateRandomHandNumbers()
    {
        if (_catalog.Cards.Count < LobbyCardSelectionRules.HandSize)
            throw new InvalidOperationException("The card catalog does not contain enough cards to create a hand.");

        return _catalog.Cards
            .OrderBy(_ => Random.Shared.Next())
            .Take(LobbyCardSelectionRules.HandSize)
            .Select(card => card.Number)
            .ToArray();
    }

    private List<CardState> CreateHand(Seat seat, IReadOnlyList<int> cardNumbers) =>
        cardNumbers
            .Select(number => new CardState($"{seat.ToString().ToLowerInvariant()}-{number}", _catalog.Get(number), seat))
            .ToList();

    private void PlayCard(PlayCardCommand command)
    {
        if (_isComplete)
        {
            Reject("The match is already complete.", command);
            return;
        }

        if (command.BoardSlotIndex is < 0 or > 8)
        {
            Reject("That board slot does not exist.", command);
            return;
        }

        if (_board[command.BoardSlotIndex] is not null)
        {
            Reject("That board slot is already occupied.", command);
            return;
        }

        var card = FindCardInHands(command.CardInstanceId, out var handSeat, out var hand);
        if (card is null || hand is null)
        {
            Reject("That card is not in a hand.", command);
            return;
        }

        if (handSeat != _activeSeat)
        {
            Reject($"It is {_activeSeat}'s turn.", command);
            return;
        }

        var sourceHandIndex = hand.IndexOf(card);
        hand.Remove(card);
        _board[command.BoardSlotIndex] = card;

        var capturedCards = CaptureAdjacentCards(command.BoardSlotIndex, card);
        _isComplete = IsBoardFull();
        if (!_isComplete)
            _activeSeat = _activeSeat.Opponent();

        var snapshot = BuildSnapshot();
        var playedEvent = new CardPlayedEvent(
            card.InstanceId,
            ToSnapshot(card, isFaceUp: true, isPlayable: false),
            handSeat,
            sourceHandIndex,
            command.BoardSlotIndex,
            card.Owner,
            command.ClientRequestId);

        PublishEvent(playedEvent);

        foreach (var captured in capturedCards)
        {
            PublishEvent(
                new CardCapturedEvent(
                    captured.Card.InstanceId,
                    ToSnapshot(captured.Card, isFaceUp: true, isPlayable: false),
                    captured.BoardSlotIndex,
                    captured.PreviousOwner,
                    captured.Card.Owner,
                    command.ClientRequestId));
        }

        if (_isComplete)
        {
            PublishEvent(new MatchEndedEvent(GetWinner(snapshot), command.ClientRequestId));
            PublishSnapshot(snapshot);
            return;
        }

        PublishEvent(new TurnChangedEvent(_activeSeat, command.ClientRequestId));
        PublishSnapshot(snapshot);
    }

    private List<CapturedCard> CaptureAdjacentCards(int boardSlotIndex, CardState playedCard)
    {
        var captured = new List<CapturedCard>();
        var row = boardSlotIndex / 3;
        var column = boardSlotIndex % 3;

        TryCapture(row - 1, column, Direction.North, Direction.South);
        TryCapture(row + 1, column, Direction.South, Direction.North);
        TryCapture(row, column - 1, Direction.West, Direction.East);
        TryCapture(row, column + 1, Direction.East, Direction.West);

        return captured;

        void TryCapture(int neighborRow, int neighborColumn, Direction attackSide, Direction defenseSide)
        {
            if (neighborRow is < 0 or > 2 || neighborColumn is < 0 or > 2)
                return;

            var neighborIndex = neighborRow * 3 + neighborColumn;
            var neighbor = _board[neighborIndex];
            if (neighbor is null || neighbor.Owner == playedCard.Owner)
                return;

            var attackRank = playedCard.Definition.Ranks.Get(attackSide);
            var defenseRank = neighbor.Definition.Ranks.Get(defenseSide);
            if (attackRank <= defenseRank)
                return;

            var previousOwner = neighbor.Owner;
            neighbor.Owner = playedCard.Owner;
            captured.Add(new CapturedCard(neighborIndex, neighbor, previousOwner));
        }
    }

    private CardState? FindCardInHands(
        string cardInstanceId,
        out Seat seat,
        out List<CardState>? hand)
    {
        foreach (var pair in _hands)
        {
            var card = pair.Value.FirstOrDefault(candidate => candidate.InstanceId == cardInstanceId);
            if (card is null)
                continue;

            seat = pair.Key;
            hand = pair.Value;
            return card;
        }

        seat = default;
        hand = null;
        return null;
    }

    private MatchSnapshot BuildSnapshot()
    {
        var board = _board
            .Select((card, index) => new BoardCellSnapshot(
                index,
                Element.None,
                card is null ? null : ToSnapshot(card, isFaceUp: true, isPlayable: false),
                CanDropOn(index)))
            .ToArray();

        var hands = new[] { BuildHandSnapshot(Seat.Red), BuildHandSnapshot(Seat.Blue), };

        var allCards = _hands.Values.SelectMany(cards => cards).Concat(_board.OfType<CardState>());
        var blueScore = allCards.Count(card => card.Owner == Seat.Blue);
        var redScore = allCards.Count(card => card.Owner == Seat.Red);

        return new MatchSnapshot(
            _activeSeat,
            _localSeat,
            Rules,
            blueScore,
            redScore,
            board,
            hands,
            _isComplete);
    }

    private HandSnapshot BuildHandSnapshot(Seat seat)
    {
        var isLocal = seat == _localSeat;
        var isRevealed = isLocal || _revealOpponentHand;
        var cards = _hands[seat]
            .Select(card => ToSnapshot(
                card,
                isFaceUp: isRevealed,
                isPlayable: !_isComplete && isLocal && seat == _activeSeat))
            .ToArray();

        return new HandSnapshot(seat, isLocal, isRevealed, cards);
    }

    private bool CanDropOn(int boardSlotIndex) =>
        !_isComplete
        && _activeSeat == _localSeat
        && _board[boardSlotIndex] is null;

    private static CardSnapshot ToSnapshot(CardState card, bool isFaceUp, bool isPlayable) => new(card.InstanceId, card.Definition.Number, card.Definition.Name, card.Definition.Element, card.Definition.Ranks, card.Owner, isFaceUp, isPlayable);

    private void PublishSnapshot(MatchSnapshot snapshot)
    {
        CurrentSnapshot = snapshot;
        PublishUpdate(new GameSessionSnapshotUpdate(NextSequence(), snapshot));
    }

    private void PublishEvent(GameEvent gameEvent) =>
        PublishUpdate(new GameSessionEventUpdate(NextSequence(), gameEvent));

    private void PublishConnectionState(SessionConnectionState state, string? reason = null)
    {
        ConnectionState = state;
        PublishUpdate(new GameSessionConnectionStateUpdate(NextSequence(), state, reason));
    }

    private void PublishUpdate(GameSessionUpdate update)
    {
        foreach (var subscriber in _subscribers)
        {
            if (!subscriber.Value.TryWrite(update))
                _subscribers.TryRemove(subscriber.Key, out _);
        }
    }

    private long AddSubscriber(ChannelWriter<GameSessionUpdate> writer)
    {
        var subscriberId = Interlocked.Increment(ref _nextSubscriberId);
        _subscribers[subscriberId] = writer;
        return subscriberId;
    }

    private void RemoveSubscriber(
        long subscriberId,
        ChannelWriter<GameSessionUpdate> writer)
    {
        _subscribers.TryRemove(subscriberId, out _);
        writer.TryComplete();
    }

    private void Reject(string reason, PlayCardCommand command)
    {
        var rejected = new MoveRejectedEvent(
            reason,
            command.CardInstanceId,
            command.BoardSlotIndex,
            command.ClientRequestId);

        PublishEvent(rejected);
    }

    private void RejectUnknownCommand(GameCommand command)
    {
        var rejected = new MoveRejectedEvent(
            "Unknown command.",
            null,
            null,
            command.ClientRequestId);

        PublishEvent(rejected);
    }

    private long NextSequence() => ++_nextSequence;

    private void EnsureCanSubmit()
    {
        if (ConnectionState == SessionConnectionState.NotStarted)
            throw new InvalidOperationException("StartAsync must complete before submitting commands.");

        if (ConnectionState != SessionConnectionState.Connected)
            throw new InvalidOperationException($"Cannot submit commands while the session is {ConnectionState}.");
    }

    private bool IsBoardFull() => _board.All(card => card is not null);

    private static Seat? GetWinner(MatchSnapshot snapshot)
    {
        if (snapshot.BlueScore == snapshot.RedScore)
            return null;

        return snapshot.BlueScore > snapshot.RedScore ? Seat.Blue : Seat.Red;
    }

    private sealed class CardState(string instanceId, CardDefinition definition, Seat owner)
    {
        public string InstanceId { get; } = instanceId;

        public CardDefinition Definition { get; } = definition;

        public Seat Owner { get; set; } = owner;
    }

    private readonly record struct CapturedCard(int BoardSlotIndex, CardState Card, Seat PreviousOwner);

    private sealed class GameSessionUpdateSubscription(
        LocalGameSession session,
        CancellationToken subscriptionCancellationToken) : IAsyncEnumerable<GameSessionUpdate>
    {
        public IAsyncEnumerator<GameSessionUpdate> GetAsyncEnumerator(
            CancellationToken cancellationToken = default)
        {
            var channel = Channel.CreateUnbounded<GameSessionUpdate>();
            var subscriberId = session.AddSubscriber(channel.Writer);
            var linkedCancellation = CreateLinkedCancellation(
                subscriptionCancellationToken,
                cancellationToken);
            var effectiveCancellationToken = linkedCancellation?.Token
                ?? (subscriptionCancellationToken.CanBeCanceled
                    ? subscriptionCancellationToken
                    : cancellationToken);

            return new Enumerator(
                session,
                subscriberId,
                channel,
                effectiveCancellationToken,
                linkedCancellation);
        }

        private static CancellationTokenSource? CreateLinkedCancellation(
            CancellationToken subscriptionCancellationToken,
            CancellationToken enumerationCancellationToken)
        {
            if (!subscriptionCancellationToken.CanBeCanceled)
                return enumerationCancellationToken.CanBeCanceled
                    ? CancellationTokenSource.CreateLinkedTokenSource(enumerationCancellationToken)
                    : null;

            return enumerationCancellationToken.CanBeCanceled
                ? CancellationTokenSource.CreateLinkedTokenSource(subscriptionCancellationToken, enumerationCancellationToken)
                : null;
        }

        private sealed class Enumerator(
            LocalGameSession session,
            long subscriberId,
            Channel<GameSessionUpdate> channel,
            CancellationToken cancellationToken,
            CancellationTokenSource? linkedCancellation) : IAsyncEnumerator<GameSessionUpdate>
        {
            public GameSessionUpdate Current { get; private set; } = null!;

            public async ValueTask<bool> MoveNextAsync()
            {
                while (await channel.Reader.WaitToReadAsync(cancellationToken))
                {
                    if (channel.Reader.TryRead(out var update))
                    {
                        Current = update;
                        return true;
                    }
                }

                return false;
            }

            public ValueTask DisposeAsync()
            {
                session.RemoveSubscriber(subscriberId, channel.Writer);
                linkedCancellation?.Dispose();
                return ValueTask.CompletedTask;
            }
        }
    }
}
