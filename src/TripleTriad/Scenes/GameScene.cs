using TripleTriad.Components;
using TripleTriad.Objects;
using TripleTriad.Services;

namespace TripleTriad.Scenes;

public sealed class GameScene(
    CardProvider cardProvider,
    OrthographicCamera camera,
    InputListenerComponent inputListener)
    : IScene
{
    private readonly Card[] _cards = Enumerable
        .Range(1, 110)
        .Select((n, i) =>
        {
            var card = cardProvider.CreateCard(n);
            card.Position = new Vector2(i % 11 * 256, i / 11 * 256);
            return card;
        })
        .ToArray();

    public void Update(GameTime gameTime)
    {
        const float PixelsPerSecond = 500f;

        var position = camera.Position;
        if (inputListener.KeyboardState.IsKeyDown(Keys.Right))
            position.X += gameTime.GetElapsedSeconds() * PixelsPerSecond;
        if (inputListener.KeyboardState.IsKeyDown(Keys.Left))
            position.X -= gameTime.GetElapsedSeconds() * PixelsPerSecond;
        if (inputListener.KeyboardState.IsKeyDown(Keys.Down))
            position.Y += gameTime.GetElapsedSeconds() * PixelsPerSecond;
        if (inputListener.KeyboardState.IsKeyDown(Keys.Up))
            position.Y -= gameTime.GetElapsedSeconds() * PixelsPerSecond;

        camera.Position = position;

        if (camera.Contains(inputListener.MouseState.Position) is ContainmentType.Contains)
        {
            if (inputListener.MouseState.WasButtonPressed(MouseButton.Left))
            {
                if (_cards.FirstOrDefault(card => card.Border.Contains(inputListener.MouseState.Position)) is Card card)
                {
                    card.Flip180();
                }
            }
        }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        foreach (var card in _cards)
            card.Draw(gameTime, spriteBatch);
    }
}
