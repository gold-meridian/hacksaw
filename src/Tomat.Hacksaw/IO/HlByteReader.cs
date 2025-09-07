using System;
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
public readonly struct HlByteReader(BinaryReader reader)
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

    public int ReadBytes(Span<byte> buffer)
    {
        return reader.Read(buffer);
    }

    public int ReadIndex()
    {
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
    }

    public uint ReadUIndex()
    {
        var index = ReadIndex();
        if (index < 0)
        {
            throw new InvalidDataException("Unsigned index read with negative value");
        }

        return (uint)index;
    }
}