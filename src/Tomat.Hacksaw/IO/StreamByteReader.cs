using System;
using System.Diagnostics;
using System.IO;

namespace Tomat.Hacksaw.IO;

// Wrapping BinaryReader is a little useless, but it's somewhat convenient.
// BinaryReader is quite fast!!! The standard library also knows how to optimize
// around it; respect it.

/// <summary>
///     Light-weight wrapper over a <see cref="BinaryReader"/> to provide
///     specifically the functions required to read a HashLink binary (with the
///     correct endianness).
/// </summary>
/// <param name="reader">The reader.</param>
public readonly struct StreamByteReader(BinaryReader reader) : IByteReader
{
    // BinaryReader implements its functions with LE in mind
    // (BinaryPrimitives.Read*LittleEndian), I have no idea if this is
    // explicitly documented somewhere.
    // We could get better gains by either pre-allocating the entire buffer or
    // using a scratch buffer and directly interpreting memory, but we lose
    // (admittedly rarely useful) little endianness guarantees, and it's kind of
    // a headache.

    public long Length => reader.BaseStream.Length;

    public long Position
    {
        get => reader.BaseStream.Position;
        set => reader.BaseStream.Position = value;
    }

    public byte ReadByte()
    {
        return reader.ReadByte();
    }

    public int ReadInt32()
    {
        return reader.ReadInt32();
    }

    public double ReadDouble()
    {
        return reader.ReadDouble();
    }

    public int ReadBytes(scoped Span<byte> buffer)
    {
        return reader.Read(buffer);
    }

    public int BorrowSlice(int length, out Span<byte> buffer)
    {
        buffer = new byte[length];
        return reader.Read(buffer);
    }

    public int ReadIndex()
    {
        var b = reader.ReadByte();
        int v;

        switch ((b >> 6) & 0b11)
        {
            case 0b00:
            case 0b01:
                return b;

            case 0b10:
                v = reader.ReadByte() | ((b & 0x1F) << 8);
                break;

            case 0b11:
                v = ((b & 0x1F) << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte();
                /*Position--;
                var buf = (Span<byte>)stackalloc byte[4];
                _ = reader.Read(buf);
                buf[0] = (byte)(b & 0x1F);
                v = BinaryPrimitives.ReadInt32BigEndian(buf);*/
                break;

            default:
                throw new InvalidOperationException($"Invalid var-int prefix: {(b >> 6) & 0b11}");
        }

        var signBit = (b >> 5) & 1;
        return (v ^ -signBit) + signBit;
        
        /*Span<byte> buf = stackalloc byte[3];
        var b = reader.ReadByte();

        if ((b & 0x80) == 0)
        {
            return b;
        }

        var extraBytes = (b & 0x40) == 0 ? 1 : 3;
        var read = reader.Read(buf[..extraBytes]);
        {
            Debug.Assert(read == extraBytes);
        }

        var v = extraBytes switch
        {
            1 => buf[0] | ((b & 0x1F) << 8),
            3 => ((b & 0x1F) << 24) | (buf[0] << 16) | (buf[1] << 8) | buf[2],
            _ => throw new InvalidOperationException("Unreachable"),
        };

        var signBit = (b >> 5) & 1;
        return (v ^ -signBit) + signBit;*/

        /*
        var b = reader.ReadByte();

        if ((b & 0x80) == 0)
        {
            return b & 0x7f;
        }

        if ((b & 0x40) == 0)
        {
            var v = reader.ReadByte() | ((b & 31) << 8);
            return (b & 0x20) == 0 ? v : -v;
        }
        else
        {
            var c = reader.ReadByte();
            var d = reader.ReadByte();
            var e = reader.ReadByte();
            var v = ((b & 31) << 24) | (c << 16) | (d << 8) | e;
            return (b & 0x20) == 0 ? v : -v;
        }
        */
    }

    public uint ReadUIndex()
    {
        var b = reader.ReadByte();
        uint v;

        switch ((b >> 6) & 0b11)
        {
            case 0b00:
            case 0b01:
                return b;

            case 0b10:
                v = reader.ReadByte() | (uint)((b & 0x1F) << 8);
                break;

            case 0b11:
                v = ((uint)(b & 0x1F) << 24) | (uint)(reader.ReadByte() << 16) | (uint)(reader.ReadByte() << 8) | reader.ReadByte();
                break;

            default:
                throw new InvalidOperationException($"Invalid var-int prefix: {(b >> 6) & 0b11}");
        }

        Debug.Assert(((b >> 5) & 1) != 1);
        // throw new InvalidDataException("Unsigned index read with negative value");
        return v;
    }

    public void SkipIndex()
    {
        var b = reader.ReadByte();

        switch ((b >> 6) & 0b11)
        {
            case 0b00:
            case 0b01:
                return;

            case 0b10:
                Position++;
                return;

            case 0b11:
                Position += 3;
                return;

            default:
                throw new InvalidOperationException($"Invalid var-int prefix: {(b >> 6) & 0b11}");
        }
    }
}