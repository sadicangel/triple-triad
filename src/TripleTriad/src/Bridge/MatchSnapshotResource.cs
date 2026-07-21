using Godot;
using TripleTriad.Contracts;
using TripleTriad.Data;

namespace TripleTriad.Bridge;

public abstract partial class SnapshotResource : Resource;

[GlobalClass]
public sealed partial class CardRanksResource : SnapshotResource
{
    [Export] public int West { get; set; }

    [Export] public int North { get; set; }

    [Export] public int East { get; set; }

    [Export] public int South { get; set; }

    public static CardRanksResource FromModel(CardRanks model) =>
        new()
        {
            West = model.West,
            North = model.North,
            East = model.East,
            South = model.South,
        };
}

[GlobalClass]
public sealed partial class CardSnapshotResource : SnapshotResource
{
    [Export] public string CardInstanceId { get; set; } = string.Empty;

    [Export] public int CardNumber { get; set; }

    [Export] public string Name { get; set; } = string.Empty;

    [Export] public Element Element { get; set; }

    [Export] public CardRanksResource? Ranks { get; set; }

    [Export] public Seat Owner { get; set; }

    [Export] public bool IsFaceUp { get; set; }

    [Export] public bool IsPlayable { get; set; }

    public static CardSnapshotResource FromModel(CardSnapshot model) =>
        new()
        {
            CardInstanceId = model.CardInstanceId,
            CardNumber = model.CardNumber,
            Name = model.Name,
            Element = model.Element,
            Ranks = CardRanksResource.FromModel(model.Ranks),
            Owner = model.Owner,
            IsFaceUp = model.IsFaceUp,
            IsPlayable = model.IsPlayable,
        };

    public static CardSnapshotResource FromDefinition(
        CardDefinition definition,
        Seat owner,
        bool isFaceUp = true,
        bool isPlayable = false,
        string? cardInstanceId = null) =>
        new()
        {
            CardInstanceId = cardInstanceId ?? $"catalog-{definition.Number}",
            CardNumber = definition.Number,
            Name = definition.Name,
            Element = definition.Element,
            Ranks = CardRanksResource.FromModel(definition.Ranks),
            Owner = owner,
            IsFaceUp = isFaceUp,
            IsPlayable = isPlayable,
        };

    public static CardSnapshotResource CreateBack(Seat owner) =>
        new()
        {
            CardInstanceId = "card-back",
            CardNumber = 1,
            Name = "Random card",
            Element = Element.None,
            Ranks = new CardRanksResource(),
            Owner = owner,
            IsFaceUp = false,
            IsPlayable = false,
        };
}

[GlobalClass]
public sealed partial class BoardCellSnapshotResource : SnapshotResource
{
    [Export] public int Index { get; set; }

    [Export] public Element Element { get; set; }

    [Export] public CardSnapshotResource? Card { get; set; }

    [Export] public bool CanDrop { get; set; }

    public bool HasCard => Card is not null;

    public static BoardCellSnapshotResource FromModel(BoardCellSnapshot model) =>
        new()
        {
            Index = model.Index,
            Element = model.Element,
            Card = model.Card is null ? null : CardSnapshotResource.FromModel(model.Card),
            CanDrop = model.CanDrop,
        };
}

[GlobalClass]
public sealed partial class HandSnapshotResource : SnapshotResource
{
    [Export] public Seat Seat { get; set; }

    [Export] public bool IsLocal { get; set; }

    [Export] public bool IsRevealed { get; set; }

    [Export] public Godot.Collections.Array Cards { get; set; } = [];

    public static HandSnapshotResource FromModel(HandSnapshot model)
    {
        var cards = new Godot.Collections.Array();
        foreach (var card in model.Cards)
            cards.Add(CardSnapshotResource.FromModel(card));

        return new HandSnapshotResource
        {
            Seat = model.Seat,
            IsLocal = model.IsLocal,
            IsRevealed = model.IsRevealed,
            Cards = cards,
        };
    }
}

[GlobalClass]
public sealed partial class MatchSnapshotResource : SnapshotResource
{
    [Export] public Seat ActiveSeat { get; set; }

    [Export] public Seat LocalSeat { get; set; }

    [Export] public GameRules Rules { get; set; }

    [Export] public int BlueScore { get; set; }

    [Export] public int RedScore { get; set; }

    [Export] public Godot.Collections.Array Board { get; set; } = [];

    [Export] public Godot.Collections.Array Hands { get; set; } = [];

    [Export] public bool IsComplete { get; set; }

    public static MatchSnapshotResource FromModel(MatchSnapshot model)
    {
        var board = new Godot.Collections.Array();
        foreach (var cell in model.Board)
            board.Add(BoardCellSnapshotResource.FromModel(cell));

        var hands = new Godot.Collections.Array();
        foreach (var hand in model.Hands)
            hands.Add(HandSnapshotResource.FromModel(hand));

        return new MatchSnapshotResource
        {
            ActiveSeat = model.ActiveSeat,
            LocalSeat = model.LocalSeat,
            Rules = model.Rules,
            BlueScore = model.BlueScore,
            RedScore = model.RedScore,
            Board = board,
            Hands = hands,
            IsComplete = model.IsComplete,
        };
    }
}
