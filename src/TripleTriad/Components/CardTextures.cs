namespace TripleTriad.Components;

public sealed record class CardTextures(
    Texture2DRegion Card,
    Texture2DRegion Back,
    Texture2DRegion Fill,
    Texture2DRegion W,
    Texture2DRegion N,
    Texture2DRegion E,
    Texture2DRegion S,
    Texture2DRegion Element)
{
    public Vector2 Origin { get; } = new(Card.Size.Width * .5f, Card.Size.Height * .5f);
}
