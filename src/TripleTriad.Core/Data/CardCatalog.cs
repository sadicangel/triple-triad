using System.Text.Json;
using System.Text.Json.Serialization;

namespace TripleTriad.Data;

public sealed class CardCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly Dictionary<int, CardDefinition> _byNumber;

    public CardCatalog(IEnumerable<CardDefinition> cards)
    {
        Cards = cards.OrderBy(card => card.Number).ToArray();
        _byNumber = Cards.ToDictionary(card => card.Number);
    }

    public IReadOnlyList<CardDefinition> Cards { get; }

    public static CardCatalog Load(string path)
    {
        using var stream = File.OpenRead(path);
        var cards = JsonSerializer.Deserialize<CardDefinition[]>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"No card definitions were found in '{path}'.");

        return new CardCatalog(cards);
    }

    public CardDefinition Get(int number)
    {
        if (_byNumber.TryGetValue(number, out var card))
            return card;

        throw new ArgumentOutOfRangeException(nameof(number), number, "Unknown Triple Triad card number.");
    }
}
