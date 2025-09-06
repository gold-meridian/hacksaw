using System;

namespace Tomat.Hacksaw.Metadata.Image;

/// <summary>
///     A wrapper over a collection of bytes in memory which may be reliably
///     used for equality checks.
/// </summary>
public readonly struct ByteCollection(ReadOnlyMemory<byte> value) : IEquatable<ByteCollection>
{
    public ReadOnlyMemory<byte> Value => value;

    public bool Equals(ByteCollection other)
    {
        return Value.Span.SequenceEqual(other.Value.Span);
    }

    public override bool Equals(object? obj)
    {
        return obj is ByteCollection other && Equals(other);
    }

    public override int GetHashCode()
    {
        // Doesn't need to be unique!  Length can vary greatly, so this should
        // somewhat reliably split data up (and it's fast).
        return Value.Length;
    }

    public static bool operator ==(ByteCollection left, ByteCollection right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ByteCollection left, ByteCollection right)
    {
        return !(left == right);
    }
}