using System;
using System.IO;
using System.Linq;
using System.Text;

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

    public required IPool<IntHandle, int> IntPool { get; init; }

    public required IPool<FloatHandle, double> FloatPool { get; init; }

    public required IPool<StringHandle, string> StringPool { get; init; }

    public required IPool<ByteHandle, ByteCollection> BytePool { get; init; }

    public required IPool<DebugFileHandle, string> DebugFilePool { get; init; }

    public required IPool<TypeHandle, object> TypePool { get; init; } // TODO: HlType

    public required IPool<GlobalHandle, object> GlobalPool { get; init; } // TODO: HlGlobal

    public required IPool<NativeHandle, object> NativePool { get; init; } // TODO: HlNative

    public required IPool<FunctionHandle, object> FunctionPool { get; init; } // TODO: HlFunction

    public required IPool<ConstantHandle, object> ConstantPool { get; init; } // TODO: HlConstant

    public required FunctionHandle Entrypoint { get; init; }

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

        var intPool = ReadInts(reader, intCount);
        var floatPool = ReadFloats(reader, floatCount);
        var stringPool = ReadStrings(reader, stringCount);
        var bytePool = ReadBytes(reader, byteCount);
        var debugFilePool = ReadDebugFiles(reader, flags.HasFlag(HlFlags.Debug));
        var typePool = ReadTypes(reader, typeCount);
        var globalPool = ReadGlobals(reader, globalCount);
        var nativePool = ReadNatives(reader, nativeCount);
        var functionPool = ReadFunctions(reader, functionCount);
        var constantPool = ReadConstants(reader, constantCount);

        return new HlImage
        {
            Header = header,
            Version = version,
            Flags = flags,
            IntPool = intPool,
            FloatPool = floatPool,
            StringPool = stringPool,
            BytePool = bytePool,
            DebugFilePool = debugFilePool,
            TypePool = typePool,
            GlobalPool = globalPool,
            NativePool = nativePool,
            FunctionPool = functionPool,
            ConstantPool = constantPool,
            Entrypoint = FunctionHandle.From((int)entryPoint),
        };
    }

    private static IPool<IntHandle, int> ReadInts(HlByteReader reader, uint intCount)
    {
        var ints = new int[intCount];
        for (var i = 0; i < intCount; i++)
        {
            ints[i] = reader.ReadInt32();
        }

        return new HashPool<IntHandle, int>(ints);
    }

    private static IPool<FloatHandle, double> ReadFloats(HlByteReader reader, uint floatCount)
    {
        var floats = new double[floatCount];
        for (var i = 0; i < floatCount; i++)
        {
            floats[i] = reader.ReadDouble();
        }

        return new HashPool<FloatHandle, double>(floats);
    }

    private static IPool<StringHandle, string> ReadStrings(HlByteReader reader, uint stringCount)
    {
        return new HashPool<StringHandle, string>(ReadStringBlock(reader, stringCount));
    }

    private static IPool<ByteHandle, ByteCollection> ReadBytes(HlByteReader reader, uint byteCount)
    {
        return new HashPool<ByteHandle, ByteCollection>([]);
    }

    private static IPool<DebugFileHandle, string> ReadDebugFiles(HlByteReader reader, bool hasFlag)
    {
        return new HashPool<DebugFileHandle, string>([]);
    }

    private static IPool<TypeHandle, object> ReadTypes(HlByteReader reader, uint typeCount)
    {
        return new HashPool<TypeHandle, object>([]);
    }

    private static IPool<GlobalHandle, object> ReadGlobals(HlByteReader reader, uint globalCount)
    {
        return new HashPool<GlobalHandle, object>([]);
    }

    private static IPool<NativeHandle, object> ReadNatives(HlByteReader reader, uint nativeCount)
    {
        return new HashPool<NativeHandle, object>([]);
    }

    private static IPool<FunctionHandle, object> ReadFunctions(HlByteReader reader, uint functionCount)
    {
        return new HashPool<FunctionHandle, object>([]);
    }

    private static IPool<ConstantHandle, object> ReadConstants(HlByteReader reader, uint constantCount)
    {
        return new HashPool<ConstantHandle, object>([]);
    }

    private static string[] ReadStringBlock(HlByteReader reader, uint stringCount)
    {
        var sizeInBytes = reader.ReadInt32();

        var stringBytes = sizeInBytes < 1024 ? stackalloc byte[sizeInBytes] : new byte[sizeInBytes];
        if (reader.ReadBytes(stringBytes) != sizeInBytes)
        {
            throw new InvalidDataException($"Could not read string block; not enough bytes for size: {sizeInBytes}");
        }

        var strings = new string[stringCount];

        var offset = 0;
        for (var i = 0; i < stringCount; i++)
        {
            var stringSize = reader.ReadUIndex();
            strings[i] = Encoding.UTF8.GetString(stringBytes.Slice(offset, (int)stringSize));

            // Account for the null terminator character.
            offset += (int)stringSize + 1;
        }

        return strings;
    }
}