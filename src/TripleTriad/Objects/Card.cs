using TripleTriad.Core;

namespace TripleTriad.Objects;

public sealed class Card
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
    }

    public Vector2 Position { get; set; }

    public Vector2 Scale { get; set; } = Vector2.One;

    public Color Color { get; set; }

    public Vector2 Origin => new(_fill.Size.Width * .5f, _fill.Size.Height * .5f);

    public float LayerDepth { get; set; } = 1f;

    public Rectangle Border => _fill.Bounds with { Location = Position.ToPoint() };

    public bool IsInUse { get; set; }

    public bool IsFlipped { get; set; }

    public bool IsHighlighted { get; set; }

    public void Draw(SpriteBatch spriteBatch)
    {
        var position = Position + Origin;

        if (IsFlipped is false)
        {
            spriteBatch.Draw(_fill, position, Color, 0f, Origin, Scale, SpriteEffects.None, LayerDepth);
            spriteBatch.Draw(_card, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, LayerDepth - .01f);
            spriteBatch.Draw(_w, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, LayerDepth - .02f);
            spriteBatch.Draw(_n, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, LayerDepth - .02f);
            spriteBatch.Draw(_e, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, LayerDepth - .02f);
            spriteBatch.Draw(_s, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, LayerDepth - .02f);
            spriteBatch.Draw(_t, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, LayerDepth - .02f);
            if (IsHighlighted)
                spriteBatch.Draw(_fill, position, Color with { A = 128 }, 0f, Origin, Scale, SpriteEffects.None, LayerDepth - 0.005f);
        }
        else
        {
            spriteBatch.Draw(_back, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, LayerDepth);
        }
    }
}
