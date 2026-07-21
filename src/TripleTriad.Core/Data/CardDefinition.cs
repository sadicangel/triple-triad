using System.Text.Json.Serialization;

namespace TripleTriad.Data;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Element
{
    None,
    Fire,
    Ice,
    Thunder,
    Water,
    Earth,
    Wind,
    Holy,
    Dark,
    Poison,
}

public enum Direction
{
    West,
    North,
    East,
    South,
}

public readonly record struct CardRanks(int West, int North, int East, int South)
{
    public int Get(Direction direction) => direction switch
    {
        Direction.West => West,
        Direction.North => North,
        Direction.East => East,
        Direction.South => South,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
    };
}

public sealed record CardDefinition(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("edition")]
    int Edition,
    [property: JsonPropertyName("number")] int Number,
    [property: JsonPropertyName("tier")] int Tier,
    [property: JsonPropertyName("element")]
    Element Element,
    [property: JsonPropertyName("w")] int W,
    [property: JsonPropertyName("n")] int N,
    [property: JsonPropertyName("e")] int E,
    [property: JsonPropertyName("s")] int S)
{
    public CardRanks Ranks => new(W, N, E, S);
}
