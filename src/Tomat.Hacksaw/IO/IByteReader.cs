using System;

namespace Tomat.Hacksaw.IO;

public interface IByteReader
{
    long Length { get; }

    long Position { get; set; }

    byte ReadByte();

    int ReadInt32();

    double ReadDouble();

    int ReadBytes(scoped Span<byte> buffer);

    int BorrowSlice(int length, out Span<byte> buffer);

    int ReadIndex();

    uint ReadUIndex();
}