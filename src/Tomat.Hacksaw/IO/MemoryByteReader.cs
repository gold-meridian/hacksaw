using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Tomat.Hacksaw.IO;

public unsafe ref struct MemoryByteReader(Span<byte> bytes) : IByteReader
{
    public long Length => bytes.Length;

    public long Position
    {
        get => position;
        set => position = (int)value;
    }

    private int position;
    private readonly Span<byte> bytes = bytes;

    private T Read<T>()
        where T : unmanaged
    {
        /*var size = sizeof(T);
        {
            Debug.Assert(position + size <= bytes.Length);
        }*/

        var value = Unsafe.As<byte, T>(ref bytes[position]);
        Position += sizeof(T);
        return value;
    }
    
    public byte ReadByte()
    {
        return bytes[position++];
    }

    public int ReadInt32()
    {
        return Read<int>();
    }

    public double ReadDouble()
    {
        return Read<double>();
    }

    public int ReadBytes(scoped Span<byte> buffer)
    {
        var read = Math.Min(buffer.Length, bytes.Length - position);
        bytes.Slice(position, read).CopyTo(buffer);
        position += read;
        return read;
    }

    public int ReadIndex()
    {
        var b = bytes[position++];
        int v;

        switch ((b >> 6) & 0b11)
        {
            case 0b00:
            case 0b01:
                return b;

            case 0b10:
                v = bytes[position++] | ((b & 0x1F) << 8);
                break;

            case 0b11:
                v = ((b & 0x1F) << 24) | (bytes[position++] << 16) | (bytes[position++] << 8) | bytes[position++];
                break;

            default:
                throw new InvalidOperationException($"Invalid var-int prefix: {(b >> 6) & 0b11}");
        }

        var signBit = (b >> 5) & 1;
        return (v ^ -signBit) + signBit;
    }

    public uint ReadUIndex()
    {
        var b = bytes[position++];
        uint v;

        switch ((b >> 6) & 0b11)
        {
            case 0b00:
            case 0b01:
                return b;

            case 0b10:
                v = bytes[position++] | (uint)((b & 0x1F) << 8);
                break;

            case 0b11:
                v = ((uint)(b & 0x1F) << 24) | (uint)(bytes[position++] << 16) | (uint)(bytes[position++] << 8) | bytes[position++];
                break;

            default:
                throw new InvalidOperationException($"Invalid var-int prefix: {(b >> 6) & 0b11}");
        }

        Debug.Assert(((b >> 5) & 1) != 1);
        // throw new InvalidDataException("Unsigned index read with negative value");
        return v;
    }
}