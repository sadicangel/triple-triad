using TripleTriad.Components;
using TripleTriad.Util;

namespace TripleTriad.Services;

public sealed class CardDataProvider
{
    private readonly Texture2DAtlas _atlas;
    private readonly CardValues[] _values;
    private readonly CardTextures[] _textures;

    public CardDataProvider(ContentManager contentManager)
    {
        _atlas = CreateAtlas(contentManager);
        _values = contentManager.Load<CardValues[]>("cards.json", new JsonContentLoaderEx());
        _textures = CreateCardTextures(_atlas, _values);
    }

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

    private static CardTextures[] CreateCardTextures(Texture2DAtlas atlas, CardValues[] values)
    {
        return values
            .Select(card => new CardTextures(
                atlas.GetRegion($"card_{card.Number}"),
                atlas.GetRegion("fill_0"),
                atlas.GetRegion("fill_1"),
                atlas.GetRegion($"val_W_{card.W:X1}"),
                atlas.GetRegion($"val_N_{card.N:X1}"),
                atlas.GetRegion($"val_E_{card.E:X1}"),
                atlas.GetRegion($"val_S_{card.S:X1}"),
                atlas.GetRegion($"elem_{card.Element}")))
            .ToArray();
    }

    public CardValues GetValues(int number) => _values[number - 1];

    public CardTextures GetTextures(int number) => _textures[number - 1];
}
