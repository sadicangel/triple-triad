using System.Diagnostics.CodeAnalysis;

namespace TripleTriad.Models;

public sealed class Move
{
    [NotNull] public Player? Player { get; set; }
    [NotNull] public Card? Card { get; set; }
    public int Index { get; set; } = -1;
}