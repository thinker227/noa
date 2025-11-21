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
    EnterTempFrame = 0x5,
    ExitTempFrame = 0x6,

    // Stack operations
    PushFloat = 0x14,
    PushBool = 0x15,
    PushFunc = 0x16,
    PushNil = 0x17,
    PushString = 0x18,
    PushObject = 0x19,
    PushList = 0x20,
    Pop = 0x32,
    Dup = 0x33,
    Swap = 0x34,

    // Locals operations
    StoreVar = 0x46,
    LoadVar = 0x47,
    StoreVarBoxed = 0x48,

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
    Concat = 0x6E,
    ToString = 0x6F,
    AddField = 0x70,
    WriteField = 0x71,
    ReadField = 0x72,
    AppendElement = 0x73,
    WriteElement = 0x74,
    ReadElement = 0x75,
    
    // Operations instructions
    Boundary = 0xFF,
}
