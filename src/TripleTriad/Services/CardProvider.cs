using TripleTriad.Core;
using TripleTriad.Objects;
using TripleTriad.Util;

namespace TripleTriad.Services;

public sealed class CardProvider(ContentManager contentManager)
{
    private readonly Texture2DAtlas _atlas = CreateAtlas(contentManager);
    private readonly CardData[] _cards = CreateCards(contentManager);

    private static Texture2DAtlas CreateAtlas(ContentManager contentManager)
    {
        var size = new Point(256);
        var atlas = new Texture2DAtlas("atlas", contentManager.Load<Texture2D>("spritesheet"));
        for (var j = 0; j < 10; ++j)
            for (var i = 0; i < 11; ++i)
                atlas.CreateRegion(new Rectangle(new(size.X * i, size.Y * j), size), $"card_{j * 11 + i + 1}");

        for (var j = 10; j < 11; ++j)
            for (var i = 0; i < 11; ++i)
                atlas.CreateRegion(new Rectangle(new(size.X * i, size.Y * j), size), $"fill_{i}");

        for (var j = 11; j < 15; ++j)
            for (var i = 0; i < 11; ++i)
                atlas.CreateRegion(new Rectangle(new(size.X * i, size.Y * j), size), $"val_{(Direction)j - 11}_{i:X1}");

        for (var j = 15; j < 16; ++j)
            for (var i = 0; i < 11; ++i)
                atlas.CreateRegion(new Rectangle(new(size.X * i, size.Y * j), size), $"elem_{(Element)i}");

        return atlas;
    }

    private static CardData[] CreateCards(ContentManager contentManager)
    {
        return contentManager.Load<CardData[]>("cards.json", new JsonContentLoaderEx());
    }

    public Card CreateCard(int number, Vector2 position)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(number, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(number, 110);

        return new Card(_cards[number - 1], _atlas)
        {
            Position = position
        };
    }

    public Cell CreateCell(Vector2 position)
    {
        return new Cell(_atlas)
        {
            Position = position
        };
    }
}
