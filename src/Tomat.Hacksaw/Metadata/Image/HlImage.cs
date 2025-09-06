using System;
using System.IO;
using System.Linq;

using Tomat.Hacksaw.IO;
using Tomat.Hacksaw.Metadata.Image.Pooling;

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

    public IPool<IntHandle, int> IntPool { get; } = null!;

    public IPool<FloatHandle, double> FloatPool { get; } = null!;

    public IPool<StringHandle, string> StringPool { get; } = null!;

    public IPool<ByteHandle, ByteCollection> BytePool { get; } = null!;

    public IPool<TypeHandle, object> TypePool { get; } = null!; // TODO: HlType

    public IPool<GlobalHandle, object> GlobalPool { get; } = null!; // TODO: HlGlobal

    public IPool<NativeHandle, object> NativePool { get; } = null!; // TODO: HlNative

    public IPool<FunctionHandle, object> FunctionPool { get; } = null!; // TODO: HlFunction

    public IPool<ConstantHandle, object> ConstantPool { get; } = null!; // TODO: HlConstant

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