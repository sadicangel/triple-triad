using TripleTriad.Core;

namespace TripleTriad.Sprites;

public sealed record class CardSprite(Sprite Sprite, Card Card)
{
    public Vector2 Position { get; set; }

    public Color Color { get; set; } = Color.DarkBlue with { A = 64 };

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawRectangle(Position, Sprite.TextureRegion.Bounds.Size, Color);
        spriteBatch.Draw(Sprite, Position);
    }
}
