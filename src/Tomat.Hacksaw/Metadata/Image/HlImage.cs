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
    public readonly record struct ReadSettings(
        bool StoreDebugInfo = true,
        bool StoreFunctionAssigns = true,
        // It gets faster the greater the amount, but let's strike a balance between speed and memory consumption.  As
        // it stands, it's more efficient memory-wise to use this than to allocate for every array since there's
        // additional data associated with arrays (since they're objects).
        int OpcodeBytePoolSize = 1 << 18
    )
    {
        internal readonly PooledArrayAllocator<int> OpcodePoolAllocator = new(OpcodeBytePoolSize);
    }

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

    public required IPool<FunctionHandle, ImageFunction> FunctionPool { get; init; }

    public required IPool<ConstantHandle, ImageConstant> ConstantPool { get; init; }

    public required FunctionHandle Entrypoint { get; init; }

    public static HlImage Read(Stream stream, ReadSettings settings)
    {
        using var br = new BinaryReader(stream);
        return Read(br, settings);
    }

    public static HlImage Read(BinaryReader reader, ReadSettings settings)
    {
        var sReader = new StreamByteReader(reader);
        return Read(ref sReader, settings);
    }

    public static HlImage Read(byte[] data, ReadSettings settings)
    {
        var reader = new MemoryByteReader(data);
        return Read(ref reader, settings);
    }

    public static HlImage Read<TByteReader>(ref TByteReader reader, ReadSettings settings)
        where TByteReader : IByteReader, allows ref struct
    {
        var header = HlHeader.Read(ref reader, HlHeader.HLB);
        if (header != HlHeader.HLB)
        {
            throw new InvalidDataException("Expected header: 'HLB'");
        }

        var version = HlVersion.Read(ref reader);

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

        var intPool = ReadInts(ref reader, intCount);
        var floatPool = ReadFloats(ref reader, floatCount);
        var stringPool = ReadStrings(ref reader, stringCount);
        var bytePool = ReadBytes(ref reader, byteCount, version >= HlVersion.FEATURE_BYTES);
        var debugFilePool = ReadDebugFiles(ref reader, flags.HasFlag(HlFlags.Debug));
        var typePool = ReadTypes(ref reader, typeCount);
        var globalPool = ReadGlobals(ref reader, globalCount);
        var nativePool = ReadNatives(ref reader, nativeCount);
        var functionPool = ReadFunctions(ref reader, functionCount, flags.HasFlag(HlFlags.Debug), version, settings);
        var constantPool = ReadConstants(ref reader, constantCount);

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

    private static ImmutablePool<IntHandle, int> ReadInts<TByteReader>(ref TByteReader reader, uint intCount)
        where TByteReader : IByteReader, allows ref struct
    {
        var ints = new int[intCount];
        for (var i = 0; i < intCount; i++)
        {
            ints[i] = reader.ReadInt32();
        }

        return new ImmutablePool<IntHandle, int>(ints);
    }

    private static ImmutablePool<FloatHandle, double> ReadFloats<TByteReader>(ref TByteReader reader, uint floatCount)
        where TByteReader : IByteReader, allows ref struct
    {
        var floats = new double[floatCount];
        for (var i = 0; i < floatCount; i++)
        {
            floats[i] = reader.ReadDouble();
        }

        return new ImmutablePool<FloatHandle, double>(floats);
    }

    private static ImmutablePool<StringHandle, string> ReadStrings<TByteReader>(ref TByteReader reader, uint stringCount)
        where TByteReader : IByteReader, allows ref struct
    {
        return new ImmutablePool<StringHandle, string>(ReadStringBlock(ref reader, stringCount));
    }

    private static ImmutablePool<ByteHandle, ByteCollection> ReadBytes<TByteReader>(ref TByteReader reader, uint byteCount, bool readBytes)
        where TByteReader : IByteReader, allows ref struct
    {
        if (!readBytes)
        {
            return new ImmutablePool<ByteHandle, ByteCollection>([]);
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

        return new ImmutablePool<ByteHandle, ByteCollection>(byteCollections);
    }

    private static ImmutablePool<DebugFileHandle, string> ReadDebugFiles<TByteReader>(ref TByteReader reader, bool hasFlag)
        where TByteReader : IByteReader, allows ref struct
    {
        if (!hasFlag)
        {
            return new ImmutablePool<DebugFileHandle, string>([]);
        }

        var debugCount = reader.ReadUIndex();
        return new ImmutablePool<DebugFileHandle, string>(ReadStringBlock(ref reader, debugCount));
    }

    private static ImmutablePool<TypeHandle, ImageType> ReadTypes<TByteReader>(ref TByteReader reader, uint typeCount)
        where TByteReader : IByteReader, allows ref struct
    {
        var types = new ImageType[typeCount];
        for (var i = 0; i < typeCount; i++)
        {
            types[i] = ReadType(ref reader);
        }

        return new ImmutablePool<TypeHandle, ImageType>(types);
    }

    private static ImmutablePool<GlobalHandle, ImageGlobal> ReadGlobals<TByteReader>(ref TByteReader reader, uint globalCount)
        where TByteReader : IByteReader, allows ref struct
    {
        var globals = new ImageGlobal[globalCount];
        for (var i = 0; i < globalCount; i++)
        {
            globals[i] = ImageGlobal.From(FunctionHandle.From(reader.ReadIndex()));
        }

        return new ImmutablePool<GlobalHandle, ImageGlobal>(globals);
    }

    private static ImmutablePool<NativeHandle, ImageNative> ReadNatives<TByteReader>(ref TByteReader reader, uint nativeCount)
        where TByteReader : IByteReader, allows ref struct
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

        return new ImmutablePool<NativeHandle, ImageNative>(natives);
    }

    private static ImmutablePool<FunctionHandle, ImageFunction> ReadFunctions<TByteReader>(
        ref TByteReader reader,
        uint functionCount,
        bool debug,
        HlVersion version,
        ReadSettings settings
    )
        where TByteReader : IByteReader, allows ref struct
    {
        var functions = new ImageFunction[functionCount];
        for (var i = 0; i < functionCount; i++)
        {
            var function = ReadFunction(ref reader, settings);

            if (debug)
            {
                if (settings.StoreDebugInfo)
                {
                    function = function with
                    {
                        Debugs = ReadDebugInfo(ref reader, function.Opcodes.Length),
                    };
                }
                else
                {
                    SkipDebugInfo(ref reader, function.Opcodes.Length);
                }
            }

            if (version >= HlVersion.FEATURE_FUNC_ASSIGNS)
            {
                var assignCount = reader.ReadUIndex();

                if (settings.StoreFunctionAssigns)
                {
                    function = function with
                    {
                        Assigns = new ImageFunction.Assign[assignCount],
                    };

                    for (var j = 0; j < assignCount; j++)
                    {
                        function.Assigns[j] = new ImageFunction.Assign(
                            Name: StringHandle.From((int)reader.ReadUIndex()),
                            Index: reader.ReadIndex()
                        );
                    }
                }
                else
                {
                    for (var j = 0; j < assignCount; j++)
                    {
                        reader.SkipIndex();
                        reader.SkipIndex();
                    }
                }
            }

            functions[i] = function;
        }

        return new ImmutablePool<FunctionHandle, ImageFunction>(functions);
    }

    private static ImmutablePool<ConstantHandle, ImageConstant> ReadConstants<TByteReader>(ref TByteReader reader, uint constantCount)
        where TByteReader : IByteReader, allows ref struct
    {
        var constants = new ImageConstant[constantCount];
        for (var i = 0; i < constantCount; i++)
        {
            var constant = new ImageConstant(
                GlobalIndex: (int)reader.ReadUIndex(),
                Fields: new int[reader.ReadUIndex()]
            );

            for (var j = 0; j < constant.Fields.Length; j++)
            {
                constant.Fields[j] = (int)reader.ReadUIndex();
            }

            constants[i] = constant;
        }

        return new ImmutablePool<ConstantHandle, ImageConstant>(constants);
    }

    private static string[] ReadStringBlock<TByteReader>(ref TByteReader reader, uint stringCount)
        where TByteReader : IByteReader, allows ref struct
    {
        var sizeInBytes = reader.ReadInt32();

        if (reader.BorrowSlice(sizeInBytes, out var stringBytes) != sizeInBytes)
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

    private static ImageType ReadType<TByteReader>(ref TByteReader reader)
        where TByteReader : IByteReader, allows ref struct
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

    private static ImageFunction ReadFunction<TByteReader>(ref TByteReader reader, ReadSettings settings)
        where TByteReader : IByteReader, allows ref struct
    {
        var type = TypeHandle.From(reader.ReadIndex());
        var functionIndex = (int)reader.ReadUIndex();
        var variableCount = reader.ReadUIndex();
        var opcodeCount = reader.ReadUIndex();
        var variableTypes = new TypeHandle[variableCount];
        for (var i = 0; i < variableCount; i++)
        {
            variableTypes[i] = TypeHandle.From(reader.ReadIndex());
        }

        var opcodes = new ImageOpcode[opcodeCount];
        for (var i = 0; i < opcodeCount; i++)
        {
            opcodes[i] = OpcodeReading.ReadOpcode(ref reader, settings);
        }

        return new ImageFunction(
            FunctionIndex: functionIndex,
            Type: type,
            VariableTypes: variableTypes,
            Opcodes: opcodes,
            Debugs: [],
            Assigns: []
        );
    }

    private static ImageFunction.Debug[] ReadDebugInfo<TByteReader>(
        ref TByteReader reader,
        int opcodeCount
    )
        where TByteReader : IByteReader, allows ref struct
    {
        var debug = new ImageFunction.Debug[opcodeCount];

        var currFile = -1;
        var currLine = 0;

        var currOpcode = 0;
        while (currOpcode < opcodeCount)
        {
            var c = reader.ReadByte();

            if ((c & 1) != 0)
            {
                c >>= 1;
                currFile = (c << 8) | reader.ReadByte();
            }
            else if ((c & 2) != 0)
            {
                var delta = c >> 6;
                var count = (c >> 2) & 15;
                if (currOpcode + count > opcodeCount)
                {
                    throw new InvalidDataException($"Invalid debug line count: {count}");
                }

                while (count-- > 0)
                {
                    debug[currOpcode] = new ImageFunction.Debug(
                        DebugFileHandle.From(currFile),
                        currLine
                    );

                    currOpcode++;
                }

                currLine += delta;
            }
            else if ((c & 4) != 0)
            {
                currLine += c >> 3;
                debug[currOpcode] = new ImageFunction.Debug(
                    DebugFileHandle.From(currFile),
                    currLine
                );
                currOpcode++;
            }
            else
            {
                var b2 = reader.ReadByte();
                var b3 = reader.ReadByte();
                currLine = (c >> 3) | (b2 << 5) | (b3 << 13);
                debug[currOpcode] = new ImageFunction.Debug(
                    DebugFileHandle.From(currFile),
                    currLine
                );
                currOpcode++;
            }
        }

        return debug;
    }

    private static void SkipDebugInfo<TByteReader>(
        ref TByteReader reader,
        int opcodeCount
    )
        where TByteReader : IByteReader, allows ref struct
    {
        var currOpcode = 0;
        while (currOpcode < opcodeCount)
        {
            var c = reader.ReadByte();

            if ((c & 1) != 0)
            {
                reader.Position++;
            }
            else if ((c & 2) != 0)
            {
                var count = (c >> 2) & 15;
                if (currOpcode + count > opcodeCount)
                {
                    throw new InvalidDataException($"Invalid debug line count: {count}");
                }

                while (count-- > 0)
                {
                    currOpcode++;
                }
            }
            else if ((c & 4) != 0)
            {
                currOpcode++;
            }
            else
            {
                reader.Position += 2;
                currOpcode++;
            }
        }
    }
}