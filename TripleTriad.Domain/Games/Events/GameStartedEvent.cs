using TripleTriad.Games.Dtos;

namespace TripleTriad.Games.Events;

public sealed class GameStartedEvent : GameEvent<GameDto>
{
    public override string Type { get; init; } = "Game.Started";
}