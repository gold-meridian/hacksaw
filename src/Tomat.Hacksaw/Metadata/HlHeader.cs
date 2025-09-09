using System;
using System.IO;

using Tomat.Hacksaw.IO;

namespace Tomat.Hacksaw.Metadata;

public readonly struct HlHeader(ReadOnlyMemory<byte> value) : IEquatable<HlHeader>
{
    public static readonly HlHeader HLB = new("HLB"u8.ToArray());

    public ReadOnlyMemory<byte> Value { get; } = value;

    // TODO: Write
    public static unsafe HlHeader Read<TByteReader>(ref TByteReader reader, HlHeader? searchHeader = null)
        where TByteReader : IByteReader, allows ref struct
    {
        var headerSize = searchHeader.HasValue ? searchHeader.Value.Value.Length : 3;

        var headerBytes = (Span<byte>)stackalloc byte[headerSize];

        if (searchHeader.HasValue)
        {
            // If we are searching for a specific header then we should analyze
            // all the data available to us.

            while (true)
            {
                while (reader.ReadBytes(headerBytes) == headerSize)
                {
                    if (headerBytes.SequenceEqual(searchHeader.Value.Value.Span))
                    {
                        return new HlHeader(headerBytes.ToArray());
                    }

                    reader.Position -= headerSize - 1;
                }

                throw new InvalidDataException("Could not find expected header");
            }
        }

        // Just assume the immediate data read is the header.  Return
        // whatever data we get (invalid headers should be handled by
        // calling code).
        if (reader.ReadBytes(headerBytes) != headerSize)
        {
            throw new InvalidDataException("Could not read header (not enough bytes)");
        }

        return new HlHeader(headerBytes.ToArray());
    }

    public bool Equals(HlHeader other)
    {
        return Value.Span.SequenceEqual(other.Value.Span);
    }

    public override bool Equals(object? obj)
    {
        return obj is HlHeader other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(HlHeader left, HlHeader right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HlHeader left, HlHeader right)
    {
        return !(left == right);
    }
}