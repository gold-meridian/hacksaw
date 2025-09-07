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

    private readonly record struct OpCode(PayloadKind[] Payloads, PayloadKind? VariablePayload = null);

    private static readonly OpCode Mov = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register]);
    private static readonly OpCode Int = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.IntIndex]);
    private static readonly OpCode Float = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.FloatIndex]);
    private static readonly OpCode Bool = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Impl]);
    private static readonly OpCode Bytes = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.BytesIndex]);
    private static readonly OpCode String = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.StringIndex]);
    private static readonly OpCode Null = new([PayloadKind.Register | PayloadKind.StoreResult]);
    private static readonly OpCode Add = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode Sub = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode Mul = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode SDiv = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode UDiv = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode SMod = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode UMod = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode Shl = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode SShr = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode UShr = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode And = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode Or = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode Xor = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode Neg = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register]);
    private static readonly OpCode Not = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register]);
    private static readonly OpCode Incr = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register]);
    private static readonly OpCode Decr = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Register]);
    private static readonly OpCode Call0 = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Function]);
    private static readonly OpCode Call1 = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Function, PayloadKind.Register]);
    private static readonly OpCode Call2 = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Function, PayloadKind.Register, PayloadKind.Register | PayloadKind.ExtraParamPointer]);
    private static readonly OpCode Call3 = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Function, PayloadKind.Register, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode Call4 = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Function, PayloadKind.Register, PayloadKind.Register, PayloadKind.Register, PayloadKind.Register]);
    private static readonly OpCode CallN = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Function, PayloadKind.VariableCount], PayloadKind.Register);
    private static readonly OpCode CallMethod = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Proto, PayloadKind.VariableCount, PayloadKind.Register | PayloadKind.TypeProvider], PayloadKind.Register);
    private static readonly OpCode CallThis = new([PayloadKind.Register | PayloadKind.StoreResult, PayloadKind.Proto | PayloadKind.DeclaringOnThis, PayloadKind.VariableCount], PayloadKind.Register);
    private static readonly OpCode CallClosure = new([]);
    private static readonly OpCode StaticClosure = new([]);
    private static readonly OpCode InstanceClosure = new([]);
    private static readonly OpCode VirtualClosure = new([]);
    private static readonly OpCode GetGlobal = new([]);
    private static readonly OpCode SetGlobal = new([]);
    private static readonly OpCode Field = new([]);
    private static readonly OpCode SetField = new([]);
    private static readonly OpCode GetThis = new([]);
    private static readonly OpCode SetThis = new([]);
    private static readonly OpCode DynGet = new([]);
    private static readonly OpCode DynSet = new([]);
    private static readonly OpCode JTrue = new([]);
    private static readonly OpCode JFalse = new([]);
    private static readonly OpCode JNull = new([]);
    private static readonly OpCode JNotNull = new([]);
    private static readonly OpCode JSLt = new([]);
    private static readonly OpCode JSGte = new([]);
    private static readonly OpCode JSGt = new([]);
    private static readonly OpCode JSLte = new([]);
    private static readonly OpCode JULt = new([]);
    private static readonly OpCode JUGte = new([]);
    private static readonly OpCode JNotLt = new([]);
    private static readonly OpCode JNotGte = new([]);
    private static readonly OpCode JEq = new([]);
    private static readonly OpCode JNotEq = new([]);
    private static readonly OpCode JAlways = new([]);
    private static readonly OpCode ToDyn = new([]);
    private static readonly OpCode ToSFloat = new([]);
    private static readonly OpCode ToUFloat = new([]);
    private static readonly OpCode ToInt = new([]);
    private static readonly OpCode SafeCast = new([]);
    private static readonly OpCode UnsafeCast = new([]);
    private static readonly OpCode ToVirtual = new([]);
    private static readonly OpCode Label = new([]);
    private static readonly OpCode Ret = new([]);
    private static readonly OpCode Throw = new([]);
    private static readonly OpCode Rethrow = new([]);
    private static readonly OpCode Switch = new([]);
    private static readonly OpCode NullCheck = new([]);
    private static readonly OpCode Trap = new([]);
    private static readonly OpCode EndTrap = new([]);
    private static readonly OpCode GetI8 = new([]);
    private static readonly OpCode GetI16 = new([]);
    private static readonly OpCode GetMem = new([]);
    private static readonly OpCode GetArray = new([]);
    private static readonly OpCode SetI8 = new([]);
    private static readonly OpCode SetI16 = new([]);
    private static readonly OpCode SetMem = new([]);
    private static readonly OpCode SetArray = new([]);
    private static readonly OpCode New = new([]);
    private static readonly OpCode ArraySize = new([]);
    private static readonly OpCode Type = new([]);
    private static readonly OpCode GetType = new([]);
    private static readonly OpCode GetTID = new([]);
    private static readonly OpCode Ref = new([]);
    private static readonly OpCode Unref = new([]);
    private static readonly OpCode Setref = new([]);
    private static readonly OpCode MakeEnum = new([]);
    private static readonly OpCode EnumAlloc = new([]);
    private static readonly OpCode EnumIndex = new([]);
    private static readonly OpCode EnumField = new([]);
    private static readonly OpCode SetEnumField = new([]);
    private static readonly OpCode Assert = new([]);
    private static readonly OpCode RefData = new([]);
    private static readonly OpCode RefOffset = new([]);
    private static readonly OpCode Nop = new([]);
    private static readonly OpCode Prefetch = new([]); // TODO
    private static readonly OpCode Asm = new([]); // TODO

    public static void Main(string[] args) { }
}