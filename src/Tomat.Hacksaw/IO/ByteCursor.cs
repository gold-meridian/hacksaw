using System.Buffers.Binary;

namespace Tomat.Hacksaw.IO;

/// <summary>
///     Allocation-free primitive parser (stack-friendly).
/// </summary>
public ref struct ByteCursor(ReadOnlySpan<byte> span, long offset)
{
    /// <summary>
    ///     Cursor offset in the byte source.
    /// </summary>
    public long Offset { get; private set; } = offset;

    /// <summary>
    ///     Pointer to the current rented buffer.
    /// </summary>
    public ReadOnlySpan<byte> Span { get; private set; } = span;

    public bool TryReadUInt32(out uint value)
    {
        if (Span.Length < 4)
        {
            value = 0;
            return false;
        }

        value = BinaryPrimitives.ReadUInt32LittleEndian(Span);
        Span = Span[4..];
        Offset += 4;
        return true;
    }

    public bool TryReadInt32(out int value)
    {
        if (Span.Length < 4)
        {
            value = 0;
            return false;
        }

        value = BinaryPrimitives.ReadInt32LittleEndian(Span);
        Span = Span[4..];
        Offset += 4;
        return true;
    }

    public bool TryReadBytes(int count, out ReadOnlySpan<byte> bytes)
    {
        if (count < 0 || Span.Length < count)
        {
            bytes = default(ReadOnlySpan<byte>);
            return false;
        }

        bytes = Span[..count];
        Span = Span[count..];
        Offset += count;
        return true;
    }

    // TODO: TryReadVarInt
}