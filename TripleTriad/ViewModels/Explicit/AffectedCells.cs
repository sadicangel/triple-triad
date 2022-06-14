using System.Collections;
using TripleTriad.Models;

namespace TripleTriad.ViewModels.Explicit;

public sealed class AffectedCells
{
    public CellViewModel CenterCell { get; }
    public CellViewModel? Left { get; set; }
    public CellViewModel? Up { get; set; }
    public CellViewModel? Right { get; set; }
    public CellViewModel? Down { get; set; }
    public int Count { get => (Left is not null ? 1 : 0) + (Up is not null ? 1 : 0) + (Right is not null ? 1 : 0) + (Down is not null ? 1 : 0); }
    public int CountUnowned { get => (Left is not null && Left.Player != CenterCell.Player ? 1 : 0) + (Up is not null && Up.Player != CenterCell.Player ? 1 : 0) + (Right is not null && Right.Player != CenterCell.Player ? 1 : 0) + (Down is not null && Down.Player != CenterCell.Player? 1 : 0); }

    public AffectedCells(CellViewModel centerCell)
    {
        CenterCell = centerCell;
    }

    public IEnumerable<DirectedCell> Cells()
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

    public IEnumerable<DirectedCell> CellsUnowned()
    {
        if (Left is not null && Left.Player != CenterCell.Player)
            yield return new(Direction.Left, Left);
        if (Up is not null && Up.Player != CenterCell.Player)
            yield return new(Direction.Up, Up);
        if (Right is not null && Right.Player != CenterCell.Player)
            yield return new(Direction.Right, Right);
        if (Down is not null && Down.Player != CenterCell.Player)
            yield return new(Direction.Down, Down);
    }
}
