using System.Runtime.CompilerServices;

namespace Tomat.Hacksaw.Metadata.Image.Pooling;

public interface IHandle<out THandle>
    where THandle : IHandle<THandle>
{
    int Value { get; }

    static abstract THandle From(int value);
}

public readonly record struct IntHandle(int Value) : IHandle<IntHandle>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntHandle From(int value)
    {
        return new IntHandle(value);
    }
}

public readonly record struct FloatHandle(int Value) : IHandle<FloatHandle>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FloatHandle From(int value)
    {
        return new FloatHandle(value);
    }
}

public readonly record struct StringHandle(int Value) : IHandle<StringHandle>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringHandle From(int value)
    {
        return new StringHandle(value);
    }
}

public readonly record struct ByteHandle(int Value) : IHandle<ByteHandle>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ByteHandle From(int value)
    {
        return new ByteHandle(value);
    }
}

public readonly record struct DebugFileHandle(int Value) : IHandle<DebugFileHandle>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DebugFileHandle From(int value)
    {
        return new DebugFileHandle(value);
    }
}

public readonly record struct TypeHandle(int Value) : IHandle<TypeHandle>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeHandle From(int value)
    {
        return new TypeHandle(value);
    }
}

public readonly record struct GlobalHandle(int Value) : IHandle<GlobalHandle>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GlobalHandle From(int value)
    {
        return new GlobalHandle(value);
    }
}

public readonly record struct NativeHandle(int Value) : IHandle<NativeHandle>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeHandle From(int value)
    {
        return new NativeHandle(value);
    }
}

public readonly record struct FunctionHandle(int Value) : IHandle<FunctionHandle>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FunctionHandle From(int value)
    {
        return new FunctionHandle(value);
    }
}

public readonly record struct ConstantHandle(int Value) : IHandle<ConstantHandle>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConstantHandle From(int value)
    {
        return new ConstantHandle(value);
    }
}