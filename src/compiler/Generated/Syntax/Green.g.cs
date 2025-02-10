// <auto-generated/>

#nullable enable
#pragma warning disable CS0108

namespace Noa.Compiler.Syntax.Green;

internal sealed partial class RootSyntax : SyntaxNode
{
    public required ImmutableArray<StatementSyntax> Statements { get; init; }

    public required ExpressionSyntax TrailingExpression { get; init; }

    public override int Width() => Statements.Width() + TrailingExpression.Width();
}

internal abstract partial class StatementSyntax : SyntaxNode;

internal abstract partial class DeclarationSyntax : StatementSyntax;

internal sealed partial class FunctionDeclarationSyntax : DeclarationSyntax
{
    public required Token Func { get; init; }

    public required Token Name { get; init; }

    public required ParameterListSyntax Parameters { get; init; }

    public required FunctionBodySyntax Body { get; init; }

    public override int Width() => Func.Width() + Name.Width() + Parameters.Width() + Body.Width();
}

internal sealed partial class ParameterListSyntax : SyntaxNode
{
    public required Token OpenParen { get; init; }

    public required SeparatedSyntaxList<ParameterSyntax> Parameters { get; init; }

    public required Token CloseParen { get; init; }

    public override int Width() => OpenParen.Width() + Parameters.Width() + CloseParen.Width();
}

internal sealed partial class ParameterSyntax : SyntaxNode
{
    public required Token? Mut { get; init; }

    public required Token Name { get; init; }

    public override int Width() => Mut?.Width() ?? 0 + Name.Width();
}

internal abstract partial class FunctionBodySyntax : SyntaxNode;

internal sealed partial class BlockBodySyntax : FunctionBodySyntax
{
    public required BlockExpressionSyntax Block { get; init; }

    public override int Width() => Block.Width();
}

internal sealed partial class ExpressionBodySyntax : FunctionBodySyntax
{
    public required Token Arrow { get; init; }

    public required ExpressionSyntax Expression { get; init; }

    public required Token Semicolon { get; init; }

    public override int Width() => Arrow.Width() + Expression.Width() + Semicolon.Width();
}

internal sealed partial class LetDeclarationSyntax : SyntaxNode
{
    public required Token Let { get; init; }

    public required Token? Mut { get; init; }

    public required Token Name { get; init; }

    public required Token Equals { get; init; }

    public required ExpressionSyntax Value { get; init; }

    public required Token Semicolon { get; init; }

    public override int Width() => Let.Width() + Mut?.Width() ?? 0 + Name.Width() + Equals.Width() + Value.Width() + Semicolon.Width();
}

internal sealed partial class AssignmentStatementSyntax : StatementSyntax
{
    public required Token Identifier { get; init; }

    public required Token Operator { get; init; }

    public required ExpressionSyntax Value { get; init; }

    public required Token Semicolon { get; init; }

    public override int Width() => Identifier.Width() + Operator.Width() + Value.Width() + Semicolon.Width();
}

internal sealed partial class FlowControlStatement : SyntaxNode
{
    public required ExpressionSyntax Expression { get; init; }

    public override int Width() => Expression.Width();
}

internal sealed partial class ExpressionStatementSyntax : StatementSyntax
{
    public required ExpressionSyntax Expression { get; init; }

    public required Token Semicolon { get; init; }

    public override int Width() => Expression.Width() + Semicolon.Width();
}

internal abstract partial class ExpressionSyntax : SyntaxNode;

internal sealed partial class BlockExpressionSyntax : ExpressionSyntax
{
    public required Token OpenBrace { get; init; }

    public required ImmutableArray<StatementSyntax> Statements { get; init; }

    public required ExpressionSyntax TrailingExpression { get; init; }

    public required Token CloseBrace { get; init; }

    public override int Width() => OpenBrace.Width() + Statements.Width() + TrailingExpression.Width() + CloseBrace.Width();
}

internal sealed partial class CallExpressionSyntax : ExpressionSyntax
{
    public required ExpressionSyntax Target { get; init; }

    public required Token OpenParen { get; init; }

    public required SeparatedSyntaxList<ExpressionSyntax> Arguments { get; init; }

    public required Token CloseParen { get; init; }

    public override int Width() => Target.Width() + OpenParen.Width() + Arguments.Width() + CloseParen.Width();
}

internal sealed partial class LambdaExpressionSyntax : ExpressionSyntax
{
    public required ParameterListSyntax Parameters { get; init; }

    public required Token Arrow { get; init; }

    public required ExpressionSyntax Expression { get; init; }

    public override int Width() => Parameters.Width() + Arrow.Width() + Expression.Width();
}

internal sealed partial class TupleExpressionSyntax : ExpressionSyntax
{
    public required Token OpenParen { get; init; }

    public required SeparatedSyntaxList<ExpressionSyntax> Expressions { get; init; }

    public required Token CloseParen { get; init; }

    public override int Width() => OpenParen.Width() + Expressions.Width() + CloseParen.Width();
}

internal sealed partial class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    public required ExpressionSyntax Expression { get; init; }

    public override int Width() => Expression.Width();
}

internal sealed partial class IfExpressionSyntax : ExpressionSyntax
{
    public required Token If { get; init; }

    public required Token OpenParen { get; init; }

    public required ExpressionSyntax Condition { get; init; }

    public required Token CloseParen { get; init; }

    public required BlockExpressionSyntax Body { get; init; }

    public required ElseClauseSyntax Else { get; init; }

    public override int Width() => If.Width() + OpenParen.Width() + Condition.Width() + CloseParen.Width() + Body.Width() + Else.Width();
}

internal sealed partial class ElseClauseSyntax : SyntaxNode
{
    public required Token Else { get; init; }

    public required BlockExpressionSyntax Body { get; init; }

    public override int Width() => Else.Width() + Body.Width();
}

internal sealed partial class LoopExpression : ExpressionSyntax
{
    public required Token Loop { get; init; }

    public required BlockExpressionSyntax Body { get; init; }

    public override int Width() => Loop.Width() + Body.Width();
}

internal sealed partial class ReturnExpressionSyntax : ExpressionSyntax
{
    public required Token Return { get; init; }

    public required ExpressionSyntax? Value { get; init; }

    public override int Width() => Return.Width() + Value?.Width() ?? 0;
}

internal sealed partial class BreakExpressionSyntax : ExpressionSyntax
{
    public required Token Break { get; init; }

    public required ExpressionSyntax? Value { get; init; }

    public override int Width() => Break.Width() + Value?.Width() ?? 0;
}

internal sealed partial class ContinueExpressionSyntax : ExpressionSyntax
{
    public required Token Continue { get; init; }

    public override int Width() => Continue.Width();
}

internal sealed partial class UnaryExpressionSyntax : ExpressionSyntax
{
    public required Token Operator { get; init; }

    public required ExpressionSyntax Operand { get; init; }

    public override int Width() => Operator.Width() + Operand.Width();
}

internal sealed partial class BinaryExpressionSyntax : ExpressionSyntax
{
    public required ExpressionSyntax Left { get; init; }

    public required Token Operator { get; init; }

    public required ExpressionSyntax Right { get; init; }

    public override int Width() => Left.Width() + Operator.Width() + Right.Width();
}

internal sealed partial class IdentifierExpressionSyntax : ExpressionSyntax
{
    public required Token Identifier { get; init; }

    public override int Width() => Identifier.Width();
}

internal sealed partial class StringExpressionSyntax : ExpressionSyntax
{
    public required Token OpenQuote { get; init; }

    public required ImmutableArray<StringPart> Parts { get; init; }

    public required Token CloseQuote { get; init; }

    public override int Width() => OpenQuote.Width() + Parts.Width() + CloseQuote.Width();
}

internal abstract partial class StringPart : SyntaxNode;

internal sealed partial class TextStringPart : StringPart
{
    public required Token Text { get; init; }

    public override int Width() => Text.Width();
}

internal sealed partial class InterpolationStringPart : StringPart
{
    public required Token OpenDelimiter { get; init; }

    public required ExpressionSyntax Expression { get; init; }

    public required Token CloseDelimiter { get; init; }

    public override int Width() => OpenDelimiter.Width() + Expression.Width() + CloseDelimiter.Width();
}

internal sealed partial class BoolExpressionSyntax : ExpressionSyntax
{
    public required Token Value { get; init; }

    public override int Width() => Value.Width();
}

internal sealed partial class NumberExpressionSyntax : ExpressionSyntax
{
    public required Token Value { get; init; }

    public override int Width() => Value.Width();
}

internal sealed partial class NilExpressionSyntax : ExpressionSyntax
{
    public required Token OpenParen { get; init; }

    public required Token CloseParen { get; init; }

    public override int Width() => OpenParen.Width() + CloseParen.Width();
}
