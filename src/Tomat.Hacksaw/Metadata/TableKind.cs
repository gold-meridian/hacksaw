namespace Tomat.Hacksaw.Metadata;

/// <summary>
///     Known kinds of tables.
/// </summary>
public enum TableKind : byte
{
    Int = 0,
    Float = 1,
    String = 2,
    Byte = 3,
    Debug = 4,
    Type = 5,
    Global = 6,
    Native = 7,
    Function = 8,
    Constant = 9,
    
    Raw = 255,
}