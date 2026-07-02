//using TripleTriad.Services;

//namespace TripleTriad.Objects;
//public sealed class Board
//{
//    public Board(CardProvider cardProvider, List<Card> leftHand, List<Card> rightHand)
//    {
//        const int HandSize = 5;

//        ArgumentOutOfRangeException.ThrowIfNotEqual(leftHand.Count, HandSize);
//        ArgumentOutOfRangeException.ThrowIfNotEqual(rightHand.Count, HandSize);

//        LeftHand = leftHand;
//        RightHand = rightHand;
//        Cells = [
//            cardProvider.CreateCell(new(336, 24)), cardProvider.CreateCell(new(592, 24)), cardProvider.CreateCell(new(848, 24)),
//            cardProvider.CreateCell(new(336, 280)), cardProvider.CreateCell(new(592, 280)), cardProvider.CreateCell(new(848, 280)),
//            cardProvider.CreateCell(new(336, 536)), cardProvider.CreateCell(new(592, 536)), cardProvider.CreateCell(new(848, 536)),
//        ];

//        for (var i = 0; i < HandSize; ++i)
//        {
//            var y = i * 128 + 24;
//            leftHand[i].Position = new Vector2(24, y);
//            leftHand[i].LayerDepth = 1f - i * .1f;
//            leftHand[i].Color = Color.DarkRed;

//            rightHand[i].Position = new Vector2(1440 - 256 - 24, y);
//            rightHand[i].LayerDepth = 1f - i * .1f;
//            rightHand[i].Color = Color.DarkBlue;
//        }
//    }

//    public List<Card> LeftHand { get; }
//    public List<Card> RightHand { get; }

//    public Cell[] Cells { get; }

//    public void Draw(SpriteBatch spriteBatch)
//    {
//        foreach (var cell in Cells)
//            cell.Draw(spriteBatch);
//        foreach (var card in LeftHand)
//            card.Draw(spriteBatch);
//        foreach (var card in RightHand)
//            card.Draw(spriteBatch);
//    }
//}

//public sealed class Cell(Texture2DAtlas atlas)
//{
//    private readonly Texture2DRegion _fill = atlas.GetRegion("fill_2");

//    public Card? Card { get; set; }

//    public Vector2 Position { get; set; }

//    public Vector2 Origin => new(_fill.Size.Width * .5f, _fill.Size.Height * .5f);

//    public float LayerDepth { get; set; } = 1f;

//    public Rectangle Border => _fill.Bounds with { Location = Position.ToPoint() };

//    public void Draw(SpriteBatch spriteBatch)
//    {
//        var position = Position + Origin;
//        spriteBatch.Draw(_fill, position, Color.Wheat, 0f, Origin, Vector2.One, SpriteEffects.None, LayerDepth);
//        Card?.Draw(spriteBatch);

//    }
//}
