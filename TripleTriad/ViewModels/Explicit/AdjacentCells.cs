using System.Collections;
using TripleTriad.Models;

namespace TripleTriad.ViewModels.Explicit;

public sealed class AdjacentCells
{
    public CellViewModel? Left { get; init; }
    public CellViewModel? Up { get; init; }
    public CellViewModel? Right { get; init; }
    public CellViewModel? Down { get; init; }
    public int Count { get => (Left is not null ? 1 : 0) + (Up is not null ? 1 : 0) + (Right is not null ? 1 : 0) + (Down is not null ? 1 : 0); }

    public IEnumerator<DirectedCell> Cells()
    {
        if (Left is not null)
            yield return new(Direction.Left, Left);
        if (Up is not null)
            yield return new(Direction.Up, Up);
        if (Right is not null)
            yield return new(Direction.Right, Right);
        if (Down is not null)
            yield return new(Direction.Down, Down);
    }
}
