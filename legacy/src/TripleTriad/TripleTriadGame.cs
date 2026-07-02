using MonoGame.Extended.ViewportAdapters;
using TripleTriad.Configuration;
using TripleTriad.Scenes;
using TripleTriad.Services;
using TripleTriad.Util;

namespace TripleTriad;
public class TripleTriadGame : Game
{
    public TripleTriadGame()
    {
        var opts = MonoGameJsonSerializerOptionsProvider.GetOptions(Content, "appsettings.json");
        Content.RootDirectory = "Content";
        Configuration = Content.Load<GameConfiguration>("appsettings.json", new JsonContentLoaderEx());

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
            .AddSingleton(Window)
            .AddSingleton(graphicsDeviceManager)
            .AddSingleton(provider => provider.GetRequiredService<GraphicsDeviceManager>().GraphicsDevice)
            .AddSingleton(provider => new SpriteBatch(provider.GetRequiredService<GraphicsDevice>()))
            .AddSingleton<ViewportAdapter>(provider => new BoxingViewportAdapter(
                window: provider.GetRequiredService<GameWindow>(),
                graphicsDevice: provider.GetRequiredService<GraphicsDevice>(),
                virtualWidth: 1440,
                virtualHeight: 810))
            .AddSingleton(provider => new OrthographicCamera(provider.GetRequiredService<ViewportAdapter>()))
            .AddSingleton(Content)
            .AddSingleton<CardDataProvider>()
            .AddSingleton<SceneManager>()
            .AddTransient<GameScene>()
            .BuildServiceProvider();
    }

    public new IServiceProvider Services { get; }

    public GameConfiguration Configuration { get; }

    protected override void Initialize()
    {
        Components.Add(Services.GetRequiredService<SceneManager>());

        base.Initialize();
    }
}
