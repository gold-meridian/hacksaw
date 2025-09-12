using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Tomat.Hacksaw.IO;

public unsafe ref struct MemoryByteReader : IByteReader
{
    public readonly long Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => end - ptr;
    }

    public long Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => current - ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => current = ptr + value;
    }

    private readonly byte* ptr;
    private readonly byte* end;
    private byte* current;

    public MemoryByteReader(Span<byte> bytes)
    {
        fixed (byte* p = bytes)
        {
            ptr = p;
            current = p;
            end = p + bytes.Length;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
    {
        return *current++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32()
    {
        var result = *(int*)current;
        current += 4;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble()
    {
        var result = *(double*)current;
        current += 8;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadBytes(scoped Span<byte> buffer)
    {
        var remaining = (int)(end - current);
        var read = Math.Min(buffer.Length, remaining);

        if (read > 0)
        {
            new ReadOnlySpan<byte>(current, read).CopyTo(buffer);
            current += read;
        }

        return read;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int BorrowSlice(int length, out Span<byte> buffer)
    {
        var remaining = (int)(end - current);
        var read = Math.Min(length, remaining);

        buffer = new Span<byte>(current, read);
        current += read;

        return read;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadIndex()
    {
        var p = current;
        uint b = *p;

        if (b < 0x80)
        {
            current = p + 1;
            return (int)b;
        }

        p++;

        var signBit = (int)(b << 26) >> 31;

        if (b < 0xC0)
        {
            var v = *p | ((b & 0x1F) << 8);
            current = p + 1;

            return ((int)v ^ signBit) - signBit;
        }

        var chunk = *(uint*)p;
        var swapped = BinaryPrimitives.ReverseEndianness(chunk) >> 8;
        var v4 = ((b & 0x1F) << 24) | swapped;
        current = p + 3;

        return ((int)v4 ^ signBit) - signBit;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUIndex()
    {
        return (uint)ReadIndex();
    }
}