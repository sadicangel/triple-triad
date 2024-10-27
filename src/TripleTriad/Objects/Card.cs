using TripleTriad.Core;

namespace TripleTriad.Objects;

public sealed class Card
{
    private readonly CardData _data;
    private readonly Texture2DAtlas _atlas;

    private readonly Texture2DRegion _card;
    private readonly Texture2DRegion _fill;
    private readonly Texture2DRegion _back;

    public Card(CardData data, Texture2DAtlas atlas)
    {
        _data = data;
        _atlas = atlas;

        _card = _atlas.GetRegion(_data.Number - 1);
        _fill = _atlas.GetRegion("card_fill");
        _back = _atlas.GetRegion("card_back");

        Color = (data.Number % 2 == 0 ? Color.DarkRed : Color.DarkBlue) with { A = 64 };
    }

    public Vector2 Position { get; set; }

    public Color Color { get; set; }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_fill, Position, Color);
        spriteBatch.Draw(_card, Position, Color.White);
    }
}
