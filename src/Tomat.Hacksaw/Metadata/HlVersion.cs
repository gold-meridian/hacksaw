using Tomat.Hacksaw.IO;

using Vogen;

namespace Tomat.Hacksaw.Metadata;

[ValueObject<int>]
[Instance("MIN_VERSION", min_version)]
[Instance("MAX_VERSION", max_version)]
[Instance("FEATURE_BYTES", 5)]
[Instance("FEATURE_FUNC_ASSIGNS", 3)]
public readonly partial record struct HlVersion
{
    private const int min_version = 2;
    private const int max_version = 5;

    public static HlVersion Read<TByteReader>(ref TByteReader reader)
        where TByteReader : IByteReader, allows ref struct
    {
        var version = reader.ReadByte();
        return From(version);
    }
    
    public static bool operator <(HlVersion left, HlVersion right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(HlVersion left, HlVersion right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(HlVersion left, HlVersion right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(HlVersion left, HlVersion right)
    {
        return left.CompareTo(right) >= 0;
    }
    
    private static int NormalizeInput(int input)
    {
        return input;
    }
    
    private static Validation Validate(int input)
    {
        if (input < min_version)
        {
            return Validation.Invalid($"'{input}' is less than the minimum version '{min_version}'");
        }
        
        if (input > max_version)
        {
            return Validation.Invalid($"'{input}' is less than the maximum version '{max_version}'");
        }
        
        return Validation.Ok;
    }
}