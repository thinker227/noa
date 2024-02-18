namespace Noa.Compiler.Bytecode;

/// <summary>
/// A bytecode op-code.
/// </summary>
public enum OpCode : byte
{
    NoOp = 0,

    // Control flow
    Jump,
    JumpIf,
    Call,
    Ret,

    // Stack operations
    PushInt,
    PushBool,
    Pop,
    Dup,

    // Locals operations
    StoreVar,
    LoadVar,

    // Value operations
    Add,
    Sub,
    Mult,
    Div,
    Equal,
    LessThan,
    Not,
    And,
    Or,
}
