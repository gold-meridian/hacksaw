namespace Tomat.Hacksaw.Metadata;

public readonly record struct IntHandle(RowId Id);

public readonly record struct FloatHandle(RowId Id);

public readonly record struct StringHandle(RowId Id);

public readonly record struct ByteHandle(RowId Id);

public readonly record struct DebugHandle(RowId Id);

public readonly record struct TypeHandle(RowId Id);

public readonly record struct GlobalHandle(RowId Id);

public readonly record struct NativeHandle(RowId Id);

public readonly record struct FunctionHandle(RowId Id);

public readonly record struct ConstantHandle(RowId Id);