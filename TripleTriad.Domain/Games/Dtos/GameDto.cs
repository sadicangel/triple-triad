namespace TripleTriad.Games.Dtos;
public sealed class GameDto
{
    public required string Id { get; init; }

    public required Ruleset Rules { get; init; }

    public required Player LeftPlayer { get; init; }

    public required Player RightPlayer { get; init; }

    public required Side ActiveSide { get; set; }

    public required Player ActivePlayer { get; set; }
}
