using System.Diagnostics.CodeAnalysis;
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
    public Semantic<Node> Parent => Ast.GetParent(this)!;
    
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
    public Semantic<IScope> Scope { get; internal set; }

    /// <summary>
    /// Returns an empty collection if the passed in node is null,
    /// otherwise returns a singleton collection containing the node.
    /// </summary>
    protected IEnumerable<Node> EmptyIfNull(Node? node) =>
        node is not null
            ? [node]
            : [];
}

public sealed class Root : BlockExpression;

public sealed class Identifier : Node
{
    public required string Name { get; init; }

    public override IEnumerable<Node> Children => [];
}

public abstract class Statement : Node;

public sealed class Parameter : Node
{
    public required bool IsMutable { get; init; }
    
    public required Identifier Identifier { get; init; }

    public Semantic<ParameterSymbol> Symbol { get; internal set; }

    public override IEnumerable<Node> Children => [Identifier];
}

public abstract class Declaration : Statement;

public sealed class FunctionDeclaration : Declaration
{
    public required Identifier Identifier { get; init; }
    
    public required ImmutableArray<Parameter> Parameters { get; init; }
    
    public required Expression? ExpressionBody { get; init; }
    
    public required BlockExpression? BlockBody { get; init; }

    public Semantic<NomialFunction> Symbol { get; internal set; }

    public override IEnumerable<Node> Children => [
        Identifier,
        ..Parameters,
        ..EmptyIfNull(ExpressionBody),
        ..EmptyIfNull(BlockBody)
    ];
}

public sealed class LetDeclaration : Declaration
{
    public required bool IsMutable { get; init; }
    
    public required Identifier Identifier { get; init; }
    
    public required Expression Expression { get; init; }

    public Semantic<VariableSymbol> Symbol { get; internal set; }

    public override IEnumerable<Node> Children => [Identifier, Expression];
}

public sealed class AssignmentStatement : Statement
{
    public required Expression Target { get; init; }
    
    public required Expression Value { get; init; }

    public override IEnumerable<Node> Children => [Target, Value];
}

public sealed class ExpressionStatement : Statement
{
    public required Expression Expression { get; init; }

    public override IEnumerable<Node> Children => [Expression];
}

public abstract class Expression : Node;

public sealed class ErrorExpression : Expression
{
    public override IEnumerable<Node> Children => [];
}

public class BlockExpression : Expression
{
    public required ImmutableArray<Statement> Statements { get; init; }
    
    public required Expression? TrailingExpression { get; init; }

    public override IEnumerable<Node> Children => [..Statements, ..EmptyIfNull(TrailingExpression)];
}

public sealed class CallExpression : Expression
{
    public required Expression Target { get; init; }
    
    public required ImmutableArray<Expression> Arguments { get; init; }

    public override IEnumerable<Node> Children => [Target, ..Arguments];
}

public sealed class LambdaExpression : Expression
{
    public required ImmutableArray<Parameter> Parameters { get; init; }
    
    public required Expression Body { get; init; }

    public override IEnumerable<Node> Children => [..Parameters, Body];
}

public sealed class TupleExpression : Expression
{
    public required ImmutableArray<Expression> Expressions { get; init; }

    public override IEnumerable<Node> Children => Expressions;
}

public sealed class IfExpression : Expression
{
    public required Expression Condition { get; init; }
    
    public required BlockExpression IfTrue { get; init; }
    
    public required BlockExpression IfFalse { get; init; }

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
    
    public Semantic<FunctionOrLambda?> Function { get; internal set; }
}

public sealed class BreakExpression : Expression
{
    public required Expression? Expression { get; init; }

    public override IEnumerable<Node> Children =>
        Expression is not null
            ? [Expression]
            : [];
    
    public Semantic<LoopExpression?> Loop { get; internal set; }
}

public sealed class ContinueExpression : Expression
{
    public override IEnumerable<Node> Children => [];
    
    public Semantic<LoopExpression?> Loop { get; internal set; }
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

    public Semantic<ISymbol> ReferencedSymbol { get; internal set; }

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
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
}
