using TripleTriad.Systems;

namespace TripleTriad.Scenes;

public sealed class SceneManager(
    TripleTriadGame game,
    GraphicsDevice graphicsDevice,
    SpriteBatch spriteBatch,
    OrthographicCamera camera)
    : DrawableGameComponent(game)
{
    private readonly Stack<Scene> _scenes = [];

    public Scene ActiveScene => _scenes.Peek();

    public void Push<TScene>() where TScene : Scene =>
        _scenes.Push(game.Services.GetRequiredService<TScene>());

    public Scene Pop() => _scenes.Pop();

    public override void Initialize()
    {
        Push<GameScene>();
        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        Input.Update();

        if (Input.Keyboard.WasKeyPressed(Keys.Escape))
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
        spriteBatch.Begin(/*sortMode: SpriteSortMode.BackToFront, */transformMatrix: camera.GetViewMatrix());
        ActiveScene.Draw(spriteBatch);
        spriteBatch.End();
    }
}
