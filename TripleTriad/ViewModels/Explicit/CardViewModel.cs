using Microsoft.UI;
using TripleTriad.Models;
using Windows.UI;

namespace TripleTriad.ViewModels.Explicit;

public sealed class CardViewModel : BaseViewModel<Card>
{
    public string Id { get => Model.Id; }
    public string Name { get => Model.Name; }
    public int Edition { get => Model.Edition; }
    public int Tier { get => Model.Tier; }
    public int Number { get => Model.Number; }
    public int Version { get => Model.Version; }
    public string ImageUri { get => $"ms-appx://TripleTriad.Shared/{Model.ImageUri}"; }
    public Element Element { get => Model.Element; }
    public int Left { get => Model.Left; }
    public int Up { get => Model.Up; }
    public int Right { get => Model.Right; }
    public int Down { get => Model.Down; }

    public Color Color { get => _borderColor; set => SetProperty(ref _borderColor, value); }
    private Color _borderColor = Colors.Transparent;

    public int HandIndex { get => _handIndex; set => SetProperty(ref _handIndex, value); }
    private int _handIndex = -1;
}