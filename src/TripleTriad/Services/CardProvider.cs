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
        const int Size = 256;
        var atlas = Texture2DAtlas.Create("atlas", contentManager.Load<Texture2D>("spritesheet"), Size, Size, 110);
        atlas.CreateRegion("card_back", new Point(0, 10 * Size), new Size(Size, Size));
        atlas.CreateRegion("card_fill", new Point(Size, 10 * Size), new Size(Size, Size));
        return atlas;
    }

    private static CardData[] CreateCards(ContentManager contentManager)
    {
        return contentManager.Load<CardData[]>("cards.json", new JsonContentLoaderEx());
    }

    public Card CreateCard(int number)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(number, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(number, 110);

        return new Card(_cards[number - 1], _atlas);
    }
}
