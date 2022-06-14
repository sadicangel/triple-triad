using TripleTriad.Models;
using Windows.UI;

namespace TripleTriad.ViewModels.Explicit;

public sealed class PlayerViewModel : BaseViewModel<Player>, IEquatable<PlayerViewModel>
{
    public string Name { get => Model.Name; set => SetProperty(m => m.Name, (m, v) => m.Name = v, value); }

    public Color Color { get => _color; set => SetProperty(ref _color, value); }
    private Color _color;

    public PlayerViewModel() => Model = new Player();

    public bool Equals(PlayerViewModel? other) => other is not null && Name == other.Name && _color == other._color;

    public override bool Equals(object? obj) => Equals(obj as PlayerViewModel);

    public override int GetHashCode() => HashCode.Combine(Name, _color);
    public static bool operator ==(PlayerViewModel? left, PlayerViewModel? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(PlayerViewModel? left, PlayerViewModel? right) => !(left == right);
}