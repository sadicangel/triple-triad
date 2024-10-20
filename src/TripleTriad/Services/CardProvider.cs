using TripleTriad.Core;
using TripleTriad.Sprites;
using TripleTriad.Util;

namespace TripleTriad.Services;

public sealed class CardProvider(ContentManager contentManager)
{
    private readonly Texture2DAtlas _atlas = CreateAtlas(contentManager);
    private readonly Card[] _cards = CreateCards(contentManager);

    private static Texture2DAtlas CreateAtlas(ContentManager contentManager)
    {
        var atlas = Texture2DAtlas.Create("atlas", contentManager.Load<Texture2D>("spritesheet"), 256, 256, 110);
        atlas.CreateRegion("card_back", new Point(0, 10 * 256), new Size(256, 256));
        return atlas;
    }

    private static Card[] CreateCards(ContentManager contentManager)
    {
        return contentManager.Load<Card[]>("cards.json", new JsonContentLoaderEx());
    }

    public CardSprite CreateCard(int number)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(number, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(number, 110);

        var index = number - 1;

        var card = _cards[index] with { };

        var sprite = _atlas.CreateSprite(index);

        sprite.Origin = Vector2.Zero;

        return new CardSprite(sprite, card);
    }
}
