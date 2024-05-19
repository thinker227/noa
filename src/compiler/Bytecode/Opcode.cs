namespace Noa.Compiler.Bytecode;

/// <summary>
/// An op-code.
/// </summary>
internal enum Opcode : byte
{
    NoOp = 0x0,

    // Control flow
    Jump = 0x1,
    JumpIf = 0x2,
    Call = 0x3,
    Ret = 0x4,

    // Stack operations
    PushInt = 0x14,
    PushBool = 0x15,
    PushFunc = 0x16,
    PushNil = 0x17,
    Pop = 0x32,
    Dup = 0x33,
    Swap = 0x34,

    // Locals operations
    StoreVar = 0x46,
    LoadVar = 0x47,

    // Value operations
    Add = 0x64,
    Sub = 0x65,
    Mult = 0x66,
    Div = 0x67,
    Equal = 0x68,
    LessThan = 0x69,
    Not = 0x6A,
    And = 0x6B,
    Or = 0x6C,
    GreaterThan = 0x6D,
}
