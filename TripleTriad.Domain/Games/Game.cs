using TripleTriad.Interfaces;

namespace TripleTriad.Games;

public sealed class Game : IEntity<string>
{
    public required string Id { get; init; }

    public required Ruleset Rules { get; init; }

    public required Player LeftPlayer { get; init; }

    public required Player RightPlayer { get; init; }

    public required Side ActiveSide { get; set; }

    public Player ActivePlayer { get => ActiveSide == Side.Left ? LeftPlayer : RightPlayer; }
}


//using CommunityToolkit.Mvvm.Messaging;
//using System.Diagnostics;
//using System.Diagnostics.CodeAnalysis;
//using TripleTriad.Domain.Entities;
//using TripleTriad.Domain.Enums;
//using TripleTriad.Domain.Events;

//namespace TripleTriad.Domain.Aggregates;
//public sealed class Game
//{
//	private static readonly IReadOnlyList<Direction> Directions = Enum.GetValues<Direction>();

//	private readonly IMessenger _messenger;
//	private Board? _board;

//	public Board Board { get => _board ?? throw new InvalidOperationException("Board is invalid"); }

//	public Game(IMessenger messenger)
//	{
//		_messenger = messenger;
//	}

//	public void StartGame(Ruleset rules, Player leftPlayer, List<Card> leftHand, Player rightPlayer, List<Card> rightHand)
//	{
//		Debug.WriteLine("Game has started");
//		_board = new ServerBoard(_messenger)
//		{
//			Rules = rules,
//			Cells = Enumerable.Range(0, 9).Select(i => new Cell { Index = i, Element = Element.None }).ToList(),
//			LeftPlayer = leftPlayer,
//			LeftHand = new(leftHand),
//			RightPlayer = rightPlayer,
//			RightHand = new(rightHand),
//			ActiveSide = (Side)Random.Shared.Next(2)
//		};

//		_messenger.Send(new GameStartedEvent
//		{
//			Data = new GameStartedData
//			{
//				Rules = _board.Rules,
//				Cells = _board.Cells,
//				LeftPlayer = leftPlayer,
//				LeftHand = leftHand,
//				RightPlayer = rightPlayer,
//				RightHand = rightHand,
//				ActiveSide = _board.ActiveSide,
//			}
//		});
//	}

//	public void PlayCard(CardMove move)
//	{
//		Debug.WriteLine($"{move.Player.Side} plays a card [Hand {move.HandIndex} => Cell {move.CellIndex}]");
//		var cell = Board.Cells[move.CellIndex];
//		var card = Board.ActiveHand[move.HandIndex];
//		Board.ActiveHand.RemoveAt(move.HandIndex);

//		cell.Owner = Board.ActivePlayer;
//		cell.Card = card;

//		var adjacent = GetAdjacentCells(cell);

//		var flipped = new List<DirectedValue<AffectedCell>>();

//		if (TrySameRule(Board.Rules, adjacent, out var affected) || TryPlusRule(Board.Rules, adjacent, out affected) || TryDefaultRule(adjacent, out affected))
//		{
//			var queue = new Queue<DirectedValue<AffectedCell>>(affected.EnumerateUnowned());
//			if (Board.Rules.HasRule(BoardRules.Combo))
//			{
//				while (queue.Count > 0)
//				{
//					var d = queue.Dequeue();
//					var c = d.Value.Cell!;
//					var adj = GetAdjacentCells(c);
//					if (TryDefaultRule(adj, out var aff))
//						foreach (var f in aff.EnumerateUnowned())
//							queue.Enqueue(f);
//					flipped.Add(d);
//				}
//			}
//			else
//			{
//				flipped.AddRange(affected.EnumerateUnowned());
//			}
//		}

//		if (flipped.Count > 0)
//		{
//			_messenger.Send(new CardsFlippedEvent
//			{
//				Data = new CardsFlippedData
//				{
//					NewOwner = Board.ActivePlayer,
//					CardsFlipped = flipped.Select(e => new CardFlipped(e.Value.Cell!.Index, e.Value.Rules)).ToList()
//				}
//			});
//		}

//		if (Board.Cells.Any(e => e.IsEmpty))
//		{
//			Board.ActiveSide = Board.ActiveSide.Toggle();
//			_messenger.Send(new ActiveSideChangedEvent { Data = new ActiveSideChangedData { ActiveSide = Board.ActiveSide } });
//		}
//		else
//		{
//			var winnerSide = Board.Cells.GroupBy(c => c.Owner!.Side).MaxBy(e => e.Count())!.Key;

//			_messenger.Send(new GameOverEvent
//			{
//				Data = new GameOverData
//				{
//					Winner = winnerSide == Side.Left ? Board.LeftPlayer : Board.RightPlayer
//				}
//			});
//		}
//	}

//	private AdjacentGroup GetAdjacentCells(Cell center)
//	{
//		Cell? left = null, up = null, right = null, down = null;
//		if (center.Column > 0)
//			left = Board.Cells[center.Row * 3 + center.Column - 1];
//		if (center.Row > 0)
//			up = Board.Cells[(center.Row - 1) * 3 + center.Column];
//		if (center.Column < 2)
//			right = Board.Cells[center.Row * 3 + center.Column + 1];
//		if (center.Row < 2)
//			down = Board.Cells[(center.Row + 1) * 3 + center.Column];
//		return new AdjacentGroup
//		{
//			Center = center,
//			Left = left,
//			Up = up,
//			Right = right,
//			Down = down
//		};
//	}

//	private static bool TrySameRule(Ruleset rules, AdjacentGroup adjacent, [NotNullWhen(true)] out AffectedGroup? affected)
//	{
//		if (adjacent.Center.Card is null)
//			throw new ArgumentException("Invalid center cell: No card", nameof(adjacent));

//		if (!rules.HasRule(BoardRules.Same))
//		{
//			affected = null;
//			return false;
//		}

//		affected = new AffectedGroup { Center = adjacent.Center };

//		var useWall = rules.HasRule(BoardRules.SameWall);
//		foreach (var direction in Directions)
//		{
//			var value = useWall ? 10 : 0;
//			if (adjacent.TryGetCell(direction, out var other))
//				value = other.GetValueOrZero(direction.Opposite());
//			if (adjacent.Center.GetValue(direction) == value)
//			{
//				affected.SetCell(direction, new AffectedCell(other, BoardRules.Same | (useWall ? BoardRules.SameWall : 0)));
//				if (affected.CountUnowned >= 1)
//					return true;
//			}
//		}

//		return false;
//	}

//	private static bool TryPlusRule(Ruleset rules, AdjacentGroup adjacent, [NotNullWhen(true)] out AffectedGroup? affected)
//	{
//		if (adjacent.Center.Card is null)
//			throw new ArgumentException("Invalid center cell: No card", nameof(adjacent));

//		if (!rules.HasRule(BoardRules.Plus))
//		{
//			affected = null;
//			return false;
//		}

//		affected = new AffectedGroup { Center = adjacent.Center };

//		var values = new int[4];
//		foreach (var direction in Directions)
//		{
//			if (adjacent.TryGetCell(direction, out var other))
//			{
//				var value = other.GetValueOrZero(direction.Opposite());
//				if (value > 0)
//				{
//					values[(int)direction] = value + adjacent.Center.Card[direction];
//					for (int i = (int)direction; i >= 0; ++i)
//					{
//						for (int j = 0; j < (int)direction; ++j)
//						{
//							if (values[j] > 0 && values[j] == value)
//							{
//								affected.SetCell((Direction)j, new AffectedCell(adjacent.GetCell(direction), BoardRules.Plus));
//								affected.SetCell(direction, new AffectedCell(other, BoardRules.Plus));
//								if (affected.CountUnowned > 0)
//								{
//									return true;
//								}

//								affected.Clear();
//							}
//						}
//					}
//				}
//			}
//		}

//		return false;
//	}

//	private static bool TryDefaultRule(AdjacentGroup adjacent, [NotNullWhen(true)] out AffectedGroup? affected)
//	{
//		if (adjacent.Center.Card is null)
//			throw new ArgumentException("Invalid center cell: No card", nameof(adjacent));

//		affected = new AffectedGroup { Center = adjacent.Center };

//		foreach (var direction in Directions)
//		{
//			if (adjacent.TryGetCell(direction, out var other))
//			{
//				var value = other.GetValueOrZeroWithElement(direction.Opposite());
//				if (value > 0 && adjacent.Center.GetValueOrZeroWithElement(direction) > value)
//					affected.SetCell(direction, new AffectedCell(other, BoardRules.None));
//			}
//		}

//		return affected.CountUnowned > 0;
//	}

//}

//public readonly record struct DirectedValue<T>(Direction Direction, T Value);

//public readonly record struct AffectedCell(Cell? Cell, BoardRules Rules);

//public sealed class AdjacentGroup
//{
//	public Cell Center { get; init; } = default!;

//	public Cell? Left { get; init; }

//	public Cell? Up { get; init; }

//	public Cell? Right { get; init; }

//	public Cell? Down { get; init; }

//	public Cell? GetCell(Direction direction) => TryGetCell(direction, out var cell) ? cell : throw new ArgumentException("Cell does not exist", nameof(direction));

//	public bool TryGetCell(Direction direction, out Cell? cell)
//	{
//		cell = null;
//		switch (direction)
//		{
//			case Direction.Left when Center.Column > 0:
//				cell = Left;
//				return true;
//			case Direction.Up when Center.Row > 0:
//				cell = Up;
//				return true;
//			case Direction.Right when Center.Column < 2:
//				cell = Right;
//				return true;
//			case Direction.Down when Center.Row < 2:
//				cell = Down;
//				return true;
//			default:
//				return false;
//		}
//	}

//	public IEnumerable<DirectedValue<Cell>> Enumerate()
//	{
//		if (Left is not null)
//			yield return new(Direction.Left, Left);
//		if (Up is not null)
//			yield return new(Direction.Up, Up);
//		if (Right is not null)
//			yield return new(Direction.Right, Right);
//		if (Down is not null)
//			yield return new(Direction.Down, Down);
//	}
//}

//public sealed class AffectedGroup
//{
//	public Cell Center { get; init; } = default!;

//	public AffectedCell? Left { get; set; }

//	public AffectedCell? Up { get; set; }

//	public AffectedCell? Right { get; set; }

//	public AffectedCell? Down { get; set; }

//	public int CountAll { get => (Left is not null ? 1 : 0) + (Up is not null ? 1 : 0) + (Right is not null ? 1 : 0) + (Down is not null ? 1 : 0); }
//	public int CountUnowned { get => (Left is not null && Left.Value.Cell!.Owner != Center.Owner ? 1 : 0) + (Up is not null && Up.Value.Cell!.Owner != Center.Owner ? 1 : 0) + (Right is not null && Right.Value.Cell!.Owner != Center.Owner ? 1 : 0) + (Down is not null && Down.Value.Cell!.Owner != Center.Owner ? 1 : 0); }

//	public void SetCell(Direction direction, AffectedCell cell)
//	{
//		switch (direction)
//		{
//			case Direction.Left:
//				Left = cell;
//				break;
//			case Direction.Up:
//				Up = cell;
//				break;
//			case Direction.Right:
//				Right = cell;
//				break;
//			case Direction.Down:
//				Down = cell;
//				break;
//			default:
//				throw new ArgumentException("Invalid direction", nameof(direction));
//		}
//	}

//	public void Clear()
//	{
//		Left = null;
//		Up = null;
//		Right = null;
//		Down = null;
//	}

//	public IEnumerable<DirectedValue<AffectedCell>> EnumerateAll()
//	{
//		if (Left is not null)
//			yield return new(Direction.Left, Left.Value);
//		if (Up is not null)
//			yield return new(Direction.Up, Up.Value);
//		if (Right is not null)
//			yield return new(Direction.Right, Right.Value);
//		if (Down is not null)
//			yield return new(Direction.Down, Down.Value);
//	}

//	public IEnumerable<DirectedValue<AffectedCell>> EnumerateUnowned()
//	{
//		if (Left is not null && Left.Value.Cell!.Owner != Center.Owner)
//			yield return new(Direction.Left, Left.Value);
//		if (Up is not null && Up.Value.Cell!.Owner != Center.Owner)
//			yield return new(Direction.Up, Up.Value);
//		if (Right is not null && Right.Value.Cell!.Owner != Center.Owner)
//			yield return new(Direction.Right, Right.Value);
//		if (Down is not null && Down.Value.Cell!.Owner != Center.Owner)
//			yield return new(Direction.Down, Down.Value);
//	}
//}
