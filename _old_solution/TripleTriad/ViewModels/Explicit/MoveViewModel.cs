using TripleTriad.Models;

namespace TripleTriad.ViewModels.Explicit;

public sealed class MoveViewModel : BaseViewModel<Move>
{
    public PlayerViewModel Player { get; }
    public CardViewModel Card { get; }

    public int HandIndex { get => Model.HandIndex; set => SetProperty(m => m.HandIndex, (m, v) => m.HandIndex = v, value); }

    public int CellIndex { get => Model.CellIndex; set => SetProperty(m => m.CellIndex, (m, v) => m.CellIndex = v, value); }

    public MoveViewModel(PlayerViewModel player, CardViewModel card)
    {
        Model = new Move
        {
            Card = card.Model,
            HandIndex = card.HandIndex,
        };
        Player = player;
        Card = card;
    }
}