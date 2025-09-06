using System;
using System.IO;
using System.Linq;

using Tomat.Hacksaw.IO;

namespace Tomat.Hacksaw.Metadata.Image;

/// <summary>
///     A read-only view of a HashLink binary.
/// </summary>
public readonly struct HlImage
{
    public required HlHeader Header { get; init; }

    public required HlVersion Version { get; init; }

    public required HlFlags Flags { get; init; }

    public bool HasDebug => Flags.HasFlag(HlFlags.Debug);

    // int[]
    // float[]
    // string[]
    // byte[][]
    // type[]
    // global[]
    // native[]
    // function[]
    // constant[]

    public static HlImage Read(Stream stream)
    {
        using var br = new BinaryReader(stream);
        return Read(br);
    }

    public static HlImage Read(BinaryReader reader)
    {
        return Read(new HlByteReader(reader));
    }

    public static unsafe HlImage Read(HlByteReader reader)
    {
        var header = HlHeader.Read(reader, HlHeader.HLB);
        if (header != HlHeader.HLB)
        {
            throw new InvalidDataException($"Expected header: 'HLB'");
        }

        var version = HlVersion.Read(reader);

        var flags = (HlFlags)reader.ReadUIndex();
        var intCount = reader.ReadUIndex();
        var floatCount = reader.ReadUIndex();
        var stringCount = reader.ReadUIndex();
        var byteCount = version >= HlVersion.FEATURE_BYTES ? reader.ReadUIndex() : 0;
        var typeCount = reader.ReadUIndex();
        var globalCount = reader.ReadUIndex();
        var nativeCount = reader.ReadUIndex();
        var functionCount = reader.ReadUIndex();
        var constantCount = reader.ReadUIndex();
        var entryPoint = reader.ReadUIndex();

        return new HlImage
        {
            Header = header,
            Version = version,
            Flags = flags,
        };
    }
}