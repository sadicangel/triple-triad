using TripleTriad.Models;

namespace TripleTriad.ViewModels.Explicit
{
    public static class CellViewModelExtensions
    {
        public static int ValueOrZero(this CellViewModel? x, Direction direction) => x?.Card?[direction] ?? 0;
        public static int ValueOrZeroWithElement(this CellViewModel? x, Direction direction)
        {
            if (x is null || x.Card is null)
                return 0;
            var mod = 0;
            if (x.Element != Element.None)
                mod = x.Element != x.Card.Element ? -1 : 1;
            return x.Card[direction] + mod;
        }
    }
}
