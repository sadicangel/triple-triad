namespace TripleTriad.Models;

public sealed class Card
{
    public string Id { get; init; }
    public string Name { get; init; }
    public int Edition { get; init; }
    public int Tier { get; init; }
    public int Number { get; init; }
    public int Version { get; init; }
    public string ImageUri { get => $"Assets/E{Edition:D2}/{Id}.png"; }
    public Element Element { get; init; }
    public int Left { get; init; }
    public int Up { get; init; }
    public int Right { get; init; }
    public int Down { get; init; }
}
