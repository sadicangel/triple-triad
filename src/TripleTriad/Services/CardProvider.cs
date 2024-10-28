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
        const int CardSz = 256;
        var atlas = Texture2DAtlas.Create("atlas", contentManager.Load<Texture2D>("spritesheet"), CardSz, CardSz, 110);
        atlas.CreateRegion("card_back", new Point(0 * CardSz, 10 * CardSz), new Size(CardSz, CardSz));
        atlas.CreateRegion("card_fill", new Point(1 * CardSz, 10 * CardSz), new Size(CardSz, CardSz));

        const int ElemSz = 32;
        var numOffset = new Point(2 * CardSz, 10 * CardSz);
        for (var i = 0; i <= 10; ++i)
            atlas.CreateRegion($"number_{i:X1}", numOffset + new Point(i % 8 * ElemSz, i / 8 * ElemSz), new Size(ElemSz, ElemSz));

        var elemOffset = new Point(3 * CardSz, 10 * CardSz);
        foreach (var element in Enum.GetValues<Element>())
            atlas.CreateRegion($"element_{(int)element:X1}", elemOffset + new Point((int)element % 8 * ElemSz, (int)element / 8 * ElemSz), new Size(ElemSz, ElemSz));
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
