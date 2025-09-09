using System;
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

    public int ReadIndex()
    {
        var p = current;
        var b = *p++;

        if ((b & 0x80) == 0)
        {
            current = p;
            return b;
        }

        if ((b & 0x40) == 0)
        {
            var v = *p++ | ((b & 0x1F) << 8);
            current = p;
            
            var signBit = (b << 26) >> 31;
            return (v ^ signBit) - signBit;
        }

        var bytes3 = Unsafe.ReadUnaligned<int>(p) & 0x00FFFFFF;
        var c = bytes3 & 0xFF;
        var d = (bytes3 >> 8) & 0xFF;
        var e = (bytes3 >> 16) & 0xFF;

        var v4 = ((b & 0x1F) << 24) | (c << 16) | (d << 8) | e;
        current = p + 3;

        var signBit4 = (b << 26) >> 31;
        return (v4 ^ signBit4) - signBit4;
    }

    public uint ReadUIndex()
    {
        var p = current;
        var b = *p++;

        if ((b & 0x80) == 0)
        {
            current = p;
            return b;
        }

        if ((b & 0x40) == 0)
        {
            var v = (uint)(*p++ | ((b & 0x1F) << 8));
            current = p;

            var mask = (uint)((b << 26) >> 31);
            return v & ~mask;
        }

        var bytes3 = Unsafe.ReadUnaligned<uint>(p) & 0x00FFFFFF;
        var c = bytes3 & 0xFF;
        var d = (bytes3 >> 8) & 0xFF;
        var e = (bytes3 >> 16) & 0xFF;

        var v4 = (uint)((b & 0x1F) << 24) | (c << 16) | (d << 8) | e;
        current = p + 3;

        var mask4 = (uint)((b << 26) >> 31);
        return v4 & ~mask4;
    }
}