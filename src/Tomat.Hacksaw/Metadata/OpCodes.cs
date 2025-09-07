namespace Tomat.Hacksaw.Metadata;

public enum HlOpcodeKind
{
    Mov,
    Int,
    Float,
    Bool,
    Bytes,
    String,
    Null,

    Add,
    Sub,
    Mul,
    SDiv,
    UDiv,
    SMod,
    UMod,
    Shl,
    SShr,
    UShr,
    And,
    Or,
    Xor,

    Neg,
    Not,
    Incr,
    Decr,

    Call0,
    Call1,
    Call2,
    Call3,
    Call4,
    CallN,
    CallMethod,
    CallThis,
    CallClosure,

    StaticClosure,
    InstanceClosure,
    VirtualClosure,

    GetGlobal,
    SetGlobal,
    Field,
    SetField,
    GetThis,
    SetThis,
    DynGet,
    DynSet,

    JTrue,
    JFalse,
    JNull,
    JNotNull,
    JSLt,
    JSGte,
    JSGt,
    JSLte,
    JULt,
    JUGte,
    JNotLt,
    JNotGte,
    JEq,
    JNotEq,
    JAlways,

    ToDyn,
    ToSFloat,
    ToUFloat,
    ToInt,
    SafeCast,
    UnsafeCast,
    ToVirtual,

    Label,
    Ret,
    Throw,
    Rethrow,
    Switch,
    NullCheck,
    Trap,
    EndTrap,

    GetI8,
    GetI16,
    GetMem,
    GetArray,
    SetI8,
    SetI16,
    SetMem,
    SetArray,

    New,
    ArraySize,
    Type,
    GetType,
    GetTID,

    Ref,
    Unref,
    Setref,

    MakeEnum,
    EnumAlloc,
    EnumIndex,
    EnumField,
    SetEnumField,

    Assert,
    RefData,
    RefOffset,
    Nop,
    
    // TODO: Dumb and annoying
    Prefetch,
    Asm,

    Last,
}

public interface IHlOpcode
{
    static abstract int Arity { get; }
}

public class OpCodes
{
    
}