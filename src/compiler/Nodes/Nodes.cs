using Noa.Compiler.Symbols;

namespace Noa.Compiler.Nodes;

/// <summary>
/// An abstract syntax node.
/// </summary>
public abstract class Node
{
    /// <summary>
    /// The AST the node belongs to.
    /// </summary>
    public required Ast Ast { get; init; }

    /// <summary>
    /// The parent of the node, or null if the node is the root node.
    /// </summary>
    public Node Parent => Ast.GetParent(this)!;
    
    /// <summary>
    /// The source location of the node.
    /// </summary>
    public required Location Location { get; init; }
    
    /// <summary>
    /// The child nodes of the node.
    /// </summary>
    public abstract IEnumerable<Node> Children { get; }
    
    /// <summary>
    /// The semantic scope of the node.
    /// </summary>
    public Scope Scope { get; internal set; } = null!;
}

public sealed class Root : Node
{
    public required ImmutableArray<Statement> Statements { get; init; }

    public override IEnumerable<Node> Children => Statements;
}

public sealed class Identifier : Node
{
    public required string Name { get; init; }

    public override IEnumerable<Node> Children => [];
}

public sealed class Statement : Node
{
    public Declaration? Declaration { get; init; }
    
    public Expression? Expression { get; init; }

    public override IEnumerable<Node> Children => (Declaration, Expression) switch
    {
        (null, null) => [],
        (not null, null) => [Declaration],
        (null, not null) => [Expression],
        (not null, not null) => [Declaration, Expression],
    };
}

public abstract class Declaration : Node;

public sealed class LetDeclaration : Declaration
{
    public required bool IsMutable { get; init; }
    
    public required Identifier Identifier { get; init; }
    
    public required Expression Expression { get; init; }

    public VariableSymbol Symbol { get; internal set; } = null!;

    public override IEnumerable<Node> Children => [Identifier, Expression];
}

public abstract class Expression : Node;

public sealed class BlockExpression : Expression
{
    public required ImmutableArray<Statement> Statements { get; init; }
    
    public required Expression? TrailingExpression { get; init; }

    public override IEnumerable<Node> Children => [..Statements, TrailingExpression];
}

public sealed class CallExpression : Expression
{
    public required Expression Target { get; init; }
    
    public required ImmutableArray<Expression> Arguments { get; init; }

    public override IEnumerable<Node> Children => [Target, ..Arguments];
}

public sealed class FunctionExpression : Expression
{
    public required ImmutableArray<Parameter> Parameters { get; init; }
    
    public required Expression Body { get; init; }

    public override IEnumerable<Node> Children => [..Parameters, Body];
}

public sealed class Parameter : Node
{
    public required bool IsMutable { get; init; }
    
    public required Identifier Identifier { get; init; }

    public ParameterSymbol Symbol { get; internal set; } = null!;

    public override IEnumerable<Node> Children => [Identifier];
}

public sealed class IfExpression : Expression
{
    public required Expression Condition { get; init; }
    
    public required Expression IfTrue { get; init; }
    
    public required Expression IfFalse { get; init; }

    public override IEnumerable<Node> Children => [Condition, IfTrue, IfFalse];
}

public sealed class LoopExpression : Expression
{
    public required BlockExpression Block { get; init; }

    public override IEnumerable<Node> Children => [Block];
}

public sealed class ReturnExpression : Expression
{
    public required Expression? Expression { get; init; }

    public override IEnumerable<Node> Children =>
        Expression is not null
            ? [Expression]
            : [];
}

public sealed class BreakExpression : Expression
{
    public required Expression? Expression { get; init; }

    public override IEnumerable<Node> Children =>
        Expression is not null
            ? [Expression]
            : [];
}

public sealed class ContinueExpression : Expression
{
    public override IEnumerable<Node> Children => [];
}

public sealed class UnaryExpression : Expression
{
    public required UnaryKind Kind { get; init; }
    
    public required Expression Operand { get; init; }

    public override IEnumerable<Node> Children => [Operand];
}

public sealed class BinaryExpression : Expression
{
    public required Expression Left { get; init; }
    
    public required BinaryKind Kind { get; init; }
    
    public required Expression Right { get; init; }

    public override IEnumerable<Node> Children => [Left, Right];
}

public sealed class IdentifierExpression : Expression
{
    public required string Identifier { get; init; }

    public ISymbol ReferencedSymbol { get; internal set; } = null!;

    public override IEnumerable<Node> Children => [];
}

public sealed class StringExpression : Expression
{
    public required string Value { get; init; }

    public override IEnumerable<Node> Children => [];
}

public sealed class BoolExpression : Expression
{
    public required bool Value { get; init; }

    public override IEnumerable<Node> Children => [];
}

public sealed class NumberExpression : Expression
{
    public required int Value { get; init; }

    public override IEnumerable<Node> Children => [];
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
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
}
