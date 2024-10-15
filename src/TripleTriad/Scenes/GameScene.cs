using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace TripleTriad.Scenes;
public sealed class GameScene(
    SpriteBatch spriteBatch,
    ContentManager contentManager)
    : IScene
{
    private readonly Sprite[] _sprites = CreateSprites(contentManager);

    private static Sprite[] CreateSprites(ContentManager contentManager)
    {
        var atlas = Texture2DAtlas.Create("atlas", contentManager.Load<Texture2D>("spritesheet"), 256, 256, 110);
        atlas.CreateRegion("card_back", new Point(0, 10 * 256), new Size(256, 256));
        atlas.CreateRegion("card_front", new Point(256, 10 * 256), new Size(256, 256));

        var sprites = new Sprite[110];

        for (var i = 0; i < sprites.Length; ++i)
        {
            sprites[i] = atlas.CreateSprite(i);
            sprites[i].Origin = new Vector2(0, 0);
        }

        return sprites;
    }

    public void Update(GameTime gameTime)
    {

    }

    public void Draw(SpriteBatch gameTime)
    {
        for (var i = 0; i < _sprites.Length; ++i)
        {
            spriteBatch.Draw(_sprites[i], new Vector2(i % 11 * 256, i / 11 * 256));
        }
    }
}
