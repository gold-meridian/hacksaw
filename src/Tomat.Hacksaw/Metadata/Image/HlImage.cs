using System;
using System.Collections.Generic;
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

    public required IPool<FunctionHandle, ImageFunction> FunctionPool { get; init; }

    public required IPool<ConstantHandle, ImageConstant> ConstantPool { get; init; }

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
        var functionPool = ReadFunctions(reader, functionCount, flags.HasFlag(HlFlags.Debug), version);
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
        var ints = new List<int>((int)intCount);
        for (var i = 0; i < intCount; i++)
        {
            ints.Add(reader.ReadInt32());
        }

        return new ImmutableListPool<IntHandle, int>(ints);
    }

    private static IPool<FloatHandle, double> ReadFloats(HlByteReader reader, uint floatCount)
    {
        var floats = new List<double>((int)floatCount);
        for (var i = 0; i < floatCount; i++)
        {
            floats.Add(reader.ReadDouble());
        }

        return new ImmutableListPool<FloatHandle, double>(floats);
    }

    private static IPool<StringHandle, string> ReadStrings(HlByteReader reader, uint stringCount)
    {
        return new ImmutableListPool<StringHandle, string>(ReadStringBlock(reader, stringCount));
    }

    private static IPool<ByteHandle, ByteCollection> ReadBytes(HlByteReader reader, uint byteCount, bool readBytes)
    {
        if (!readBytes)
        {
            return new ImmutableListPool<ByteHandle, ByteCollection>([]);
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

        var byteCollections = new List<ByteCollection>((int)byteCount);
        for (var i = 0; i < byteCount; i++)
        {
            var start = reader.ReadUIndex();
            var length = 0;

            while (bytes[start + length] != 0)
            {
                length++;
            }

            byteCollections.Add(new ByteCollection(bytes.AsMemory((int)start, length)));
        }

        return new ImmutableListPool<ByteHandle, ByteCollection>(byteCollections);
    }

    private static IPool<DebugFileHandle, string> ReadDebugFiles(HlByteReader reader, bool hasFlag)
    {
        if (!hasFlag)
        {
            return new ImmutableListPool<DebugFileHandle, string>([]);
        }

        var debugCount = reader.ReadUIndex();
        return new ImmutableListPool<DebugFileHandle, string>(ReadStringBlock(reader, debugCount));
    }

    private static IPool<TypeHandle, ImageType> ReadTypes(HlByteReader reader, uint typeCount)
    {
        var types = new List<ImageType>((int)typeCount);
        for (var i = 0; i < typeCount; i++)
        {
            types.Add(ReadType(reader));
        }

        return new ImmutableListPool<TypeHandle, ImageType>(types);
    }

    private static IPool<GlobalHandle, ImageGlobal> ReadGlobals(HlByteReader reader, uint globalCount)
    {
        var globals = new List<ImageGlobal>((int)globalCount);
        for (var i = 0; i < globalCount; i++)
        {
            globals.Add(ImageGlobal.From(FunctionHandle.From(reader.ReadIndex())));
        }

        return new ImmutableListPool<GlobalHandle, ImageGlobal>(globals);
    }

    private static IPool<NativeHandle, ImageNative> ReadNatives(HlByteReader reader, uint nativeCount)
    {
        var natives = new List<ImageNative>((int)nativeCount);
        for (var i = 0; i < nativeCount; i++)
        {
            natives.Add(
                new ImageNative(
                    LibraryName: StringHandle.From(reader.ReadIndex()),
                    FunctionName: StringHandle.From(reader.ReadIndex()),
                    Type: TypeHandle.From(reader.ReadIndex()),
                    NativeIndex: reader.ReadUIndex()
                )
            );
        }

        return new ImmutableListPool<NativeHandle, ImageNative>(natives);
    }

    private static IPool<FunctionHandle, ImageFunction> ReadFunctions(HlByteReader reader, uint functionCount, bool debug, HlVersion version)
    {
        var functions = new List<ImageFunction>((int)functionCount);
        for (var i = 0; i < functionCount; i++)
        {
            var function = ReadFunction(reader);

            if (debug)
            {
                function = function with
                {
                    Debugs = ReadDebugInfo(reader, function.Opcodes.Length),
                };
            }

            if (version >= HlVersion.FEATURE_FUNC_ASSIGNS)
            {
                var assignCount = reader.ReadUIndex();
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

            functions.Add(function);
        }

        return new ImmutableListPool<FunctionHandle, ImageFunction>(functions);
    }

    private static IPool<ConstantHandle, ImageConstant> ReadConstants(HlByteReader reader, uint constantCount)
    {
        var constants = new List<ImageConstant>((int)constantCount);
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

            constants.Add(constant);
        }

        return new ImmutableListPool<ConstantHandle, ImageConstant>(constants);
    }

    private static List<string> ReadStringBlock(HlByteReader reader, uint stringCount)
    {
        var sizeInBytes = reader.ReadInt32();

        var stringBytes = sizeInBytes < 1024 ? stackalloc byte[sizeInBytes] : new byte[sizeInBytes];
        if (reader.ReadBytes(stringBytes) != sizeInBytes)
        {
            throw new InvalidDataException($"Could not read string block; not enough bytes for size: {sizeInBytes}");
        }

        var strings = new List<string>((int)stringCount);

        var offset = 0;
        for (var i = 0; i < stringCount; i++)
        {
            var stringSize = reader.ReadUIndex();
            strings.Add(Encoding.UTF8.GetString(stringBytes.Slice(offset, (int)stringSize)));

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

    private static ImageFunction ReadFunction(HlByteReader reader)
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
            opcodes[i] = ReadOpcode(reader);
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

    private static ImageOpcode ReadOpcode(HlByteReader reader)
    {
        var kind = (HlOpcodeKind)reader.ReadUIndex();

        if (kind >= HlOpcodeKind.Last)
        {
            throw new InvalidDataException($"Got invalid opcode kind: {kind}");
        }

        switch (kind.GetArgumentCount())
        {
            case -1:
            {
                switch (kind)
                {
                    case HlOpcodeKind.CallN:
                    case HlOpcodeKind.CallClosure:
                    case HlOpcodeKind.CallMethod:
                    case HlOpcodeKind.CallThis:
                    case HlOpcodeKind.MakeEnum:
                    {
                        var p1 = reader.ReadIndex();
                        var p2 = reader.ReadIndex();
                        var p3 = (int)reader.ReadByte();
                        var parameters = (Span<int>)stackalloc int[p3 + 4];
                        {
                            parameters[0] = (int)kind;
                            parameters[1] = p1;
                            parameters[2] = p2;
                            parameters[3] = p3;
                        }

                        for (var i = 0; i < p3; i++)
                        {
                            parameters[i + 4] = reader.ReadIndex();
                        }

                        return CreateOpcode(parameters);
                    }

                    case HlOpcodeKind.Switch:
                    {
                        var p1 = (int)reader.ReadUIndex();
                        var p2 = (int)reader.ReadUIndex();
                        var parameters = (Span<int>)stackalloc int[p2 + 4];
                        {
                            parameters[0] = (int)kind;
                            parameters[1] = p1;
                            parameters[2] = p2;
                        }

                        for (var i = 0; i < p2; i++)
                        {
                            parameters[i + 3] = (int)reader.ReadUIndex();
                        }

                        var p3 = (int)reader.ReadUIndex();
                        {
                            parameters[^1] = p3;
                        }

                        return CreateOpcode(parameters);
                    }

                    default:
                        throw new InvalidDataException($"Invalid opcode kind for variable-length decoding: {kind}");
                }
            }

            default:
            {
                var size = kind.GetArgumentCount();
                var parameters = (Span<int>)stackalloc int[size + 1];
                {
                    parameters[0] = (int)kind;
                }

                for (var i = 0; i < size; i++)
                {
                    parameters[i + 1] = reader.ReadIndex();
                }

                return CreateOpcode(parameters);
            }
        }

        static ImageOpcode CreateOpcode(ReadOnlySpan<int> data)
        {
            return new ImageOpcode(
                Ctx: new ImageOpcode.Context(
                    Data: data.ToArray()
                )
            );
        }
    }

    private static ImageFunction.Debug[] ReadDebugInfo(HlByteReader reader, int opcodeCount)
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

                /*if (currFile >= code.DebugFiles.Count)
                {
                    throw new InvalidDataException($"Invalid debug file index: {currFile}");
                }*/
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
}