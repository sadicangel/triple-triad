using CommunityToolkit.Mvvm.ComponentModel;
using TripleTriad.Game;

namespace TripleTriad.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    public CardViewModel Card { get; } = new CardViewModel
    {
        Card = new Card
        {
            Id = Guid.NewGuid(),
            Name = "Shiva",
            Edition = 1,
            Tier = 8,
            Number = 84,
            Version = 1,
            Element = Game.Element.Ice,
            Left = 9,
            Up = 6,
            Right = 7,
            Down = 4,
            Image = "card_01_08_084_1.png"
        }
    };
}
