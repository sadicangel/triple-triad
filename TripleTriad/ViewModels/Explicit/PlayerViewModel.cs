using TripleTriad.Models;
using Windows.UI;

namespace TripleTriad.ViewModels.Explicit;

public sealed class PlayerViewModel : BaseViewModel<Player>
{
    public string Name { get => Model.Name; }

    public Color Color { get => _color; set => SetProperty(ref _color, value); }
    private Color _color;
}