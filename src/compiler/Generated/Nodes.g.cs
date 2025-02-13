// <auto-generated/>

#nullable enable

namespace Noa.Compiler.Nodes;

public sealed partial class Root : Node
{
    public required BlockExpression Block { get; init; }

    public override IEnumerable<Node> Children => [Block];
}

public sealed partial class Identifier : Node
{
    public required string Name { get; init; }

    public override IEnumerable<Node> Children => [];
}

public abstract partial class Statement : Node;

public sealed partial class Parameter : Node
{
    public required bool IsMutable { get; init; }

    public required Identifier Identifier { get; init; }

    public override IEnumerable<Node> Children => [Identifier];
}

public abstract partial class Declaration : Statement;

public sealed partial class FunctionDeclaration : Declaration
{
    public required Token FuncKeyword { get; init; }

    public required Identifier Identifier { get; init; }

    public required ImmutableArray<Parameter> Parameters { get; init; }

    public required Expression? ExpressionBody { get; init; }

    public required BlockExpression? BlockBody { get; init; }

    public override IEnumerable<Node> Children => [Identifier, ..Parameters, ..EmptyIfNull(ExpressionBody), ..EmptyIfNull(BlockBody)];
}

public sealed partial class LetDeclaration : Declaration
{
    public required Token LetKeyword { get; init; }

    public required bool IsMutable { get; init; }

    public required Identifier Identifier { get; init; }

    public required Expression Expression { get; init; }

    public override IEnumerable<Node> Children => [Identifier, Expression];
}

public sealed partial class AssignmentStatement : Statement
{
    public required Expression Target { get; init; }

    public required AssignmentKind Kind { get; init; }

    public required Expression Value { get; init; }

    public override IEnumerable<Node> Children => [Target, Value];
}

public sealed partial class ExpressionStatement : Statement
{
    public required Expression Expression { get; init; }

    public override IEnumerable<Node> Children => [Expression];
}

public abstract partial class Expression : Node;

public sealed partial class ErrorExpression : Expression
{
    public override IEnumerable<Node> Children => [];
}

public sealed partial class BlockExpression : Expression
{
    public required ImmutableArray<Statement> Statements { get; init; }

    public required Expression? TrailingExpression { get; init; }

    public override IEnumerable<Node> Children => [..Statements, ..EmptyIfNull(TrailingExpression)];
}

public sealed partial class CallExpression : Expression
{
    public required Expression Target { get; init; }

    public required ImmutableArray<Expression> Arguments { get; init; }

    public override IEnumerable<Node> Children => [Target, ..Arguments];
}

public sealed partial class LambdaExpression : Expression
{
    public required ImmutableArray<Parameter> Parameters { get; init; }

    public required Token ArrowToken { get; init; }

    public required Expression Body { get; init; }

    public override IEnumerable<Node> Children => [..Parameters, Body];
}

public sealed partial class TupleExpression : Expression
{
    public required ImmutableArray<Expression> Expressions { get; init; }

    public override IEnumerable<Node> Children => [..Expressions];
}

public sealed partial class IfExpression : Expression
{
    public required Token IfKeyword { get; init; }

    public required Expression Condition { get; init; }

    public required BlockExpression IfTrue { get; init; }

    public required ElseClause? Else { get; init; }

    public override IEnumerable<Node> Children => [Condition, IfTrue, ..EmptyIfNull(Else)];
}

public sealed partial class ElseClause : Node
{
    public required Token ElseKeyword { get; init; }

    public required BlockExpression IfFalse { get; init; }

    public override IEnumerable<Node> Children => [IfFalse];
}

public sealed partial class LoopExpression : Expression
{
    public required Token LoopKeyword { get; init; }

    public required BlockExpression Block { get; init; }

    public override IEnumerable<Node> Children => [Block];
}

public sealed partial class ReturnExpression : Expression
{
    public required Token ReturnKeyword { get; init; }

    public required Expression? Expression { get; init; }

    public override IEnumerable<Node> Children => [..EmptyIfNull(Expression)];
}

public sealed partial class BreakExpression : Expression
{
    public required Token BreakKeyword { get; init; }

    public required Expression? Expression { get; init; }

    public override IEnumerable<Node> Children => [..EmptyIfNull(Expression)];
}

public sealed partial class ContinueExpression : Expression
{
    public override IEnumerable<Node> Children => [];
}

public sealed partial class UnaryExpression : Expression
{
    public required UnaryKind Kind { get; init; }

    public required Expression Operand { get; init; }

    public override IEnumerable<Node> Children => [Operand];
}

public sealed partial class BinaryExpression : Expression
{
    public required Expression Left { get; init; }

    public required BinaryKind Kind { get; init; }

    public required Expression Right { get; init; }

    public override IEnumerable<Node> Children => [Left, Right];
}

public sealed partial class IdentifierExpression : Expression
{
    public required string Identifier { get; init; }

    public override IEnumerable<Node> Children => [];
}

public sealed partial class StringExpression : Expression
{
    public required ImmutableArray<StringPart> Parts { get; init; }

    public override IEnumerable<Node> Children => [..Parts];
}

public abstract partial class StringPart : Node;

public sealed partial class TextStringPart : StringPart
{
    public required string Text { get; init; }

    public override IEnumerable<Node> Children => [];
}

public sealed partial class InterpolationStringPart : StringPart
{
    public required Expression Expression { get; init; }

    public override IEnumerable<Node> Children => [Expression];
}

public sealed partial class BoolExpression : Expression
{
    public required bool Value { get; init; }

    public override IEnumerable<Node> Children => [];
}

public sealed partial class NumberExpression : Expression
{
    public required double Value { get; init; }

    public override IEnumerable<Node> Children => [];
}

public sealed partial class NilExpression : Expression
{
    public override IEnumerable<Node> Children => [];
}
