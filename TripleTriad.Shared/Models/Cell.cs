namespace TripleTriad.Models;

public sealed partial class Cell
{
    public int Row { get => Index / 3; }
    public int Column { get => Index % 3; }
    public bool HasCard { get => Card is not null; }
}