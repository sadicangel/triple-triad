using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content;
using MonoGame.Extended.Serialization.Json;
using TripleTriad.Components;
using TripleTriad.Configuration;
using TripleTriad.Scenes;

namespace TripleTriad;
public class TripleTriadGame : Game
{
    public TripleTriadGame()
    {
        var opts = MonoGameJsonSerializerOptionsProvider.GetOptions(Content, "appsettings.json");
        Content.RootDirectory = "Content";
        Configuration = Content.Load<GameConfiguration>("appsettings.json", new GameConfigurationLoader());

        var isFullscreen = Configuration.Window.IsFullscreen;

        var graphicsDeviceManager = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = Configuration.Window.Size.Width,
            PreferredBackBufferHeight = Configuration.Window.Size.Height,
            IsFullScreen = isFullscreen,
        };

        IsMouseVisible = true;

        Services = new ServiceCollection()
            .AddSingleton(this)
            .AddSingleton(Configuration)
            .AddSingleton(graphicsDeviceManager)
            .AddSingleton(provider => provider.GetRequiredService<GraphicsDeviceManager>().GraphicsDevice)
            .AddSingleton(provider => new SpriteBatch(provider.GetRequiredService<GraphicsDevice>()))
            .AddSingleton(Content)
            .AddSingleton<InputListenerComponent>()
            .AddSingleton<SceneManagerComponent>()
            .AddTransient<GameScene>()
            .BuildServiceProvider();
    }

    public new IServiceProvider Services { get; }

    public GameConfiguration Configuration { get; }

    protected override void Initialize()
    {
        Components.Add(Services.GetRequiredService<InputListenerComponent>());
        Components.Add(Services.GetRequiredService<SceneManagerComponent>());

        base.Initialize();
    }
}
