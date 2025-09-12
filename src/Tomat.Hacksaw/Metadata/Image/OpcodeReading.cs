using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

using Tomat.Hacksaw.IO;

namespace Tomat.Hacksaw.Metadata.Image;

internal static class OpcodeReading
{
    private static readonly sbyte[] argument_counts = new sbyte[256];

    private static readonly bool[] variable_length_table = new bool[256];

    static OpcodeReading()
    {
        for (var i = 0; i < 256; i++)
        {
            var kind = (HlOpcodeKind)i;
            argument_counts[i] = kind < HlOpcodeKind.Last ? (sbyte)kind.GetArgumentCount() : (sbyte)-2;
        }

        variable_length_table[(int)HlOpcodeKind.CallN] = true;
        variable_length_table[(int)HlOpcodeKind.CallClosure] = true;
        variable_length_table[(int)HlOpcodeKind.CallMethod] = true;
        variable_length_table[(int)HlOpcodeKind.CallThis] = true;
        variable_length_table[(int)HlOpcodeKind.MakeEnum] = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsVariableLength(HlOpcodeKind kind)
    {
        return variable_length_table[(int)kind];
    }

    public static ImageOpcode ReadOpcode<TByteReader>(ref TByteReader reader)
        where TByteReader : IByteReader, allows ref struct
    {
        var kindValue = reader.ReadUIndex();

        if (kindValue >= (uint)HlOpcodeKind.Last)
        {
            ThrowInvalidOpcode(kindValue);
            return default(ImageOpcode);
        }

        var kind = (HlOpcodeKind)kindValue;
        var argCount = argument_counts[kindValue];

        if (argCount >= 0)
        {
            return ReadFixedSizeOpcode(ref reader, kind, argCount);
        }

        if (IsVariableLength(kind))
        {
            return ReadVariableLengthOpcode(ref reader, kind);
        }

        if (kind == HlOpcodeKind.Switch)
        {
            return ReadSwitchOpcode(ref reader);
        }

        ThrowInvalidVariableLength(kind);
        return default(ImageOpcode);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ImageOpcode ReadFixedSizeOpcode<TByteReader>(ref TByteReader reader, HlOpcodeKind kind, int argCount)
        where TByteReader : IByteReader, allows ref struct
    {
        var totalSize = argCount + 1;
        var data = new int[totalSize];
        {
            data[0] = (int)kind;
        }

        switch (argCount)
        {
            case 0:
                break;

            case 1:
                data[1] = reader.ReadIndex();
                break;

            case 2:
                data[1] = reader.ReadIndex();
                data[2] = reader.ReadIndex();
                break;

            case 3:
                data[1] = reader.ReadIndex();
                data[2] = reader.ReadIndex();
                data[3] = reader.ReadIndex();
                break;

            default:
                for (var i = 0; i < argCount; i++)
                {
                    data[i + 1] = reader.ReadIndex();
                }
                break;
        }

        return CreateOpcode(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ImageOpcode ReadVariableLengthOpcode<TByteReader>(ref TByteReader reader, HlOpcodeKind kind)
        where TByteReader : IByteReader, allows ref struct
    {
        var p1 = reader.ReadIndex();
        var p2 = reader.ReadIndex();
        var p3 = (int)reader.ReadByte();

        var totalSize = p3 + 4;
        var data = new int[totalSize];
        {
            data[0] = (int)kind;
            data[1] = p1;
            data[2] = p2;
            data[3] = p3;
        }

        for (var i = 0; i < p3; i++)
        {
            data[i + 4] = reader.ReadIndex();
        }

        return CreateOpcode(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ImageOpcode ReadSwitchOpcode<TByteReader>(ref TByteReader reader)
        where TByteReader : IByteReader, allows ref struct
    {
        var p1 = (int)reader.ReadUIndex();
        var p2 = (int)reader.ReadUIndex();

        var totalSize = p2 + 4;
        var data = new int[totalSize];
        {
            data[0] = (int)HlOpcodeKind.Switch;
            data[1] = p1;
            data[2] = p2;
        }

        for (var i = 0; i < p2; i++)
        {
            data[i + 3] = (int)reader.ReadUIndex();
        }

        var p3 = (int)reader.ReadUIndex();
        {
            data[totalSize - 1] = p3;
        }

        return CreateOpcode(data);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ImageOpcode CreateOpcode(int[] data)
    {
        return new ImageOpcode(
            Ctx: new ImageOpcode.Context(
                Data: data
            )
        );
    }

    [DoesNotReturn]
    [StackTraceHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInvalidOpcode(uint kindValue)
    {
        throw new InvalidDataException($"Got invalid opcode kind: {kindValue}");
    }

    [DoesNotReturn]
    [StackTraceHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInvalidVariableLength(HlOpcodeKind kind)
    {
        throw new InvalidDataException($"Invalid opcode kind for variable-length decoding: {kind}");
    }
}