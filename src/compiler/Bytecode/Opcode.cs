namespace Noa.Compiler.Bytecode;

/// <summary>
/// An op-code.
/// </summary>
internal enum Opcode : byte
{
    NoOp = 0,

    // Control flow
    Jump = 1,
    JumpIf = 2,
    Call = 3,
    Ret = 4,

    // Stack operations
    PushInt = 20,
    PushBool = 21,
    PushFunc = 22,
    PushNil = 23,
    Pop = 50,
    Dup = 51,
    Swap = 52,

    // Locals operations
    StoreVar = 70,
    LoadVar = 71,

    // Value operations
    Add = 100,
    Sub = 101,
    Mult = 102,
    Div = 103,
    Equal = 104,
    LessThan = 105,
    Not = 106,
    And = 107,
    Or = 108,
    GreaterThan = 109,
}
