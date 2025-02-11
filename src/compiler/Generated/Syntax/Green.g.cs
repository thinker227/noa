// <auto-generated/>

#nullable enable
#pragma warning disable CS0108

namespace Noa.Compiler.Syntax.Green;

internal sealed class RootSyntax : SyntaxNode
{
    private int? width;

    public required SyntaxList<StatementSyntax> Statements { get; init; }

    public required ExpressionSyntax? TrailingExpression { get; init; }

    public required Token EndOfFile { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Statements;
            if (TrailingExpression is not null) yield return TrailingExpression;
            yield return EndOfFile;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Statements.GetWidth() + (TrailingExpression?.GetWidth() ?? 0) + EndOfFile.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.RootSyntax(this, position, parent);
}

internal abstract class StatementSyntax : SyntaxNode
{
}

internal abstract class DeclarationSyntax : StatementSyntax
{
}

internal sealed class FunctionDeclarationSyntax : DeclarationSyntax
{
    private int? width;

    public required Token Func { get; init; }

    public required Token Name { get; init; }

    public required ParameterListSyntax Parameters { get; init; }

    public required FunctionBodySyntax Body { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Func;
            yield return Name;
            yield return Parameters;
            yield return Body;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Func.GetWidth() + Name.GetWidth() + Parameters.GetWidth() + Body.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.FunctionDeclarationSyntax(this, position, parent);
}

internal sealed class ParameterListSyntax : SyntaxNode
{
    private int? width;

    public required Token OpenParen { get; init; }

    public required SeparatedSyntaxList<ParameterSyntax> Parameters { get; init; }

    public required Token CloseParen { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return OpenParen;
            yield return Parameters;
            yield return CloseParen;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + OpenParen.GetWidth() + Parameters.GetWidth() + CloseParen.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.ParameterListSyntax(this, position, parent);
}

internal sealed class ParameterSyntax : SyntaxNode
{
    private int? width;

    public required Token? Mut { get; init; }

    public required Token Name { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            if (Mut is not null) yield return Mut;
            yield return Name;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + (Mut?.GetWidth() ?? 0) + Name.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.ParameterSyntax(this, position, parent);
}

internal abstract class FunctionBodySyntax : SyntaxNode
{
}

internal sealed class BlockBodySyntax : FunctionBodySyntax
{
    private int? width;

    public required BlockExpressionSyntax Block { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Block;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Block.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.BlockBodySyntax(this, position, parent);
}

internal sealed class ExpressionBodySyntax : FunctionBodySyntax
{
    private int? width;

    public required Token Arrow { get; init; }

    public required ExpressionSyntax Expression { get; init; }

    public required Token Semicolon { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Arrow;
            yield return Expression;
            yield return Semicolon;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Arrow.GetWidth() + Expression.GetWidth() + Semicolon.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.ExpressionBodySyntax(this, position, parent);
}

internal sealed class LetDeclarationSyntax : DeclarationSyntax
{
    private int? width;

    public required Token Let { get; init; }

    public required Token? Mut { get; init; }

    public required Token Name { get; init; }

    public required Token Equals { get; init; }

    public required ExpressionSyntax Value { get; init; }

    public required Token Semicolon { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Let;
            if (Mut is not null) yield return Mut;
            yield return Name;
            yield return Equals;
            yield return Value;
            yield return Semicolon;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Let.GetWidth() + (Mut?.GetWidth() ?? 0) + Name.GetWidth() + Equals.GetWidth() + Value.GetWidth() + Semicolon.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.LetDeclarationSyntax(this, position, parent);
}

internal sealed class AssignmentStatementSyntax : StatementSyntax
{
    private int? width;

    public required ExpressionSyntax Target { get; init; }

    public required Token Operator { get; init; }

    public required ExpressionSyntax Value { get; init; }

    public required Token Semicolon { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Target;
            yield return Operator;
            yield return Value;
            yield return Semicolon;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Target.GetWidth() + Operator.GetWidth() + Value.GetWidth() + Semicolon.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.AssignmentStatementSyntax(this, position, parent);
}

internal sealed class FlowControlStatementSyntax : StatementSyntax
{
    private int? width;

    public required ExpressionSyntax Expression { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Expression;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Expression.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.FlowControlStatementSyntax(this, position, parent);
}

internal sealed class ExpressionStatementSyntax : StatementSyntax
{
    private int? width;

    public required ExpressionSyntax Expression { get; init; }

    public required Token Semicolon { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Expression;
            yield return Semicolon;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Expression.GetWidth() + Semicolon.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.ExpressionStatementSyntax(this, position, parent);
}

internal abstract class ExpressionSyntax : SyntaxNode
{
}

internal sealed class ErrorExpressionSyntax : ExpressionSyntax
{
    private int? width;

    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0;

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.ErrorExpressionSyntax(this, position, parent);
}

internal sealed class BlockExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token OpenBrace { get; init; }

    public required SyntaxList<StatementSyntax> Statements { get; init; }

    public required ExpressionSyntax? TrailingExpression { get; init; }

    public required Token CloseBrace { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return OpenBrace;
            yield return Statements;
            if (TrailingExpression is not null) yield return TrailingExpression;
            yield return CloseBrace;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + OpenBrace.GetWidth() + Statements.GetWidth() + (TrailingExpression?.GetWidth() ?? 0) + CloseBrace.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.BlockExpressionSyntax(this, position, parent);
}

internal sealed class CallExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required ExpressionSyntax Target { get; init; }

    public required Token OpenParen { get; init; }

    public required SeparatedSyntaxList<ExpressionSyntax> Arguments { get; init; }

    public required Token CloseParen { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Target;
            yield return OpenParen;
            yield return Arguments;
            yield return CloseParen;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Target.GetWidth() + OpenParen.GetWidth() + Arguments.GetWidth() + CloseParen.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.CallExpressionSyntax(this, position, parent);
}

internal sealed class LambdaExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required ParameterListSyntax Parameters { get; init; }

    public required Token Arrow { get; init; }

    public required ExpressionSyntax Expression { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Parameters;
            yield return Arrow;
            yield return Expression;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Parameters.GetWidth() + Arrow.GetWidth() + Expression.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.LambdaExpressionSyntax(this, position, parent);
}

internal sealed class TupleExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token OpenParen { get; init; }

    public required SeparatedSyntaxList<ExpressionSyntax> Expressions { get; init; }

    public required Token CloseParen { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return OpenParen;
            yield return Expressions;
            yield return CloseParen;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + OpenParen.GetWidth() + Expressions.GetWidth() + CloseParen.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.TupleExpressionSyntax(this, position, parent);
}

internal sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required ExpressionSyntax Expression { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Expression;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Expression.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.ParenthesizedExpressionSyntax(this, position, parent);
}

internal sealed class IfExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token If { get; init; }

    public required ExpressionSyntax Condition { get; init; }

    public required BlockExpressionSyntax Body { get; init; }

    public required ElseClauseSyntax? Else { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return If;
            yield return Condition;
            yield return Body;
            if (Else is not null) yield return Else;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + If.GetWidth() + Condition.GetWidth() + Body.GetWidth() + (Else?.GetWidth() ?? 0);

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.IfExpressionSyntax(this, position, parent);
}

internal sealed class ElseClauseSyntax : SyntaxNode
{
    private int? width;

    public required Token Else { get; init; }

    public required BlockExpressionSyntax Body { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Else;
            yield return Body;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Else.GetWidth() + Body.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.ElseClauseSyntax(this, position, parent);
}

internal sealed class LoopExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token Loop { get; init; }

    public required BlockExpressionSyntax Body { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Loop;
            yield return Body;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Loop.GetWidth() + Body.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.LoopExpressionSyntax(this, position, parent);
}

internal sealed class ReturnExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token Return { get; init; }

    public required ExpressionSyntax? Value { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Return;
            if (Value is not null) yield return Value;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Return.GetWidth() + (Value?.GetWidth() ?? 0);

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.ReturnExpressionSyntax(this, position, parent);
}

internal sealed class BreakExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token Break { get; init; }

    public required ExpressionSyntax? Value { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Break;
            if (Value is not null) yield return Value;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Break.GetWidth() + (Value?.GetWidth() ?? 0);

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.BreakExpressionSyntax(this, position, parent);
}

internal sealed class ContinueExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token Continue { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Continue;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Continue.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.ContinueExpressionSyntax(this, position, parent);
}

internal sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token Operator { get; init; }

    public required ExpressionSyntax Operand { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Operator;
            yield return Operand;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Operator.GetWidth() + Operand.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.UnaryExpressionSyntax(this, position, parent);
}

internal sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required ExpressionSyntax Left { get; init; }

    public required Token Operator { get; init; }

    public required ExpressionSyntax Right { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Left;
            yield return Operator;
            yield return Right;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Left.GetWidth() + Operator.GetWidth() + Right.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.BinaryExpressionSyntax(this, position, parent);
}

internal sealed class IdentifierExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token Identifier { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Identifier;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Identifier.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.IdentifierExpressionSyntax(this, position, parent);
}

internal sealed class StringExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token OpenQuote { get; init; }

    public required SyntaxList<StringPartSyntax> Parts { get; init; }

    public required Token CloseQuote { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return OpenQuote;
            yield return Parts;
            yield return CloseQuote;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + OpenQuote.GetWidth() + Parts.GetWidth() + CloseQuote.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.StringExpressionSyntax(this, position, parent);
}

internal abstract class StringPartSyntax : SyntaxNode
{
}

internal sealed class TextStringPartSyntax : StringPartSyntax
{
    private int? width;

    public required Token Text { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Text;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Text.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.TextStringPartSyntax(this, position, parent);
}

internal sealed class InterpolationStringPartSyntax : StringPartSyntax
{
    private int? width;

    public required Token OpenDelimiter { get; init; }

    public required ExpressionSyntax Expression { get; init; }

    public required Token CloseDelimiter { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return OpenDelimiter;
            yield return Expression;
            yield return CloseDelimiter;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + OpenDelimiter.GetWidth() + Expression.GetWidth() + CloseDelimiter.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.InterpolationStringPartSyntax(this, position, parent);
}

internal sealed class BoolExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token Value { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Value;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Value.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.BoolExpressionSyntax(this, position, parent);
}

internal sealed class NumberExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token Value { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Value;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + Value.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.NumberExpressionSyntax(this, position, parent);
}

internal sealed class NilExpressionSyntax : ExpressionSyntax
{
    private int? width;

    public required Token OpenParen { get; init; }

    public required Token CloseParen { get; init; }
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return OpenParen;
            yield return CloseParen;
            yield break;
        }
    }

    public override int GetWidth() => width ??= ComputeWidth();

    private int ComputeWidth() => 0 + OpenParen.GetWidth() + CloseParen.GetWidth();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        new Syntax.NilExpressionSyntax(this, position, parent);
}
