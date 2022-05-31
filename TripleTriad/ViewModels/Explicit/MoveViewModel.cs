using TripleTriad.Models;

namespace TripleTriad.ViewModels.Explicit;

public sealed class MoveViewModel : BaseViewModel<Move>
{
    public PlayerViewModel Player { get; }
    public CardViewModel Card { get; }

    public MoveViewModel(PlayerViewModel player, CardViewModel card)
    {
        Model = new Move
        {
            Player = player.Model,
            Card = card.Model,
            Index = card.HandIndex,
        };
        Player = player;
        Card = card;
    }
}