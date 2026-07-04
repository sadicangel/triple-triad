using TripleTriad.Contracts;
using TripleTriad.Data;

namespace TripleTriad.Mock;

public sealed class MockGameSession : IGameSession
{
	private readonly bool _autoPlayOpponent;
	private readonly CardState?[] _board = new CardState?[9];
	private readonly CardCatalog _catalog;
	private readonly Dictionary<Seat, List<CardState>> _hands;
	private readonly Seat _localSeat;
	private readonly bool _revealOpponentHand;
	private bool _isComplete;
	private Seat _activeSeat;

	public MockGameSession(
		CardCatalog catalog,
		Seat localSeat = Seat.Blue,
		bool revealOpponentHand = false,
		bool autoPlayOpponent = false)
	{
		_catalog = catalog;
		_localSeat = localSeat;
		_revealOpponentHand = revealOpponentHand;
		_autoPlayOpponent = autoPlayOpponent;
		_activeSeat = Seat.Blue;
		_hands = new Dictionary<Seat, List<CardState>>
		{
			[Seat.Red] = CreateHand(Seat.Red, [6, 7, 8, 9, 10]),
			[Seat.Blue] = CreateHand(Seat.Blue, [1, 2, 3, 4, 105]),
		};

		CurrentSnapshot = BuildSnapshot();
	}

	public MatchSnapshot CurrentSnapshot { get; private set; }

	public event Action<MatchSnapshot>? SnapshotChanged;

	public event Action<GameEvent>? EventRaised;

	public ValueTask SubmitAsync(GameCommand command, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (command is PlayCardCommand playCard)
			PlayCard(playCard);
		else
			EventRaised?.Invoke(new MoveRejectedEvent("Unknown command.", null, null, command.ClientRequestId));

		return ValueTask.CompletedTask;
	}

	private List<CardState> CreateHand(Seat seat, int[] cardNumbers) =>
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
		CurrentSnapshot = snapshot;
		EventRaised?.Invoke(new CardPlayedEvent(
			card.InstanceId,
			ToSnapshot(card, isFaceUp: true, isPlayable: false),
			handSeat,
			sourceHandIndex,
			command.BoardSlotIndex,
			card.Owner,
			command.ClientRequestId));

		foreach (var captured in capturedCards)
		{
			EventRaised?.Invoke(new CardCapturedEvent(
				captured.Card.InstanceId,
				ToSnapshot(captured.Card, isFaceUp: true, isPlayable: false),
				captured.BoardSlotIndex,
				captured.PreviousOwner,
				captured.Card.Owner,
				command.ClientRequestId));
		}

		if (_isComplete)
		{
			EventRaised?.Invoke(new MatchEndedEvent(GetWinner(), command.ClientRequestId));
			PublishSnapshot(snapshot);
			return;
		}

		EventRaised?.Invoke(new TurnChangedEvent(_activeSeat, command.ClientRequestId));
		PublishSnapshot(snapshot);

		if (_autoPlayOpponent && _activeSeat != _localSeat)
			AutoPlayOpponentTurn();
	}

	private void AutoPlayOpponentTurn()
	{
		if (_isComplete || !_hands.TryGetValue(_activeSeat, out var hand) || hand.Count == 0)
			return;

		var slot = Array.FindIndex(_board, card => card is null);
		if (slot < 0)
			return;

		var card = hand[0];
		PlayCard(new PlayCardCommand(card.InstanceId, slot, $"mock-{Guid.NewGuid():N}"));
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

		var hands = new[]
		{
			BuildHandSnapshot(Seat.Red),
			BuildHandSnapshot(Seat.Blue),
		};

		var allCards = _hands.Values.SelectMany(cards => cards).Concat(_board.OfType<CardState>());
		var blueScore = allCards.Count(card => card.Owner == Seat.Blue);
		var redScore = allCards.Count(card => card.Owner == Seat.Red);

		return new MatchSnapshot(
			_activeSeat,
			_localSeat,
			_revealOpponentHand ? ["Open"] : ["Basic"],
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

	private static CardSnapshot ToSnapshot(CardState card, bool isFaceUp, bool isPlayable) =>
		new(
			card.InstanceId,
			card.Definition.Number,
			card.Definition.Name,
			card.Definition.Element,
			card.Definition.Ranks,
			card.Owner,
			isFaceUp,
			isPlayable);

	private void PublishSnapshot()
	{
		CurrentSnapshot = BuildSnapshot();
		SnapshotChanged?.Invoke(CurrentSnapshot);
	}

	private void PublishSnapshot(MatchSnapshot snapshot)
	{
		CurrentSnapshot = snapshot;
		SnapshotChanged?.Invoke(CurrentSnapshot);
	}

	private void Reject(string reason, PlayCardCommand command) =>
		EventRaised?.Invoke(new MoveRejectedEvent(
			reason,
			command.CardInstanceId,
			command.BoardSlotIndex,
			command.ClientRequestId));

	private bool IsBoardFull() => _board.All(card => card is not null);

	private Seat? GetWinner()
	{
		if (CurrentSnapshot.BlueScore == CurrentSnapshot.RedScore)
			return null;

		return CurrentSnapshot.BlueScore > CurrentSnapshot.RedScore ? Seat.Blue : Seat.Red;
	}

	private sealed class CardState(string instanceId, CardDefinition definition, Seat owner)
	{
		public string InstanceId { get; } = instanceId;

		public CardDefinition Definition { get; } = definition;

		public Seat Owner { get; set; } = owner;
	}

	private readonly record struct CapturedCard(int BoardSlotIndex, CardState Card, Seat PreviousOwner);
}
