using CommunityToolkit.Mvvm.ComponentModel;

namespace TripleTriad.Games;
public sealed partial class Cell : ObservableObject
{
    [ObservableProperty]
    private Player? _owner;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private Card? _card;

    public int Index { get; init; }

    public int Row { get => Index / 3; }

    public int Column { get => Index % 3; }

    public Element Element { get; init; }

    public bool IsEmpty { get => Card is null; }
}

public static class CellExtensions
{
    public static string GetValueOrSpace(this Cell cell, Direction direction) => cell?.Card?[direction] is int number ? number.ToString("X1") : " ";
    public static int GetValue(this Cell cell, Direction direction) => cell?.Card?[direction] ?? throw new ArgumentException("Invalid cell", nameof(cell));
    public static int GetValueOrZero(this Cell? cell, Direction direction) => cell?.Card?[direction] ?? 0;
    public static int GetValueOrZeroWithElement(this Cell? cell, Direction direction)
    {
        if (cell is null || cell.Card is null)
            return 0;
        var mod = 0;
        if (cell.Element != Element.None)
            mod = cell.Element != cell.Card.Element ? -1 : 1;
        return cell.Card[direction] + mod;
    }
}