using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace TripleTriad;
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics = null!;
    private SpriteBatch _spriteBatch = null!;

    private Texture2DAtlas _atlas = null!;
    private readonly Sprite[] _sprites = new Sprite[110];
    private readonly Matrix _transform = Matrix.Identity * Matrix.CreateScale(0.5f);

    public Game1()
    {
        Content.RootDirectory = "Content";
        _graphics = new GraphicsDeviceManager(this)
        {
            //PreferredBackBufferWidth = 1920,
            PreferredBackBufferWidth = 960,
            //PreferredBackBufferHeight = 1080,
            PreferredBackBufferHeight = 540,
        };
        _graphics.ApplyChanges();

        Window.IsBorderless = false;
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _atlas = Texture2DAtlas.Create("atlas", Content.Load<Texture2D>("spritesheet"), 256, 256, 110);
        _atlas.CreateRegion("card_back", new Point(0, 10 * 256), new Size(256, 256));
        _atlas.CreateRegion("card_front", new Point(256, 10 * 256), new Size(256, 256));

        for (var i = 0; i < _sprites.Length; ++i)
        {
            _sprites[i] = _atlas.CreateSprite(i);
            _sprites[i].Origin = new Vector2(0, 0);
        }

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(transformMatrix: _transform);
        for (var i = 0; i < _sprites.Length; ++i)
        {
            _spriteBatch.Draw(_atlas["card_front"], new Vector2(i % 11 * 256, i / 11 * 256), (i % 2 == 0 ? Color.DarkBlue : Color.DarkRed) with { A = 64 });
            _spriteBatch.Draw(_sprites[i], new Vector2(i % 11 * 256, i / 11 * 256));
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
