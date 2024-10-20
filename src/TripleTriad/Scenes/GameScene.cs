using TripleTriad.Services;
using TripleTriad.Sprites;

namespace TripleTriad.Scenes;

public sealed class GameScene(
    SpriteBatch spriteBatch,
    CardProvider cardProvider)
    : IScene
{
    private readonly CardSprite[] _sprites = Enumerable.Range(1, 110).Select(cardProvider.CreateCard).ToArray();

    public void Update(GameTime gameTime)
    {

    }

    public void Draw(SpriteBatch gameTime)
    {
        for (var i = 0; i < _sprites.Length; ++i)
        {
            spriteBatch.Draw(_sprites[i].Sprite, new Vector2(i % 11 * 256, i / 11 * 256));
        }
    }
}
