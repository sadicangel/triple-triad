namespace TripleTriad.Games;
public sealed class CardMove
{
    public required Player Player { get; init; }

    public required int HandIndex { get; init; }

    public required int CellIndex { get; init; }
}
