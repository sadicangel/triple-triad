using Godot;
using TripleTriad.Contracts;

namespace TripleTriad.Bridge;

public abstract partial class GameEventResource : Resource
{
    [Export] public long Sequence { get; set; }

    [Export] public string Type { get; set; } = string.Empty;

    [Export] public MatchSnapshotResource? Snapshot { get; set; }

    [Export] public string ClientRequestId { get; set; } = string.Empty;

    public static GameEventResource FromModel(GameEvent gameEvent) => gameEvent switch
    {
        MatchStartedEvent started => MatchStartedEventResource.FromModel(started),
        CardPlayedEvent played => CardPlayedEventResource.FromModel(played),
        CardCapturedEvent captured => CardCapturedEventResource.FromModel(captured),
        TurnChangedEvent turnChanged => TurnChangedEventResource.FromModel(turnChanged),
        MoveRejectedEvent rejected => MoveRejectedEventResource.FromModel(rejected),
        MatchEndedEvent ended => MatchEndedEventResource.FromModel(ended),
        _ => throw new ArgumentOutOfRangeException(nameof(gameEvent), $"Unknown game event type: {gameEvent.GetType().Name}")
    };

    protected void CopyBaseFrom(GameEvent gameEvent)
    {
        Sequence = gameEvent.Sequence;
        Type = gameEvent.Type;
        Snapshot = MatchSnapshotResource.FromModel(gameEvent.Snapshot);
        ClientRequestId = gameEvent.ClientRequestId ?? string.Empty;
    }
}

[GlobalClass]
public sealed partial class MatchStartedEventResource : GameEventResource
{
    [Export] public Seat StartingSeat { get; set; }

    public static MatchStartedEventResource FromModel(MatchStartedEvent model)
    {
        var resource = new MatchStartedEventResource
        {
            StartingSeat = model.StartingSeat,
        };
        resource.CopyBaseFrom(model);
        return resource;
    }
}

[GlobalClass]
public sealed partial class CardPlayedEventResource : GameEventResource
{
    [Export] public string CardInstanceId { get; set; } = string.Empty;

    [Export] public CardSnapshotResource? Card { get; set; }

    [Export] public Seat SourceSeat { get; set; }

    [Export] public int SourceHandIndex { get; set; }

    [Export] public int BoardSlotIndex { get; set; }

    [Export] public Seat Seat { get; set; }

    public static CardPlayedEventResource FromModel(CardPlayedEvent model)
    {
        var resource = new CardPlayedEventResource
        {
            CardInstanceId = model.CardInstanceId,
            Card = CardSnapshotResource.FromModel(model.Card),
            SourceSeat = model.SourceSeat,
            SourceHandIndex = model.SourceHandIndex,
            BoardSlotIndex = model.BoardSlotIndex,
            Seat = model.Seat,
        };
        resource.CopyBaseFrom(model);
        return resource;
    }
}

[GlobalClass]
public sealed partial class CardCapturedEventResource : GameEventResource
{
    [Export] public string CardInstanceId { get; set; } = string.Empty;

    [Export] public CardSnapshotResource? Card { get; set; }

    [Export] public int BoardSlotIndex { get; set; }

    [Export] public Seat PreviousOwner { get; set; }

    [Export] public Seat NewOwner { get; set; }

    public static CardCapturedEventResource FromModel(CardCapturedEvent model)
    {
        var resource = new CardCapturedEventResource
        {
            CardInstanceId = model.CardInstanceId,
            Card = CardSnapshotResource.FromModel(model.Card),
            BoardSlotIndex = model.BoardSlotIndex,
            PreviousOwner = model.PreviousOwner,
            NewOwner = model.NewOwner,
        };
        resource.CopyBaseFrom(model);
        return resource;
    }
}

[GlobalClass]
public sealed partial class TurnChangedEventResource : GameEventResource
{
    [Export] public Seat ActiveSeat { get; set; }

    public static TurnChangedEventResource FromModel(TurnChangedEvent model)
    {
        var resource = new TurnChangedEventResource
        {
            ActiveSeat = model.ActiveSeat,
        };
        resource.CopyBaseFrom(model);
        return resource;
    }
}

[GlobalClass]
public sealed partial class MoveRejectedEventResource : GameEventResource
{
    [Export] public string Reason { get; set; } = string.Empty;

    [Export] public string CardInstanceId { get; set; } = string.Empty;

    [Export] public int BoardSlotIndex { get; set; } = -1;

    public static MoveRejectedEventResource FromModel(MoveRejectedEvent model)
    {
        var resource = new MoveRejectedEventResource
        {
            Reason = model.Reason,
            CardInstanceId = model.CardInstanceId ?? string.Empty,
            BoardSlotIndex = model.BoardSlotIndex ?? -1,
        };
        resource.CopyBaseFrom(model);
        return resource;
    }
}

[GlobalClass]
public sealed partial class MatchEndedEventResource : GameEventResource
{
    [Export] public Seat Winner { get; set; }

    [Export] public bool HasWinner { get; set; }

    public static MatchEndedEventResource FromModel(MatchEndedEvent model)
    {
        var resource = new MatchEndedEventResource
        {
            Winner = model.Winner ?? default,
            HasWinner = model.Winner is not null,
        };
        resource.CopyBaseFrom(model);
        return resource;
    }
}
