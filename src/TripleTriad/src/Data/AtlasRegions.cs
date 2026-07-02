namespace TripleTriad.Data;

public readonly record struct TileRegion(int X, int Y, int Width, int Height);

public static class AtlasRegions
{
    public const int TileSize = 256;
    public const int Columns = 11;

    public static TileRegion CardFace(int cardNumber)
    {
        if (cardNumber is < 1 or > 110)
            throw new ArgumentOutOfRangeException(nameof(cardNumber), cardNumber, "Card numbers are 1-110.");

        var index = cardNumber - 1;
        return Tile(index % Columns, index / Columns);
    }

    public static TileRegion Fill(int fillIndex)
    {
        if (fillIndex is < 0 or > 10)
            throw new ArgumentOutOfRangeException(nameof(fillIndex), fillIndex, "Fill atlas columns are 0-10.");

        return Tile(fillIndex, 10);
    }

    public static TileRegion Value(Direction direction, int value)
    {
        if (value is < 0 or > 10)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Rank atlas columns are 0-10.");

        var row = direction switch
        {
            Direction.West => 11,
            Direction.North => 12,
            Direction.East => 13,
            Direction.South => 14,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };

        return Tile(value, row);
    }

    public static TileRegion Element(Element element) => Tile((int)element, 15);

    private static TileRegion Tile(int column, int row) =>
        new(column * TileSize, row * TileSize, TileSize, TileSize);
}
