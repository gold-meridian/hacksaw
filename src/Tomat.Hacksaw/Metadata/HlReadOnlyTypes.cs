using Tomat.Hacksaw.Metadata.Image;

namespace Tomat.Hacksaw.Metadata;

/// <summary>
///     A read-only HashLink type definition to be used in a
///     <see cref="HlImage"/>.
/// </summary>
public interface IReadOnlyHlType
{
    /// <summary>
    ///     The kind of type this type represents.
    /// </summary>
    HlTypeKind TypeKind { get; }
}

/// <summary>
///     A <see cref="IReadOnlyHlType"/> that is named.
/// </summary>
public interface INamedHlType
{
    /// <summary>
    ///     The type's name.
    /// </summary>
    string Name { get; }
}

public readonly record struct HlTypeRecord
{
    
}