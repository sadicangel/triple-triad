namespace TripleTriad.Games;

[Flags]
public enum BoardRules
{
    /// <summary>
    /// No board rules.
    /// </summary>
    None = 0x00,
    /// <summary>
    /// When a card is placed touching two or more other cards
    /// (one or both of them have to be the opposite color), and
    /// the touching sides of each card is the same
    /// (8 touching 8 for example), then the other two cards are flipped.
    /// Combo rule applies.
    /// </summary>
    Same = 0x01,
    /// <summary>
    /// An extension of the Same rule. The edges of the board are counted
    /// as A ranks for the purposes of the Same rule. Combo rule applies.
    /// </summary>
    SameWall = 0x02,
    /// <summary>
    /// When one card is placed touching two others and the ranks touching
    /// the cards plus the opposing rank equal the same sum, then both
    /// cards are captured. Combo rule applies.
    /// </summary>
    Plus = 0x04,
    /// <summary>
    /// Of the cards captured by the Same, Same Wall or Plus rule, if they are
    /// adjacent to another card whose rank is lower, it is captured as well.
    /// </summary>
    Combo = 0x08,
    /// <summary>
    /// In the elemental rule, one or more of the spaces are randomly marked
    /// with an element. Some cards have elements in the upper-right corner.
    /// Ruby Dragon, for example, is fire-elemental, and Quezacotl is thunder-elemental.
    /// When an elemental card is placed on a corresponding element, each rank
    /// goes up a point. When any card is placed on a non-matching element, each
    /// rank goes down a point. This does not affect the Same, Plus and Same Wall
    /// rules where the cards' original ranks apply. 
    /// </summary>
    Elemental = 0x10,
}
