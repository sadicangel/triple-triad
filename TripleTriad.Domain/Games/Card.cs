using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using TripleTriad.Interfaces;

namespace TripleTriad.Games;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class Card : IEntity<Guid>
{
    public Guid Id { get; init; }

    public int this[Direction direction] { get => GetDirectionValue(direction); }

    public required string Name { get; init; }

    public required int Edition { get; init; }

    public required int Tier { get; init; }

    public required int Number { get; init; }

    public required int Version { get; init; }

    public required Element Element { get; init; }

    public required int Left { get; init; }

    public required int Up { get; init; }

    public required int Right { get; init; }

    public required int Down { get; init; }

    public required string Image { get; init; }

    public int GetDirectionValue(Direction direction) => direction switch
    {
        Direction.Left => Left,
        Direction.Up => Up,
        Direction.Right => Right,
        Direction.Down => Down,
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };

    private string GetDebuggerDisplay() => $"{Name} ({Left:X1}, {Up:X1}, {Right:X1}, {Down:X1}) {Element}";

    public static Guid NewId(int edition, int tier, int number, int version)
    {
        Span<byte> source = stackalloc byte[16];
        MemoryMarshal.Write(source, ref edition);
        MemoryMarshal.Write(source[4..], ref tier);
        MemoryMarshal.Write(source[8..], ref number);
        MemoryMarshal.Write(source[12..], ref version);
        Span<byte> target = stackalloc byte[16];
        MD5.HashData(source, target);
        return new Guid(target);
    }
}
