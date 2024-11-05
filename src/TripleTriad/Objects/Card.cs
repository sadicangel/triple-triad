using TripleTriad.Animations;
using TripleTriad.Core;

namespace TripleTriad.Objects;

public sealed class Card : IAnimationTarget
{
    private readonly CardData _data;
    private readonly Texture2DAtlas _atlas;

    private readonly Texture2DRegion _card;
    private readonly Texture2DRegion _fill;
    private readonly Texture2DRegion _back;

    private readonly Texture2DRegion _w;
    private readonly Texture2DRegion _n;
    private readonly Texture2DRegion _e;
    private readonly Texture2DRegion _s;

    private readonly Texture2DRegion _t;

    private readonly FlipAnimation<Card> _flipAnimation;

    public Card(CardData data, Texture2DAtlas atlas)
    {
        _data = data;
        _atlas = atlas;

        _card = _atlas.GetRegion($"card_{_data.Number}");
        _back = _atlas.GetRegion("fill_0");
        _fill = _atlas.GetRegion("fill_1");

        _w = _atlas.GetRegion($"val_W_{_data.W:X1}");
        _n = _atlas.GetRegion($"val_N_{_data.N:X1}");
        _e = _atlas.GetRegion($"val_E_{_data.E:X1}");
        _s = _atlas.GetRegion($"val_S_{_data.S:X1}");
        _t = _atlas.GetRegion($"elem_{_data.Element}");

        // TODO: Probably assign this through an enum instead.
        Color = (_data.Number % 2 == 0 ? Color.DarkRed : Color.DarkBlue) with { A = 64 };

        _flipAnimation = new FlipAnimation<Card>(this);
    }

    public Vector2 Position { get; set; }

    public Vector2 Scale { get; set; } = Vector2.One;

    public Color Color { get; set; }

    public Vector2 Origin => new(_fill.Size.Width * .5f, _fill.Size.Height * .5f);

    public float LayerDepth { get; set; } = 0f;

    public Rectangle Border => _fill.Bounds with { Location = Position.ToPoint() };

    public bool IsFlipped => _flipAnimation.IsFlipped;

    public void Flip180() => _flipAnimation.Flip180();

    public void Flip360() => _flipAnimation.Flip360();

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _flipAnimation.Update(gameTime);

        var position = Position + Origin;

        if (IsFlipped is false)
        {
            spriteBatch.Draw(_fill, position, Color, 0f, Origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_card, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_w, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_n, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_e, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_s, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_t, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
        }
        else
        {
            spriteBatch.Draw(_back, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
        }
    }
}
