using System.Text.Json.Serialization;

namespace TripleTriad.Components;

public sealed record class CardValues(
    int Edition,
    int Number,
    int Tier,
    Element Element,
    int W,
    int N,
    int E,
    int S);

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
    W, N, E, S,
}

