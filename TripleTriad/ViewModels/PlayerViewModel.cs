using CommunityToolkit.Mvvm.ComponentModel;
using TripleTriad.Game;

namespace TripleTriad.ViewModels;

public sealed partial class PlayerViewModel : ObservableObject
{
    public required Player Player { get; init; }

    public Color Color => Player is null ? Colors.Transparent : Color.FromUint(Player.Color);
}
