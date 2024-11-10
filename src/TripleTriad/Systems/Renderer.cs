using TripleTriad.Components;

namespace TripleTriad.Systems;

public static class Renderer
{
    public static void Render(SpriteBatch spriteBatch, ref Card card, ref Transform transform, ref Color color)
    {
        var origin = GetOrigin(card.Textures.Card);
        var position = transform.Position + origin;

        //if (IsFlipped is false)
        //{
        spriteBatch.Draw(card.Textures.Fill, position, color, transform.Rotation, origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f);
        spriteBatch.Draw(card.Textures.Card, position, Color.White, transform.Rotation, origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - .01f);
        spriteBatch.Draw(card.Textures.W, position, Color.White, transform.Rotation, origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - .02f);
        spriteBatch.Draw(card.Textures.N, position, Color.White, transform.Rotation, origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - .02f);
        spriteBatch.Draw(card.Textures.E, position, Color.White, transform.Rotation, origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - .02f);
        spriteBatch.Draw(card.Textures.S, position, Color.White, transform.Rotation, origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - .02f);
        spriteBatch.Draw(card.Textures.Element, position, Color.White, transform.Rotation, origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - .02f);
        //if (IsHighlighted)
        //    spriteBatch.Draw(card.Textures.Fill, position, Color with { A = 128 }, transform.Rotation, origin, transform.Scale, SpriteEffects.None, LayerDepth - 0.005f);
        //}
        //else
        //{
        //    spriteBatch.Draw(card.Textures.Back, position, Color.White, transform.Rotation, origin, transform.Scale, SpriteEffects.None, LayerDepth);
        //}

        static Vector2 GetOrigin(Texture2DRegion region) => new(region.Size.Width * .5f, region.Size.Height * .5f);
    }
}
