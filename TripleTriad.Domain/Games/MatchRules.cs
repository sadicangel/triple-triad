namespace TripleTriad.Games;

[Flags]
public enum MatchRules
{
    /// <summary>
    /// No board rules.
    /// </summary>
    None = 0x00,
    /// <summary>
    /// Enables the player to see which cards the opponent is using.
    /// </summary>
    Open = 0x01,
    /// <summary>
    /// Five cards are randomly chosen from the player's deck instead of the
    /// player being able to choose five cards themselves.
    /// </summary>
    Random = 0x02,
    /// <summary>
    /// If the game ends in a draw, a sudden death occurs in which a new game
    /// is started but the cards are distributed on the side of the color they
    /// were on at the end of the game.
    /// </summary>
    SuddenDeath = 0x04,
}
