using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TripleTriad.Scenes;

namespace TripleTriad.Components;

public sealed class SceneManagerComponent(
    TripleTriadGame game,
    GraphicsDeviceManager graphicsDeviceManager,
    SpriteBatch spriteBatch,
    InputListenerComponent inputListener)
    : DrawableGameComponent(game)
{
    private readonly Stack<IScene> _scenes = [];
    private Matrix _transform = GetScale(graphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferWidth);

    public IScene ActiveScene => _scenes.Peek();

    public void Push<TScene>() where TScene : IScene =>
        _scenes.Push(game.Services.GetRequiredService<TScene>());

    public IScene Pop() => _scenes.Pop();

    public override void Initialize()
    {
        graphicsDeviceManager.PreparingDeviceSettings += GraphicsDeviceManager_PreparingDeviceSettings;
        Push<GameScene>();
        base.Initialize();
    }

    private static Matrix GetScale(int width) => Matrix.Identity * Matrix.CreateScale(width / 1920f);

    protected override void UnloadContent()
    {
        graphicsDeviceManager.PreparingDeviceSettings -= GraphicsDeviceManager_PreparingDeviceSettings;

        base.UnloadContent();
    }

    private void GraphicsDeviceManager_PreparingDeviceSettings(object? sender, PreparingDeviceSettingsEventArgs e) =>
        _transform = GetScale(e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth);

    public override void Update(GameTime gameTime)
    {
        if (inputListener.KeyboardState.WasKeyPressed(Keys.Escape))
        {
            if (_scenes.Count == 1)
                game.Exit();
            else
                _scenes.Pop();
        }

        if (inputListener.KeyboardState.IsAltDown() && inputListener.KeyboardState.WasKeyPressed(Keys.Enter))
        {
            graphicsDeviceManager.ToggleFullScreen();
        }

        ActiveScene.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        graphicsDeviceManager.GraphicsDevice.Clear(Color.CornflowerBlue);

        spriteBatch.Begin(transformMatrix: _transform);
        ActiveScene.Draw(spriteBatch);
        spriteBatch.End();
    }
}
