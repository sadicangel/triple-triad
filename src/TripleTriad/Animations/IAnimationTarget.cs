namespace TripleTriad.Animations;

public interface IAnimationTarget
{
    Vector2 Position { get; set; }
    Vector2 Scale { get; set; }
    Color Color { get; set; }
}
