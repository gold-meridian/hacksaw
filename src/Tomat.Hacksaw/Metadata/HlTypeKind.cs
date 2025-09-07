namespace Tomat.Hacksaw.Metadata;

public enum HlTypeKind
{
    Void = 0,
    U8 = 1,
    U16 = 2,
    I32 = 3,
    I64 = 4,
    F32 = 5,
    F64 = 6,
    Bool = 7,
    Bytes = 8,
    Dyn = 9,
    Fun = 10,
    Obj = 11,
    Array = 12,
    Type = 13,
    Ref = 14,
    Virtual = 15,
    DynObj = 16,
    Abstract = 17,
    Enum = 18,
    Null = 19,
    Method = 20,
    Struct = 21,
    Packed = 22,
    Guid = 23, // TODO

    Last = 24,
}