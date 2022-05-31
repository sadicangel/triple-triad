namespace TripleTriad.ViewModels.Explicit;

public sealed record class NeighbourCells(CellViewModel? Left, CellViewModel? Up, CellViewModel? Right, CellViewModel? Down);
