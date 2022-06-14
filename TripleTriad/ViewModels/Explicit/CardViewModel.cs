using Microsoft.UI;
using TripleTriad.Controls;
using TripleTriad.Models;
using Windows.UI;

namespace TripleTriad.ViewModels.Explicit;

public sealed class CardViewModel : BaseViewModel<Card, CardControl>
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

    public Color BorderColor { get => _borderColor; set => SetProperty(ref _borderColor, value); }
    private Color _borderColor = Colors.Black;

    public Color Color { get => _color; set => SetProperty(ref _color, value); }
    private Color _color = Colors.Transparent;

    public int HandIndex { get => _handIndex; set => SetProperty(ref _handIndex, value); }
    private int _handIndex = -1;

    public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value, OnIsSelectedChanged); }
    private bool _isSelected;

    public int this[Direction direction]
    {
        get => direction switch
        {
            Direction.Left => Left,
            Direction.Up => Up,
            Direction.Right => Right,
            Direction.Down => Down,
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
    }

    private void OnIsSelectedChanged(bool isSelected)
    {
        View.IsSelected = isSelected;
    }

    public void RemoveVisualStates() => View.RemoveVisualStates();

    public Task FlipAsync(Direction direction) => View.FlipAsync(direction);

    public Task ColorAsync(Color color) => View.ColorAsync(color);

    public void Hightlight(BoardRules rule) => View.Highlight(rule);
}