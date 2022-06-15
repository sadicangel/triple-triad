using Windows.UI;

namespace TripleTriad.Extensions;

public static class ColorExtensions
{
    public static uint ToUint32(this Color color)
    {
        Span<byte> bytes = stackalloc byte[4]
        {
            color.A,
            color.R,
            color.G,
            color.B,
        };

        return BitConverter.ToUInt32(bytes);
    }

    public static Color ToColor(this uint color)
    {
        Span<byte> bytes = stackalloc byte[4];
        BitConverter.TryWriteBytes(bytes, color);
        return Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
    }
}