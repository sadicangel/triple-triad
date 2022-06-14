using System.Text.Json.Serialization;

namespace TripleTriad.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TradeRules
{
    /// <summary>
    /// The winner takes one card from the loser.
    /// </summary>
    One,
    /// <summary>
    /// Each player takes cards they turned over.
    /// </summary>
    Direct,
    /// <summary>
    /// The winner takes the number of cards by which they won.
    /// </summary>
    Diff,
    /// <summary>
    /// The winner takes all the cards from the loser.
    /// </summary>
    All
}
