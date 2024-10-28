using TripleTriad.Core;

namespace TripleTriad.Objects;

public sealed class Card
{
    private readonly CardData _data;
    private readonly Texture2DAtlas _atlas;

    private readonly Texture2DRegion _card;
    private readonly Texture2DRegion _fill;
    private readonly Texture2DRegion _back;

    private readonly Texture2DRegion _left;
    private readonly Texture2DRegion _up;
    private readonly Texture2DRegion _right;
    private readonly Texture2DRegion _down;

    private readonly Texture2DRegion _element;

    public Card(CardData data, Texture2DAtlas atlas)
    {
        _data = data;
        _atlas = atlas;

        _card = _atlas.GetRegion(_data.Number - 1);
        _fill = _atlas.GetRegion("card_fill");
        _back = _atlas.GetRegion("card_back");

        _left = _atlas.GetRegion($"number_{_data.Left:X1}");
        _up = _atlas.GetRegion($"number_{_data.Up:X1}");
        _right = _atlas.GetRegion($"number_{_data.Right:X1}");
        _down = _atlas.GetRegion($"number_{_data.Down:X1}");

        _element = _atlas.GetRegion($"element_{(int)_data.Element:X1}");

        // TODO: Probably assign this through an enum instead.
        Color = (_data.Number % 2 == 0 ? Color.DarkRed : Color.DarkBlue) with { A = 64 };
    }

    public Vector2 Position { get; set; }

    public Color Color { get; set; }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_fill, Position, Color);
        spriteBatch.Draw(_card, Position, Color.White);
        spriteBatch.Draw(_left, Position + new Vector2(16, 48), Color.White);
        spriteBatch.Draw(_up, Position + new Vector2(32, 16), Color.White);
        spriteBatch.Draw(_right, Position + new Vector2(48, 48), Color.White);
        spriteBatch.Draw(_down, Position + new Vector2(32, 80), Color.White);
        spriteBatch.Draw(_element, Position + new Vector2(200, 24), Color.White);
    }
}
