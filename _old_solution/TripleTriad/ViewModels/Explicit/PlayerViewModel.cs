using TripleTriad.Extensions;
using TripleTriad.Models;
using Windows.UI;

namespace TripleTriad.ViewModels.Explicit;

public sealed class PlayerViewModel : BaseViewModel<Player>, IEquatable<PlayerViewModel>
{
    public string Name { get => Model.Name; set => SetProperty(m => m.Name, (m, v) => m.Name = v, value); }

    public Color Color { get => Model.Color.ToColor(); set => SetProperty(m => m.Color.ToColor(), (m, v) => m.Color = v.ToUint32(), value); }

    public bool IsLeft { get => Model.IsLeft; set => SetProperty(m => m.IsLeft, (m, v) => m.IsLeft = v, value); }

    public PlayerViewModel() => Model = new Player();

    public bool Equals(PlayerViewModel? other) => other is not null && Model.Equals(other.Model);

    public override bool Equals(object? obj) => Equals(obj as PlayerViewModel);

    public override int GetHashCode() => HashCode.Combine(Name, Color);
    public static bool operator ==(PlayerViewModel? left, PlayerViewModel? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(PlayerViewModel? left, PlayerViewModel? right) => !(left == right);
}