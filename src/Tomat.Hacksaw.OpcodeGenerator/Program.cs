using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Tomat.Hacksaw.OpcodeGenerator;

// Op-code definitions here are taken from
// https://github.com/dead-cells-core-modding/core/blob/main/sources/HashlinkNET.Bytecode/OpCodeParser/HlOpCode.cs

internal static class Program
{
    [Flags]
    private enum PayloadKind
    {
        None = 0,

        VariableCount = 1 << 0,

        Register = 1 << 1,
        Type = 1 << 2,
        Function = 1 << 3,
        Offset = 1 << 4,

        Impl = 1 << 5,
        IntIndex = (1 << 6) | IndexedConstant,
        FloatIndex = (1 << 7) | IndexedConstant,
        StringIndex = (1 << 8) | IndexedConstant,
        BytesIndex = (1 << 9) | IndexedConstant,
        GlobalIndex = 1 << 10,

        EnumFieldIndex = 1 << 11,

        Field = (1 << 12) | RequestTypeInfo,
        Proto = (1 << 13) | RequestTypeInfo,

        // Flags
        TypeProvider = 1 << 16,
        DeclaringOnThis = 1 << 17,
        RequestTypeInfo = 1 << 18,
        IndexedConstant = 1 << 19,
        ExtraParamPointer = 1 << 20,
        StoreResult = 1 << 21,
    }

    private readonly record struct Opcode(PayloadKind[] Payloads, PayloadKind? VariablePayload = null);

    private static readonly Opcode Mov = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register]);
    private static readonly Opcode Int = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.IntIndex]);
    private static readonly Opcode Float = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.FloatIndex]);
    private static readonly Opcode Bool = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Impl]);
    private static readonly Opcode Bytes = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.BytesIndex]);
    private static readonly Opcode String = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.StringIndex]);
    private static readonly Opcode Null = new([PayloadKind.Register | PayloadKind.StoreResult]);
    private static readonly Opcode Add = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode Sub = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode Mul = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode SDiv = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode UDiv = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode SMod = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode UMod = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode Shl = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode SShr = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode UShr = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode And = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode Or = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode Xor = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode Neg = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register]);
    private static readonly Opcode Not = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register]);
    private static readonly Opcode Incr = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register]);
    private static readonly Opcode Decr = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register]);
    private static readonly Opcode Call0 = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Function]);
    private static readonly Opcode Call1 = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Function, PayloadKind.Register]);
    private static readonly Opcode Call2 = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Function, PayloadKind.Register, PayloadKind.Register | PayloadKind.ExtraParamPointer]);
    private static readonly Opcode Call3 = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Function, PayloadKind.Register, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode Call4 = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Function, PayloadKind.Register, PayloadKind.Register, PayloadKind.Register, PayloadKind.Register]);
    private static readonly Opcode CallN = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Function, PayloadKind.VariableCount], PayloadKind.Register);
    private static readonly Opcode CallMethod = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Proto, PayloadKind.VariableCount, PayloadKind.Register | PayloadKind.TypeProvider], PayloadKind.Register);
    private static readonly Opcode CallThis = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Proto | PayloadKind.DeclaringOnThis, PayloadKind.VariableCount], PayloadKind.Register);
    private static readonly Opcode CallClosure = new([]);
    private static readonly Opcode StaticClosure = new([]);
    private static readonly Opcode InstanceClosure = new([]);
    private static readonly Opcode VirtualClosure = new([]);
    private static readonly Opcode GetGlobal = new([]);
    private static readonly Opcode SetGlobal = new([]);
    private static readonly Opcode Field = new([]);
    private static readonly Opcode SetField = new([]);
    private static readonly Opcode GetThis = new([]);
    private static readonly Opcode SetThis = new([]);
    private static readonly Opcode DynGet = new([]);
    private static readonly Opcode DynSet = new([]);
    private static readonly Opcode JTrue = new([]);
    private static readonly Opcode JFalse = new([]);
    private static readonly Opcode JNull = new([]);
    private static readonly Opcode JNotNull = new([]);
    private static readonly Opcode JSLt = new([]);
    private static readonly Opcode JSGte = new([]);
    private static readonly Opcode JSGt = new([]);
    private static readonly Opcode JSLte = new([]);
    private static readonly Opcode JULt = new([]);
    private static readonly Opcode JUGte = new([]);
    private static readonly Opcode JNotLt = new([]);
    private static readonly Opcode JNotGte = new([]);
    private static readonly Opcode JEq = new([]);
    private static readonly Opcode JNotEq = new([]);
    private static readonly Opcode JAlways = new([]);
    private static readonly Opcode ToDyn = new([]);
    private static readonly Opcode ToSFloat = new([]);
    private static readonly Opcode ToUFloat = new([]);
    private static readonly Opcode ToInt = new([]);
    private static readonly Opcode SafeCast = new([]);
    private static readonly Opcode UnsafeCast = new([]);
    private static readonly Opcode ToVirtual = new([]);
    private static readonly Opcode Label = new([]);
    private static readonly Opcode Ret = new([]);
    private static readonly Opcode Throw = new([]);
    private static readonly Opcode Rethrow = new([]);
    private static readonly Opcode Switch = new([]);
    private static readonly Opcode NullCheck = new([]);
    private static readonly Opcode Trap = new([]);
    private static readonly Opcode EndTrap = new([]);
    private static readonly Opcode GetI8 = new([]);
    private static readonly Opcode GetI16 = new([]);
    private static readonly Opcode GetMem = new([]);
    private static readonly Opcode GetArray = new([]);
    private static readonly Opcode SetI8 = new([]);
    private static readonly Opcode SetI16 = new([]);
    private static readonly Opcode SetMem = new([]);
    private static readonly Opcode SetArray = new([]);
    private static readonly Opcode New = new([]);
    private static readonly Opcode ArraySize = new([]);
    private static readonly Opcode Type = new([]);
    private static readonly Opcode GetType = new([]);
    private static readonly Opcode GetTID = new([]);
    private static readonly Opcode Ref = new([]);
    private static readonly Opcode Unref = new([]);
    private static readonly Opcode Setref = new([]);
    private static readonly Opcode MakeEnum = new([]);
    private static readonly Opcode EnumAlloc = new([]);
    private static readonly Opcode EnumIndex = new([]);
    private static readonly Opcode EnumField = new([]);
    private static readonly Opcode SetEnumField = new([]);
    private static readonly Opcode Assert = new([]);
    private static readonly Opcode RefData = new([]);
    private static readonly Opcode RefOffset = new([]);
    private static readonly Opcode Nop = new([]);
    private static readonly Opcode Prefetch = new([]); // TODO
    private static readonly Opcode Asm = new([]); // TODO

    public static void Main(string[] args) { }
}