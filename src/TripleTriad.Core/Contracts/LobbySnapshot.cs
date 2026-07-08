namespace TripleTriad.Contracts;

public enum LobbyPlayerKind
{
    Human,
    AI,
}

public sealed record LobbyPlayerSnapshot(
    Seat Seat,
    string PlayerName,
    bool IsReady,
    LobbyPlayerKind Kind = LobbyPlayerKind.Human,
    bool IsConnected = true);

public sealed record LobbyCardSelectionSnapshot(
    Seat Seat,
    IReadOnlyList<int> CardNumbers);

public sealed record LobbySnapshot(
    Seat LocalSeat,
    GameRules Rules,
    IReadOnlyList<LobbyPlayerSnapshot> Players,
    IReadOnlyList<LobbyCardSelectionSnapshot> CardSelections,
    bool CanStart,
    bool IsMatchStarting)
{
    public LobbySnapshot(
        Seat LocalSeat,
        GameRules Rules,
        IReadOnlyList<LobbyPlayerSnapshot> Players,
        bool CanStart,
        bool IsMatchStarting)
        : this(LocalSeat, Rules, Players, [], CanStart, IsMatchStarting)
    {
    }
}

public sealed record MatchSetup(
    GameRules Rules,
    IReadOnlyList<LobbyPlayerSnapshot> Players,
    IReadOnlyList<LobbyCardSelectionSnapshot> CardSelections)
{
    public MatchSetup(
        GameRules Rules,
        IReadOnlyList<LobbyPlayerSnapshot> Players)
        : this(Rules, Players, [])
    {
    }
}

public static class LobbyCardSelectionRules
{
    public const int HandSize = 5;
    public const int MinCardNumber = 1;
    public const int MaxCardNumber = 110;

    public static int[] Validate(IReadOnlyList<int> cardNumbers)
    {
        ArgumentNullException.ThrowIfNull(cardNumbers);

        if (cardNumbers.Count != HandSize)
            throw new ArgumentException($"Exactly {HandSize} cards must be selected.", nameof(cardNumbers));

        var normalized = cardNumbers.ToArray();
        if (normalized.Distinct().Count() != HandSize)
            throw new ArgumentException("Selected cards must be unique.", nameof(cardNumbers));

        foreach (var cardNumber in normalized)
        {
            if (cardNumber is < MinCardNumber or > MaxCardNumber)
                throw new ArgumentOutOfRangeException(nameof(cardNumbers), cardNumber, "Unknown Triple Triad card number.");
        }

        return normalized;
    }
}
