using TripleTriad.Scenes;

namespace TripleTriad.Components;

public sealed class SceneManagerComponent(
    TripleTriadGame game,
    GraphicsDevice graphicsDevice,
    SpriteBatch spriteBatch,
    OrthographicCamera camera,
    InputListenerComponent inputListener)
    : DrawableGameComponent(game)
{
    private readonly Stack<IScene> _scenes = [];

    public IScene ActiveScene => _scenes.Peek();

    public void Push<TScene>() where TScene : IScene =>
        _scenes.Push(game.Services.GetRequiredService<TScene>());

    public IScene Pop() => _scenes.Pop();

    public override void Initialize()
    {
        Push<GameScene>();
        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        if (inputListener.KeyboardState.WasKeyPressed(Keys.Escape))
        {
            if (_scenes.Count == 1)
                game.Exit();
            else
                _scenes.Pop();
        }

        ActiveScene.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        graphicsDevice.Clear(Color.Plum);
        spriteBatch.Begin(transformMatrix: camera.GetViewMatrix());
        ActiveScene.Draw(gameTime, spriteBatch);
        spriteBatch.End();
    }
}
