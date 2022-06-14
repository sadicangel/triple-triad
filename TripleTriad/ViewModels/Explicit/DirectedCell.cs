using TripleTriad.Models;

namespace TripleTriad.ViewModels.Explicit;

public readonly record struct DirectedCell(Direction Direction, CellViewModel Cell);
