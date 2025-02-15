namespace Noa.Compiler;

public enum AssignmentKind
{
    Assign,
    Plus,
    Minus,
    Mult,
    Div,
}

public enum UnaryKind
{
    Identity,
    Negate,
    Not,
}

public enum BinaryKind
{
    Plus,
    Minus,
    Mult,
    Div,
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
}
