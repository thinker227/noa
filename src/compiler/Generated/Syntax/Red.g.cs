// <auto-generated/>

#nullable enable
#pragma warning disable CS0108

using System.Diagnostics;

namespace Noa.Compiler.Syntax;

public sealed class RootSyntax : SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.RootSyntax green;

    public SyntaxList<StatementSyntax> Statements => (SyntaxList<StatementSyntax>)green.Statements.ToRed(position, this);
    
    public ExpressionSyntax TrailingExpression => (ExpressionSyntax)green.TrailingExpression.ToRed(position + ((Green.RootSyntax)green).Statements.GetWidth(), this);
    
    public Token EndOfFile => (Token)green.EndOfFile.ToRed(position + ((Green.RootSyntax)green).Statements.GetWidth() + ((Green.RootSyntax)green).TrailingExpression.GetWidth(), this);
    
    internal RootSyntax(Green.RootSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public abstract class StatementSyntax : SyntaxNode
{
    internal StatementSyntax(int position, SyntaxNode parent) : base(position, parent) {}
}

public abstract class DeclarationSyntax : StatementSyntax
{
    internal DeclarationSyntax(int position, SyntaxNode parent) : base(position, parent) {}
}

public sealed class FunctionDeclarationSyntax : DeclarationSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.FunctionDeclarationSyntax green;

    public Token Func => (Token)green.Func.ToRed(position, this);
    
    public Token Name => (Token)green.Name.ToRed(position + ((Green.FunctionDeclarationSyntax)green).Func.GetWidth(), this);
    
    public ParameterListSyntax Parameters => (ParameterListSyntax)green.Parameters.ToRed(position + ((Green.FunctionDeclarationSyntax)green).Func.GetWidth() + ((Green.FunctionDeclarationSyntax)green).Name.GetWidth(), this);
    
    public FunctionBodySyntax Body => (FunctionBodySyntax)green.Body.ToRed(position + ((Green.FunctionDeclarationSyntax)green).Func.GetWidth() + ((Green.FunctionDeclarationSyntax)green).Name.GetWidth() + ((Green.FunctionDeclarationSyntax)green).Parameters.GetWidth(), this);
    
    internal FunctionDeclarationSyntax(Green.FunctionDeclarationSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class ParameterListSyntax : SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ParameterListSyntax green;

    public Token OpenParen => (Token)green.OpenParen.ToRed(position, this);
    
    public SeparatedSyntaxList<ParameterSyntax> Parameters => (SeparatedSyntaxList<ParameterSyntax>)green.Parameters.ToRed(position + ((Green.ParameterListSyntax)green).OpenParen.GetWidth(), this);
    
    public Token CloseParen => (Token)green.CloseParen.ToRed(position + ((Green.ParameterListSyntax)green).OpenParen.GetWidth() + ((Green.ParameterListSyntax)green).Parameters.GetWidth(), this);
    
    internal ParameterListSyntax(Green.ParameterListSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class ParameterSyntax : SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ParameterSyntax green;

    public Token? Mut => (Token?)green.Mut?.ToRed(position, this);
    
    public Token Name => (Token)green.Name.ToRed(position + ((Green.ParameterSyntax)green).Mut?.GetWidth() ?? 0, this);
    
    internal ParameterSyntax(Green.ParameterSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public abstract class FunctionBodySyntax : SyntaxNode
{
    internal FunctionBodySyntax(int position, SyntaxNode parent) : base(position, parent) {}
}

public sealed class BlockBodySyntax : FunctionBodySyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.BlockBodySyntax green;

    public BlockExpressionSyntax Block => (BlockExpressionSyntax)green.Block.ToRed(position, this);
    
    internal BlockBodySyntax(Green.BlockBodySyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class ExpressionBodySyntax : FunctionBodySyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ExpressionBodySyntax green;

    public Token Arrow => (Token)green.Arrow.ToRed(position, this);
    
    public ExpressionSyntax Expression => (ExpressionSyntax)green.Expression.ToRed(position + ((Green.ExpressionBodySyntax)green).Arrow.GetWidth(), this);
    
    public Token Semicolon => (Token)green.Semicolon.ToRed(position + ((Green.ExpressionBodySyntax)green).Arrow.GetWidth() + ((Green.ExpressionBodySyntax)green).Expression.GetWidth(), this);
    
    internal ExpressionBodySyntax(Green.ExpressionBodySyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class LetDeclarationSyntax : DeclarationSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.LetDeclarationSyntax green;

    public Token Let => (Token)green.Let.ToRed(position, this);
    
    public Token? Mut => (Token?)green.Mut?.ToRed(position + ((Green.LetDeclarationSyntax)green).Let.GetWidth(), this);
    
    public Token Name => (Token)green.Name.ToRed(position + ((Green.LetDeclarationSyntax)green).Let.GetWidth() + ((Green.LetDeclarationSyntax)green).Mut?.GetWidth() ?? 0, this);
    
    public Token Equals => (Token)green.Equals.ToRed(position + ((Green.LetDeclarationSyntax)green).Let.GetWidth() + ((Green.LetDeclarationSyntax)green).Mut?.GetWidth() ?? 0 + ((Green.LetDeclarationSyntax)green).Name.GetWidth(), this);
    
    public ExpressionSyntax Value => (ExpressionSyntax)green.Value.ToRed(position + ((Green.LetDeclarationSyntax)green).Let.GetWidth() + ((Green.LetDeclarationSyntax)green).Mut?.GetWidth() ?? 0 + ((Green.LetDeclarationSyntax)green).Name.GetWidth() + ((Green.LetDeclarationSyntax)green).Equals.GetWidth(), this);
    
    public Token Semicolon => (Token)green.Semicolon.ToRed(position + ((Green.LetDeclarationSyntax)green).Let.GetWidth() + ((Green.LetDeclarationSyntax)green).Mut?.GetWidth() ?? 0 + ((Green.LetDeclarationSyntax)green).Name.GetWidth() + ((Green.LetDeclarationSyntax)green).Equals.GetWidth() + ((Green.LetDeclarationSyntax)green).Value.GetWidth(), this);
    
    internal LetDeclarationSyntax(Green.LetDeclarationSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class AssignmentStatementSyntax : StatementSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.AssignmentStatementSyntax green;

    public Token Identifier => (Token)green.Identifier.ToRed(position, this);
    
    public Token Operator => (Token)green.Operator.ToRed(position + ((Green.AssignmentStatementSyntax)green).Identifier.GetWidth(), this);
    
    public ExpressionSyntax Value => (ExpressionSyntax)green.Value.ToRed(position + ((Green.AssignmentStatementSyntax)green).Identifier.GetWidth() + ((Green.AssignmentStatementSyntax)green).Operator.GetWidth(), this);
    
    public Token Semicolon => (Token)green.Semicolon.ToRed(position + ((Green.AssignmentStatementSyntax)green).Identifier.GetWidth() + ((Green.AssignmentStatementSyntax)green).Operator.GetWidth() + ((Green.AssignmentStatementSyntax)green).Value.GetWidth(), this);
    
    internal AssignmentStatementSyntax(Green.AssignmentStatementSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class FlowControlStatement : SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.FlowControlStatement green;

    public ExpressionSyntax Expression => (ExpressionSyntax)green.Expression.ToRed(position, this);
    
    internal FlowControlStatement(Green.FlowControlStatement green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class ExpressionStatementSyntax : StatementSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ExpressionStatementSyntax green;

    public ExpressionSyntax Expression => (ExpressionSyntax)green.Expression.ToRed(position, this);
    
    public Token Semicolon => (Token)green.Semicolon.ToRed(position + ((Green.ExpressionStatementSyntax)green).Expression.GetWidth(), this);
    
    internal ExpressionStatementSyntax(Green.ExpressionStatementSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public abstract class ExpressionSyntax : SyntaxNode
{
    internal ExpressionSyntax(int position, SyntaxNode parent) : base(position, parent) {}
}

public sealed class BlockExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.BlockExpressionSyntax green;

    public Token OpenBrace => (Token)green.OpenBrace.ToRed(position, this);
    
    public SyntaxList<StatementSyntax> Statements => (SyntaxList<StatementSyntax>)green.Statements.ToRed(position + ((Green.BlockExpressionSyntax)green).OpenBrace.GetWidth(), this);
    
    public ExpressionSyntax TrailingExpression => (ExpressionSyntax)green.TrailingExpression.ToRed(position + ((Green.BlockExpressionSyntax)green).OpenBrace.GetWidth() + ((Green.BlockExpressionSyntax)green).Statements.GetWidth(), this);
    
    public Token CloseBrace => (Token)green.CloseBrace.ToRed(position + ((Green.BlockExpressionSyntax)green).OpenBrace.GetWidth() + ((Green.BlockExpressionSyntax)green).Statements.GetWidth() + ((Green.BlockExpressionSyntax)green).TrailingExpression.GetWidth(), this);
    
    internal BlockExpressionSyntax(Green.BlockExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class CallExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.CallExpressionSyntax green;

    public ExpressionSyntax Target => (ExpressionSyntax)green.Target.ToRed(position, this);
    
    public Token OpenParen => (Token)green.OpenParen.ToRed(position + ((Green.CallExpressionSyntax)green).Target.GetWidth(), this);
    
    public SeparatedSyntaxList<ExpressionSyntax> Arguments => (SeparatedSyntaxList<ExpressionSyntax>)green.Arguments.ToRed(position + ((Green.CallExpressionSyntax)green).Target.GetWidth() + ((Green.CallExpressionSyntax)green).OpenParen.GetWidth(), this);
    
    public Token CloseParen => (Token)green.CloseParen.ToRed(position + ((Green.CallExpressionSyntax)green).Target.GetWidth() + ((Green.CallExpressionSyntax)green).OpenParen.GetWidth() + ((Green.CallExpressionSyntax)green).Arguments.GetWidth(), this);
    
    internal CallExpressionSyntax(Green.CallExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class LambdaExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.LambdaExpressionSyntax green;

    public ParameterListSyntax Parameters => (ParameterListSyntax)green.Parameters.ToRed(position, this);
    
    public Token Arrow => (Token)green.Arrow.ToRed(position + ((Green.LambdaExpressionSyntax)green).Parameters.GetWidth(), this);
    
    public ExpressionSyntax Expression => (ExpressionSyntax)green.Expression.ToRed(position + ((Green.LambdaExpressionSyntax)green).Parameters.GetWidth() + ((Green.LambdaExpressionSyntax)green).Arrow.GetWidth(), this);
    
    internal LambdaExpressionSyntax(Green.LambdaExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class TupleExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.TupleExpressionSyntax green;

    public Token OpenParen => (Token)green.OpenParen.ToRed(position, this);
    
    public SeparatedSyntaxList<ExpressionSyntax> Expressions => (SeparatedSyntaxList<ExpressionSyntax>)green.Expressions.ToRed(position + ((Green.TupleExpressionSyntax)green).OpenParen.GetWidth(), this);
    
    public Token CloseParen => (Token)green.CloseParen.ToRed(position + ((Green.TupleExpressionSyntax)green).OpenParen.GetWidth() + ((Green.TupleExpressionSyntax)green).Expressions.GetWidth(), this);
    
    internal TupleExpressionSyntax(Green.TupleExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ParenthesizedExpressionSyntax green;

    public ExpressionSyntax Expression => (ExpressionSyntax)green.Expression.ToRed(position, this);
    
    internal ParenthesizedExpressionSyntax(Green.ParenthesizedExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class IfExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.IfExpressionSyntax green;

    public Token If => (Token)green.If.ToRed(position, this);
    
    public Token OpenParen => (Token)green.OpenParen.ToRed(position + ((Green.IfExpressionSyntax)green).If.GetWidth(), this);
    
    public ExpressionSyntax Condition => (ExpressionSyntax)green.Condition.ToRed(position + ((Green.IfExpressionSyntax)green).If.GetWidth() + ((Green.IfExpressionSyntax)green).OpenParen.GetWidth(), this);
    
    public Token CloseParen => (Token)green.CloseParen.ToRed(position + ((Green.IfExpressionSyntax)green).If.GetWidth() + ((Green.IfExpressionSyntax)green).OpenParen.GetWidth() + ((Green.IfExpressionSyntax)green).Condition.GetWidth(), this);
    
    public BlockExpressionSyntax Body => (BlockExpressionSyntax)green.Body.ToRed(position + ((Green.IfExpressionSyntax)green).If.GetWidth() + ((Green.IfExpressionSyntax)green).OpenParen.GetWidth() + ((Green.IfExpressionSyntax)green).Condition.GetWidth() + ((Green.IfExpressionSyntax)green).CloseParen.GetWidth(), this);
    
    public ElseClauseSyntax? Else => (ElseClauseSyntax?)green.Else?.ToRed(position + ((Green.IfExpressionSyntax)green).If.GetWidth() + ((Green.IfExpressionSyntax)green).OpenParen.GetWidth() + ((Green.IfExpressionSyntax)green).Condition.GetWidth() + ((Green.IfExpressionSyntax)green).CloseParen.GetWidth() + ((Green.IfExpressionSyntax)green).Body.GetWidth(), this);
    
    internal IfExpressionSyntax(Green.IfExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class ElseClauseSyntax : SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ElseClauseSyntax green;

    public Token Else => (Token)green.Else.ToRed(position, this);
    
    public BlockExpressionSyntax Body => (BlockExpressionSyntax)green.Body.ToRed(position + ((Green.ElseClauseSyntax)green).Else.GetWidth(), this);
    
    internal ElseClauseSyntax(Green.ElseClauseSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class LoopExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.LoopExpressionSyntax green;

    public Token Loop => (Token)green.Loop.ToRed(position, this);
    
    public BlockExpressionSyntax Body => (BlockExpressionSyntax)green.Body.ToRed(position + ((Green.LoopExpressionSyntax)green).Loop.GetWidth(), this);
    
    internal LoopExpressionSyntax(Green.LoopExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class ReturnExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ReturnExpressionSyntax green;

    public Token Return => (Token)green.Return.ToRed(position, this);
    
    public ExpressionSyntax? Value => (ExpressionSyntax?)green.Value?.ToRed(position + ((Green.ReturnExpressionSyntax)green).Return.GetWidth(), this);
    
    internal ReturnExpressionSyntax(Green.ReturnExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class BreakExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.BreakExpressionSyntax green;

    public Token Break => (Token)green.Break.ToRed(position, this);
    
    public ExpressionSyntax? Value => (ExpressionSyntax?)green.Value?.ToRed(position + ((Green.BreakExpressionSyntax)green).Break.GetWidth(), this);
    
    internal BreakExpressionSyntax(Green.BreakExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class ContinueExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ContinueExpressionSyntax green;

    public Token Continue => (Token)green.Continue.ToRed(position, this);
    
    internal ContinueExpressionSyntax(Green.ContinueExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.UnaryExpressionSyntax green;

    public Token Operator => (Token)green.Operator.ToRed(position, this);
    
    public ExpressionSyntax Operand => (ExpressionSyntax)green.Operand.ToRed(position + ((Green.UnaryExpressionSyntax)green).Operator.GetWidth(), this);
    
    internal UnaryExpressionSyntax(Green.UnaryExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.BinaryExpressionSyntax green;

    public ExpressionSyntax Left => (ExpressionSyntax)green.Left.ToRed(position, this);
    
    public Token Operator => (Token)green.Operator.ToRed(position + ((Green.BinaryExpressionSyntax)green).Left.GetWidth(), this);
    
    public ExpressionSyntax Right => (ExpressionSyntax)green.Right.ToRed(position + ((Green.BinaryExpressionSyntax)green).Left.GetWidth() + ((Green.BinaryExpressionSyntax)green).Operator.GetWidth(), this);
    
    internal BinaryExpressionSyntax(Green.BinaryExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class IdentifierExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.IdentifierExpressionSyntax green;

    public Token Identifier => (Token)green.Identifier.ToRed(position, this);
    
    internal IdentifierExpressionSyntax(Green.IdentifierExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class StringExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.StringExpressionSyntax green;

    public Token OpenQuote => (Token)green.OpenQuote.ToRed(position, this);
    
    public SyntaxList<StringPartSyntax> Parts => (SyntaxList<StringPartSyntax>)green.Parts.ToRed(position + ((Green.StringExpressionSyntax)green).OpenQuote.GetWidth(), this);
    
    public Token CloseQuote => (Token)green.CloseQuote.ToRed(position + ((Green.StringExpressionSyntax)green).OpenQuote.GetWidth() + ((Green.StringExpressionSyntax)green).Parts.GetWidth(), this);
    
    internal StringExpressionSyntax(Green.StringExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public abstract class StringPartSyntax : SyntaxNode
{
    internal StringPartSyntax(int position, SyntaxNode parent) : base(position, parent) {}
}

public sealed class TextStringPartSyntax : StringPartSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.TextStringPartSyntax green;

    public Token Text => (Token)green.Text.ToRed(position, this);
    
    internal TextStringPartSyntax(Green.TextStringPartSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class InterpolationStringPartSyntax : StringPartSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.InterpolationStringPartSyntax green;

    public Token OpenDelimiter => (Token)green.OpenDelimiter.ToRed(position, this);
    
    public ExpressionSyntax Expression => (ExpressionSyntax)green.Expression.ToRed(position + ((Green.InterpolationStringPartSyntax)green).OpenDelimiter.GetWidth(), this);
    
    public Token CloseDelimiter => (Token)green.CloseDelimiter.ToRed(position + ((Green.InterpolationStringPartSyntax)green).OpenDelimiter.GetWidth() + ((Green.InterpolationStringPartSyntax)green).Expression.GetWidth(), this);
    
    internal InterpolationStringPartSyntax(Green.InterpolationStringPartSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class BoolExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.BoolExpressionSyntax green;

    public Token Value => (Token)green.Value.ToRed(position, this);
    
    internal BoolExpressionSyntax(Green.BoolExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class NumberExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.NumberExpressionSyntax green;

    public Token Value => (Token)green.Value.ToRed(position, this);
    
    internal NumberExpressionSyntax(Green.NumberExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}

public sealed class NilExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.NilExpressionSyntax green;

    public Token OpenParen => (Token)green.OpenParen.ToRed(position, this);
    
    public Token CloseParen => (Token)green.CloseParen.ToRed(position + ((Green.NilExpressionSyntax)green).OpenParen.GetWidth(), this);
    
    internal NilExpressionSyntax(Green.NilExpressionSyntax green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;
    
    protected override int GetWidth() => green.GetWidth();
}
