using Vogen;

namespace Tomat.Hacksaw.Metadata.Image.Pooling;

public interface IHandle<out THandle>
    where THandle : IHandle<THandle>
{
    int Value { get; }

    static abstract THandle DangerouslyCreateHandleForPool(int value);
}

[ValueObject<int>]
public readonly partial record struct IntHandle : IHandle<IntHandle>
{
    public static bool operator <(IntHandle left, IntHandle right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(IntHandle left, IntHandle right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(IntHandle left, IntHandle right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(IntHandle left, IntHandle right)
    {
        return left.CompareTo(right) >= 0;
    }

    private static int NormalizeInput(int input)
    {
        return input;
    }

    private static Validation Validate(int input)
    {
        return input >= 0 ? Validation.Ok : Validation.Invalid("Handle cannot reference negative element");
    }

    public static IntHandle DangerouslyCreateHandleForPool(int value)
    {
        return From(value);
    }
}

[ValueObject<int>]
public readonly partial record struct FloatHandle : IHandle<FloatHandle>
{
    public static bool operator <(FloatHandle left, FloatHandle right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(FloatHandle left, FloatHandle right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(FloatHandle left, FloatHandle right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(FloatHandle left, FloatHandle right)
    {
        return left.CompareTo(right) >= 0;
    }

    private static int NormalizeInput(int input)
    {
        return input;
    }

    private static Validation Validate(int input)
    {
        return input >= 0 ? Validation.Ok : Validation.Invalid("Handle cannot reference negative element");
    }

    public static FloatHandle DangerouslyCreateHandleForPool(int value)
    {
        return From(value);
    }
}

[ValueObject<int>]
public readonly partial record struct StringHandle : IHandle<StringHandle>
{
    public static bool operator <(StringHandle left, StringHandle right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(StringHandle left, StringHandle right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(StringHandle left, StringHandle right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(StringHandle left, StringHandle right)
    {
        return left.CompareTo(right) >= 0;
    }

    private static int NormalizeInput(int input)
    {
        return input;
    }

    private static Validation Validate(int input)
    {
        return input >= 0 ? Validation.Ok : Validation.Invalid("Handle cannot reference negative element");
    }

    public static StringHandle DangerouslyCreateHandleForPool(int value)
    {
        return From(value);
    }
}

[ValueObject<int>]
public readonly partial record struct ByteHandle : IHandle<ByteHandle>
{
    public static bool operator <(ByteHandle left, ByteHandle right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(ByteHandle left, ByteHandle right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(ByteHandle left, ByteHandle right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(ByteHandle left, ByteHandle right)
    {
        return left.CompareTo(right) >= 0;
    }

    private static int NormalizeInput(int input)
    {
        return input;
    }

    private static Validation Validate(int input)
    {
        return input >= 0 ? Validation.Ok : Validation.Invalid("Handle cannot reference negative element");
    }

    public static ByteHandle DangerouslyCreateHandleForPool(int value)
    {
        return From(value);
    }
}

[ValueObject<int>]
public readonly partial record struct DebugFileHandle : IHandle<DebugFileHandle>
{
    public static bool operator <(DebugFileHandle left, DebugFileHandle right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(DebugFileHandle left, DebugFileHandle right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(DebugFileHandle left, DebugFileHandle right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(DebugFileHandle left, DebugFileHandle right)
    {
        return left.CompareTo(right) >= 0;
    }

    private static int NormalizeInput(int input)
    {
        return input;
    }

    private static Validation Validate(int input)
    {
        return input >= 0 ? Validation.Ok : Validation.Invalid("Handle cannot reference negative element");
    }

    public static DebugFileHandle DangerouslyCreateHandleForPool(int value)
    {
        return From(value);
    }
}

[ValueObject<int>]
public readonly partial record struct TypeHandle : IHandle<TypeHandle>
{
    public static bool operator <(TypeHandle left, TypeHandle right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(TypeHandle left, TypeHandle right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(TypeHandle left, TypeHandle right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(TypeHandle left, TypeHandle right)
    {
        return left.CompareTo(right) >= 0;
    }

    private static int NormalizeInput(int input)
    {
        return input;
    }

    private static Validation Validate(int input)
    {
        return input >= 0 ? Validation.Ok : Validation.Invalid("Handle cannot reference negative element");
    }

    public static TypeHandle DangerouslyCreateHandleForPool(int value)
    {
        return From(value);
    }
}

[ValueObject<int>]
public readonly partial record struct GlobalHandle : IHandle<GlobalHandle>
{
    public static bool operator <(GlobalHandle left, GlobalHandle right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(GlobalHandle left, GlobalHandle right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(GlobalHandle left, GlobalHandle right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(GlobalHandle left, GlobalHandle right)
    {
        return left.CompareTo(right) >= 0;
    }

    private static int NormalizeInput(int input)
    {
        return input;
    }

    private static Validation Validate(int input)
    {
        return input >= 0 ? Validation.Ok : Validation.Invalid("Handle cannot reference negative element");
    }

    public static GlobalHandle DangerouslyCreateHandleForPool(int value)
    {
        return From(value);
    }
}

[ValueObject<int>]
public readonly partial record struct NativeHandle : IHandle<NativeHandle>
{
    public static bool operator <(NativeHandle left, NativeHandle right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(NativeHandle left, NativeHandle right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(NativeHandle left, NativeHandle right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(NativeHandle left, NativeHandle right)
    {
        return left.CompareTo(right) >= 0;
    }

    private static int NormalizeInput(int input)
    {
        return input;
    }

    private static Validation Validate(int input)
    {
        return input >= 0 ? Validation.Ok : Validation.Invalid("Handle cannot reference negative element");
    }

    public static NativeHandle DangerouslyCreateHandleForPool(int value)
    {
        return From(value);
    }
}

[ValueObject<int>]
public readonly partial record struct FunctionHandle : IHandle<FunctionHandle>
{
    public static bool operator <(FunctionHandle left, FunctionHandle right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(FunctionHandle left, FunctionHandle right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(FunctionHandle left, FunctionHandle right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(FunctionHandle left, FunctionHandle right)
    {
        return left.CompareTo(right) >= 0;
    }

    private static int NormalizeInput(int input)
    {
        return input;
    }

    private static Validation Validate(int input)
    {
        return input >= 0 ? Validation.Ok : Validation.Invalid("Handle cannot reference negative element");
    }

    public static FunctionHandle DangerouslyCreateHandleForPool(int value)
    {
        return From(value);
    }
}

[ValueObject<int>]
public readonly partial record struct ConstantHandle : IHandle<ConstantHandle>
{
    public static bool operator <(ConstantHandle left, ConstantHandle right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(ConstantHandle left, ConstantHandle right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(ConstantHandle left, ConstantHandle right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(ConstantHandle left, ConstantHandle right)
    {
        return left.CompareTo(right) >= 0;
    }

    private static int NormalizeInput(int input)
    {
        return input;
    }

    private static Validation Validate(int input)
    {
        return input >= 0 ? Validation.Ok : Validation.Invalid("Handle cannot reference negative element");
    }

    public static ConstantHandle DangerouslyCreateHandleForPool(int value)
    {
        return From(value);
    }
}