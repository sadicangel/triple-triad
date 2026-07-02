using TripleTriad.Components;

namespace TripleTriad.Systems;

public static class Renderer
{
    public static void Render(SpriteBatch spriteBatch, ref CardTextures textures, ref CardState state, ref Transform transform)
    {
        var position = transform.Position + textures.Origin;

        if (state.IsFlipped is false)
        {
            spriteBatch.Draw(textures.Fill, position, state.Color, transform.Rotation, textures.Origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f);
            spriteBatch.Draw(textures.Card, position, Color.White, transform.Rotation, textures.Origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - .01f);
            spriteBatch.Draw(textures.W, position, Color.White, transform.Rotation, textures.Origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - .02f);
            spriteBatch.Draw(textures.N, position, Color.White, transform.Rotation, textures.Origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - .02f);
            spriteBatch.Draw(textures.E, position, Color.White, transform.Rotation, textures.Origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - .02f);
            spriteBatch.Draw(textures.S, position, Color.White, transform.Rotation, textures.Origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - .02f);
            spriteBatch.Draw(textures.Element, position, Color.White, transform.Rotation, textures.Origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - .02f);
            if (state.IsHighlighted)
                spriteBatch.Draw(textures.Fill, position, state.Color with { A = 128 }, transform.Rotation, textures.Origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f - 0.005f);
        }
        else
        {
            spriteBatch.Draw(textures.Back, position, Color.White, transform.Rotation, textures.Origin, transform.Scale, SpriteEffects.None, layerDepth: 0.5f);
        }
    }
}
