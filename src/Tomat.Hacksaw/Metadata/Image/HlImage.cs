using System;
using System.IO;
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

    public required IPool<TypeHandle, ImageType> TypePool { get; init; }

    public required IPool<GlobalHandle, ImageGlobal> GlobalPool { get; init; }

    public required IPool<NativeHandle, ImageNative> NativePool { get; init; }

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

    public static HlImage Read(HlByteReader reader)
    {
        var header = HlHeader.Read(reader, HlHeader.HLB);
        if (header != HlHeader.HLB)
        {
            throw new InvalidDataException("Expected header: 'HLB'");
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
        var bytePool = ReadBytes(reader, byteCount, version >= HlVersion.FEATURE_BYTES);
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

    private static IPool<ByteHandle, ByteCollection> ReadBytes(HlByteReader reader, uint byteCount, bool readBytes)
    {
        if (!readBytes)
        {
            return new HashPool<ByteHandle, ByteCollection>([]);
        }

        var bytesSize = reader.ReadInt32();
        var bytes = new byte[bytesSize];
        if (reader.ReadBytes(bytes) != bytesSize)
        {
            throw new InvalidDataException($"Could not read bytes section (expected {bytesSize})");
        }

        /*var startPositions = new uint[byteCount];
        for (var i = 0; i < byteCount; i++)
        {
            startPositions[i] = reader.ReadUIndex();
        }*/

        var byteCollections = new ByteCollection[byteCount];
        for (var i = 0; i < byteCount; i++)
        {
            var start = reader.ReadUIndex();
            var length = 0;

            while (bytes[start + length] != 0)
            {
                length++;
            }

            byteCollections[i] = new ByteCollection(bytes.AsMemory((int)start, length));
        }

        return new HashPool<ByteHandle, ByteCollection>([]);
    }

    private static IPool<DebugFileHandle, string> ReadDebugFiles(HlByteReader reader, bool hasFlag)
    {
        if (!hasFlag)
        {
            return new HashPool<DebugFileHandle, string>([]);
        }

        var debugCount = reader.ReadUIndex();
        return new HashPool<DebugFileHandle, string>(ReadStringBlock(reader, debugCount));
    }

    private static IPool<TypeHandle, ImageType> ReadTypes(HlByteReader reader, uint typeCount)
    {
        var types = new ImageType[typeCount];
        for (var i = 0; i < typeCount; i++)
        {
            types[i] = ReadType(reader);
        }

        return new HashPool<TypeHandle, ImageType>(types);
    }

    private static IPool<GlobalHandle, ImageGlobal> ReadGlobals(HlByteReader reader, uint globalCount)
    {
        var globals = new ImageGlobal[globalCount];
        for (var i = 0; i < globalCount; i++)
        {
            globals[i] = ImageGlobal.From(FunctionHandle.From(reader.ReadIndex()));
        }

        return new ListPool<GlobalHandle, ImageGlobal>(globals);
    }

    private static IPool<NativeHandle, ImageNative> ReadNatives(HlByteReader reader, uint nativeCount)
    {
        var natives = new ImageNative[nativeCount];
        for (var i = 0; i < nativeCount; i++)
        {
            natives[i] = new ImageNative(
                LibraryName: StringHandle.From(reader.ReadIndex()),
                FunctionName: StringHandle.From(reader.ReadIndex()),
                Type: TypeHandle.From(reader.ReadIndex()),
                NativeIndex: reader.ReadUIndex()
            );
        }

        return new HashPool<NativeHandle, ImageNative>(natives);
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

    private static ImageType ReadType(HlByteReader reader)
    {
        var kind = (HlTypeKind)reader.ReadByte();

        switch (kind)
        {
            case HlTypeKind.Fun:
            case HlTypeKind.Method:
            {
                var argCount = reader.ReadByte();
                var arguments = new TypeHandle[argCount];
                for (var i = 0; i < argCount; i++)
                {
                    arguments[i] = TypeHandle.From(reader.ReadIndex());
                }

                var function = new ImageTypeFunction
                {
                    ArgumentTypes = arguments,
                    ReturnType = TypeHandle.From(reader.ReadIndex()),
                };

                return new ImageType.WithFunction(
                    Kind: kind,
                    Function: function
                );
            }

            case HlTypeKind.Obj:
            case HlTypeKind.Struct:
            {
                var name = StringHandle.From(reader.ReadIndex());
                var super = reader.ReadIndex();
                var global = reader.ReadUIndex();
                var fieldCount = reader.ReadUIndex();
                var protoCount = reader.ReadUIndex();
                var bindingCount = reader.ReadUIndex();
                var @object = new ImageTypeObject(
                    Name: name,
                    Super: super < 0 ? null : TypeHandle.From(super),
                    GlobalValue: (int)global,
                    Fields: new ImageTypeObjectField[fieldCount],
                    Prototypes: new ImageTypeObjectPrototype[protoCount],
                    Bindings: new ImageTypeObject.BindingData[bindingCount]
                );

                for (var i = 0; i < fieldCount; i++)
                {
                    @object.Fields[i] = new ImageTypeObjectField(
                        Name: StringHandle.From(reader.ReadIndex()),
                        Type: TypeHandle.From(reader.ReadIndex()),
                        Index: i
                    );
                }

                for (var i = 0; i < protoCount; i++)
                {
                    @object.Prototypes[i] = new ImageTypeObjectPrototype(
                        Name: StringHandle.From(reader.ReadIndex()),
                        FunctionIndex: (int)reader.ReadUIndex(),
                        PrototypeIndex: reader.ReadIndex()
                    );
                }

                for (var i = 0; i < bindingCount; i++)
                {
                    @object.Bindings[i] = new ImageTypeObject.BindingData(
                        FieldIndex: (int)reader.ReadUIndex(),
                        FunctionIndex: (int)reader.ReadUIndex()
                    );
                }

                return new ImageType.WithObject(
                    Kind: kind,
                    Object: @object
                );
            }

            case HlTypeKind.Ref:
            {
                return new ImageType.WithType(
                    Kind: kind,
                    Type: TypeHandle.From(reader.ReadIndex())
                );
            }

            case HlTypeKind.Virtual:
            {
                var fieldCount = reader.ReadUIndex();
                var @virtual = new ImageTypeVirtual(
                    Fields: new ImageTypeObjectField[fieldCount]
                );

                for (var i = 0; i < fieldCount; i++)
                {
                    @virtual.Fields[i] = new ImageTypeObjectField(
                        Name: StringHandle.From(reader.ReadIndex()),
                        Type: TypeHandle.From(reader.ReadIndex()),
                        i
                    );
                }

                return new ImageType.WithVirtual(
                    Kind: kind,
                    Virtual: @virtual
                );
            }

            case HlTypeKind.Abstract:
            {
                return new ImageType.WithAbstractName(
                    Kind: kind,
                    AbstractName: StringHandle.From(reader.ReadIndex())
                );
            }

            case HlTypeKind.Enum:
            {
                var @enum = new ImageTypeEnum(
                    Name: StringHandle.From(reader.ReadIndex()),
                    GlobalValue: (int)reader.ReadUIndex(),
                    Constructs: new ImageTypeEnumConstruct[reader.ReadUIndex()]
                );

                for (var i = 0; i < @enum.Constructs.Length; i++)
                {
                    var name = StringHandle.From(reader.ReadIndex());
                    var paramCount = reader.ReadUIndex();
                    var construct = @enum.Constructs[i] = new ImageTypeEnumConstruct(
                        Name: name,
                        Parameters: new TypeHandle[paramCount],
                        Offsets: new int[paramCount]
                    );

                    for (var j = 0; j < paramCount; j++)
                    {
                        construct.Parameters[j] = TypeHandle.From(reader.ReadIndex());
                    }
                }

                return new ImageType.WithEnum(
                    Kind: kind,
                    Enum: @enum
                );
            }

            case HlTypeKind.Null:
            case HlTypeKind.Packed:
            {
                return new ImageType.WithType(
                    Kind: kind,
                    Type: TypeHandle.From(reader.ReadIndex())
                );
            }

            default:
            {
                if (kind >= HlTypeKind.Last)
                {
                    throw new InvalidDataException($"Invalid type kind: : {kind}");
                }

                return new ImageType.Simple(kind);
            }
        }
    }
}