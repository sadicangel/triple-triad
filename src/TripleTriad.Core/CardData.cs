using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace TripleTriad.Core;

public sealed record class CardData(
    int Edition,
    int Number,
    int Tier,
    Element Element,
    int W,
    int N,
    int E,
    int S)
{
    public Guid Guid { get; init; } = CardId.Create(Edition, Number);
}

file static class CardId
{
    private static readonly Guid s_namespace = Guid.Parse("435A4346-52AC-49D8-8507-04B3B46A54DF");

    public static Guid Create(int edition, int number)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(edition);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);

        Span<byte> data = stackalloc byte[16 + 4 + 4]; // namespace + edition + number

        if (!s_namespace.TryWriteBytes(data, bigEndian: true, out _))
            throw new UnreachableException();
        if (!MemoryMarshal.TryWrite(data[16..], in edition))
            throw new UnreachableException();
        if (!MemoryMarshal.TryWrite(data[20..], in number))
            throw new UnreachableException();

        Span<byte> hash = stackalloc byte[20];
        SHA1.HashData(data, hash);

        hash[6] = (byte)((hash[6] & 0x0F) | (5 << 4));
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        return new Guid(hash[..16], bigEndian: true);
    }
}

