using System.Text.Json.Serialization;

namespace TripleTriad.Core;

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

