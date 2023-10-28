namespace TripleTriad.Games;

public sealed class Player
{
    public required string UserId { get; init; }

    public required string UserName { get; init; }

    public required uint Color { get; init; }

    public required Side Side { get; init; }

    public required List<Card> Hand { get; init; } = new();
}