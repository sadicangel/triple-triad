namespace TripleTriad.Contracts;

[Flags]
public enum GameRules
{
    /// <summary>
    /// No special rules are in effect. The player selects 5 cards from this deck, the only
    /// way to capture cards is by placing a card with a higher rank than the opposing card
    /// on the touching side and the game ends when all spaces are filled. If each player has
    /// captured the same number of cards, the game ends in a draw.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Players are able to see the cards in their opponent's hand.
    /// </summary>
    Open = 1 << 0,

    /// <summary>
    /// Five cards are randomly chosen from the player's deck instead of the
    /// player being able to choose five cards themselves.
    /// </summary>
    Random = 1 << 1,

    /// <summary>
    /// In the elemental rule, one or more of the spaces are randomly marked
    /// with an element. Some cards have elements in the upper-right corner.
    /// Ruby Dragon, for example, is fire-elemental, and Quezacotl is thunder-elemental.
    /// When an elemental card is placed on a corresponding element, each rank
    /// goes up a point. When any card is placed on a non-matching element, each
    /// rank goes down a point. This does not affect the Same, Plus and Same Wall
    /// rules where the cards' original ranks apply.
    /// </summary>
    Elemental = 1 << 2,

    /// <summary>
    /// When a card is placed touching two or more other cards
    /// (one or both of them have to be the opposite color), and
    /// the touching sides of each card is the same
    /// (8 touching 8 for example), then the other two cards are flipped.
    /// Combo rule applies.
    /// </summary>
    Same = 1 << 3,

    /// <summary>
    /// Similar to the Same rule. When one card is placed touching two others
    /// and the ranks touching the cards plus the opposing rank equal the same
    /// sum, then both cards are captured. Combo rule applies.
    /// </summary>
    Plus = 1 << 4,

    /// <summary>
    /// An extension of the Same rule. The edges of the board are counted
    /// as A ranks for the purposes of the Same rule. Combo rule applies.
    /// If the Same rule is not present in a region that has Same Wall,
    /// Same Wall will not appear in the list of rules when starting a game
    /// because it can have no effect without Same but it will be carried
    /// with the player to other regions, and can therefore still be spread.
    /// </summary>
    Wall = 1 << 5,

    /// <summary>
    /// Of the cards captured by the Same, Same Wall or Plus rule, if they are
    /// adjacent to another card whose rank is lower, it is captured as well.
    /// This is not a separate rule; any time Same or Plus is in effect, Combo is in effect as well.
    /// </summary>
    Combo = 1 << 6,

    /// <summary>
    /// If a game ends in a draw, each player keeps the cards they captured during
    /// that game, and a new game begins using those cards. Play continues until
    /// a game ends with one player having captured more cards than the other.
    /// </summary>
    SuddenDeath = 1 << 7,
}

public static class GameRulesExtensions
{
    private static readonly GameRules[] DisplayOrder =
    [
        GameRules.Open,
        GameRules.Random,
        GameRules.Elemental,
        GameRules.Same,
        GameRules.Plus,
        GameRules.Wall,
        GameRules.Combo,
        GameRules.SuddenDeath,
    ];

    public static IReadOnlyList<string> ToDisplayNames(this GameRules rules)
    {
        if (rules == GameRules.Default)
            return [nameof(GameRules.Default)];

        var names = DisplayOrder
            .Where(rule => rules.Contains(rule))
            .Select(rule => rule.ToString())
            .ToArray();

        return names.Length == 0 ? [rules.ToString()] : names;
    }

    public static bool Contains(this GameRules rules, GameRules rule) =>
        rule != GameRules.Default && (rules & rule) == rule;
}
