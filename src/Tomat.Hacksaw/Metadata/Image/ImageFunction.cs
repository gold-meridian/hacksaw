using System;

using Tomat.Hacksaw.Metadata.Image.Pooling;

namespace Tomat.Hacksaw.Metadata.Image;

// This representation of an op-code is based on dead-cells-core-modding's
// sharplink fork:
// https://github.com/dead-cells-core-modding/core/blob/main/sources/HashlinkNET.Bytecode/HlFunction.cs#L119
public readonly record struct ImageOpcode(ImageOpcode.Context Ctx)
{
    public readonly record struct Context(int[] Data);
    
    public HlOpcodeKind Kind => (HlOpcodeKind)Data[0];

    public ReadOnlySpan<int> Parameters => Data[1..];

    public ReadOnlySpan<int> Data => Ctx.Data;
}

public readonly record struct ImageFunction(
    int FunctionIndex,
    TypeHandle Type,
    TypeHandle[] VariableTypes,
    ImageOpcode[] Opcodes,
    ImageFunction.Debug[]? Debugs,
    ImageFunction.Assign[]? Assigns
)
{
    public readonly record struct Debug(
        DebugFileHandle? FileName,
        int LineNumber
    );

    public readonly record struct Assign(
        StringHandle Name,
        int Index
    );
}