// <auto-generated/>

#nullable enable
#pragma warning disable CS0108

using System.Diagnostics;

namespace Noa.Compiler.Syntax;

public sealed class BlockSyntax : SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.BlockSyntax green;

    internal override Green.SyntaxNode Green => green;

    public SyntaxList<StatementSyntax> Statements => (SyntaxList<StatementSyntax>)green.Statements.ToRed(FullPosition, this);
    
    public ExpressionSyntax? TrailingExpression => (ExpressionSyntax?)green.TrailingExpression?.ToRed(FullPosition + green.Statements.GetFullWidth(), this);
    
    internal BlockSyntax(Green.BlockSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Statements;
            if (TrailingExpression is not null) yield return TrailingExpression;
            yield break;
        }
    }
}

public sealed class RootSyntax : SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.RootSyntax green;

    internal override Green.SyntaxNode Green => green;

    public BlockSyntax Block => (BlockSyntax)green.Block.ToRed(FullPosition, this);
    
    public Token EndOfFile => (Token)green.EndOfFile.ToRed(FullPosition + green.Block.GetFullWidth(), this);
    
    internal RootSyntax(Green.RootSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Block;
            yield return EndOfFile;
            yield break;
        }
    }
}

public abstract class StatementSyntax : SyntaxNode
{
    internal StatementSyntax(int fullPosition, SyntaxNode parent) : base(fullPosition, parent) {}
}

public abstract class DeclarationSyntax : StatementSyntax
{
    internal DeclarationSyntax(int fullPosition, SyntaxNode parent) : base(fullPosition, parent) {}
}

public sealed class FunctionDeclarationSyntax : DeclarationSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.FunctionDeclarationSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Func => (Token)green.Func.ToRed(FullPosition, this);
    
    public Token Name => (Token)green.Name.ToRed(FullPosition + green.Func.GetFullWidth(), this);
    
    public ParameterListSyntax Parameters => (ParameterListSyntax)green.Parameters.ToRed(FullPosition + green.Func.GetFullWidth() + green.Name.GetFullWidth(), this);
    
    public FunctionBodySyntax Body => (FunctionBodySyntax)green.Body.ToRed(FullPosition + green.Func.GetFullWidth() + green.Name.GetFullWidth() + green.Parameters.GetFullWidth(), this);
    
    internal FunctionDeclarationSyntax(Green.FunctionDeclarationSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
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
}

public sealed class ParameterListSyntax : SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ParameterListSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token OpenParen => (Token)green.OpenParen.ToRed(FullPosition, this);
    
    public SeparatedSyntaxList<ParameterSyntax> Parameters => (SeparatedSyntaxList<ParameterSyntax>)green.Parameters.ToRed(FullPosition + green.OpenParen.GetFullWidth(), this);
    
    public Token CloseParen => (Token)green.CloseParen.ToRed(FullPosition + green.OpenParen.GetFullWidth() + green.Parameters.GetFullWidth(), this);
    
    internal ParameterListSyntax(Green.ParameterListSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
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
}

public sealed class ParameterSyntax : SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ParameterSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token? Mut => (Token?)green.Mut?.ToRed(FullPosition, this);
    
    public Token Name => (Token)green.Name.ToRed(FullPosition + (green.Mut?.GetFullWidth() ?? 0), this);
    
    internal ParameterSyntax(Green.ParameterSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            if (Mut is not null) yield return Mut;
            yield return Name;
            yield break;
        }
    }
}

public abstract class FunctionBodySyntax : SyntaxNode
{
    internal FunctionBodySyntax(int fullPosition, SyntaxNode parent) : base(fullPosition, parent) {}
}

public sealed class BlockBodySyntax : FunctionBodySyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.BlockBodySyntax green;

    internal override Green.SyntaxNode Green => green;

    public BlockExpressionSyntax Block => (BlockExpressionSyntax)green.Block.ToRed(FullPosition, this);
    
    internal BlockBodySyntax(Green.BlockBodySyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Block;
            yield break;
        }
    }
}

public sealed class ExpressionBodySyntax : FunctionBodySyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ExpressionBodySyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Arrow => (Token)green.Arrow.ToRed(FullPosition, this);
    
    public ExpressionSyntax Expression => (ExpressionSyntax)green.Expression.ToRed(FullPosition + green.Arrow.GetFullWidth(), this);
    
    public Token Semicolon => (Token)green.Semicolon.ToRed(FullPosition + green.Arrow.GetFullWidth() + green.Expression.GetFullWidth(), this);
    
    internal ExpressionBodySyntax(Green.ExpressionBodySyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
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
}

public sealed class LetDeclarationSyntax : DeclarationSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.LetDeclarationSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Let => (Token)green.Let.ToRed(FullPosition, this);
    
    public Token? Mut => (Token?)green.Mut?.ToRed(FullPosition + green.Let.GetFullWidth(), this);
    
    public Token Name => (Token)green.Name.ToRed(FullPosition + green.Let.GetFullWidth() + (green.Mut?.GetFullWidth() ?? 0), this);
    
    public Token Equals => (Token)green.Equals.ToRed(FullPosition + green.Let.GetFullWidth() + (green.Mut?.GetFullWidth() ?? 0) + green.Name.GetFullWidth(), this);
    
    public ExpressionSyntax Value => (ExpressionSyntax)green.Value.ToRed(FullPosition + green.Let.GetFullWidth() + (green.Mut?.GetFullWidth() ?? 0) + green.Name.GetFullWidth() + green.Equals.GetFullWidth(), this);
    
    public Token Semicolon => (Token)green.Semicolon.ToRed(FullPosition + green.Let.GetFullWidth() + (green.Mut?.GetFullWidth() ?? 0) + green.Name.GetFullWidth() + green.Equals.GetFullWidth() + green.Value.GetFullWidth(), this);
    
    internal LetDeclarationSyntax(Green.LetDeclarationSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
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
}

public sealed class AssignmentStatementSyntax : StatementSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.AssignmentStatementSyntax green;

    internal override Green.SyntaxNode Green => green;

    public ExpressionSyntax Target => (ExpressionSyntax)green.Target.ToRed(FullPosition, this);
    
    public Token Operator => (Token)green.Operator.ToRed(FullPosition + green.Target.GetFullWidth(), this);
    
    public ExpressionSyntax Value => (ExpressionSyntax)green.Value.ToRed(FullPosition + green.Target.GetFullWidth() + green.Operator.GetFullWidth(), this);
    
    public Token Semicolon => (Token)green.Semicolon.ToRed(FullPosition + green.Target.GetFullWidth() + green.Operator.GetFullWidth() + green.Value.GetFullWidth(), this);
    
    internal AssignmentStatementSyntax(Green.AssignmentStatementSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
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
}

public sealed class FlowControlStatementSyntax : StatementSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.FlowControlStatementSyntax green;

    internal override Green.SyntaxNode Green => green;

    public ExpressionSyntax Expression => (ExpressionSyntax)green.Expression.ToRed(FullPosition, this);
    
    internal FlowControlStatementSyntax(Green.FlowControlStatementSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Expression;
            yield break;
        }
    }
}

public sealed class ExpressionStatementSyntax : StatementSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ExpressionStatementSyntax green;

    internal override Green.SyntaxNode Green => green;

    public ExpressionSyntax Expression => (ExpressionSyntax)green.Expression.ToRed(FullPosition, this);
    
    public Token Semicolon => (Token)green.Semicolon.ToRed(FullPosition + green.Expression.GetFullWidth(), this);
    
    internal ExpressionStatementSyntax(Green.ExpressionStatementSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Expression;
            yield return Semicolon;
            yield break;
        }
    }
}

public abstract class ExpressionSyntax : SyntaxNode
{
    internal ExpressionSyntax(int fullPosition, SyntaxNode parent) : base(fullPosition, parent) {}
}

public sealed class ErrorExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ErrorExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    internal ErrorExpressionSyntax(Green.ErrorExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield break;
        }
    }
}

public sealed class BlockExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.BlockExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token OpenBrace => (Token)green.OpenBrace.ToRed(FullPosition, this);
    
    public BlockSyntax Block => (BlockSyntax)green.Block.ToRed(FullPosition + green.OpenBrace.GetFullWidth(), this);
    
    public Token CloseBrace => (Token)green.CloseBrace.ToRed(FullPosition + green.OpenBrace.GetFullWidth() + green.Block.GetFullWidth(), this);
    
    internal BlockExpressionSyntax(Green.BlockExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return OpenBrace;
            yield return Block;
            yield return CloseBrace;
            yield break;
        }
    }
}

public sealed class CallExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.CallExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public ExpressionSyntax Target => (ExpressionSyntax)green.Target.ToRed(FullPosition, this);
    
    public Token OpenParen => (Token)green.OpenParen.ToRed(FullPosition + green.Target.GetFullWidth(), this);
    
    public SeparatedSyntaxList<ExpressionSyntax> Arguments => (SeparatedSyntaxList<ExpressionSyntax>)green.Arguments.ToRed(FullPosition + green.Target.GetFullWidth() + green.OpenParen.GetFullWidth(), this);
    
    public Token CloseParen => (Token)green.CloseParen.ToRed(FullPosition + green.Target.GetFullWidth() + green.OpenParen.GetFullWidth() + green.Arguments.GetFullWidth(), this);
    
    internal CallExpressionSyntax(Green.CallExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
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
}

public sealed class LambdaExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.LambdaExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public ParameterListSyntax Parameters => (ParameterListSyntax)green.Parameters.ToRed(FullPosition, this);
    
    public Token Arrow => (Token)green.Arrow.ToRed(FullPosition + green.Parameters.GetFullWidth(), this);
    
    public ExpressionSyntax Expression => (ExpressionSyntax)green.Expression.ToRed(FullPosition + green.Parameters.GetFullWidth() + green.Arrow.GetFullWidth(), this);
    
    internal LambdaExpressionSyntax(Green.LambdaExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
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
}

public sealed class TupleExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.TupleExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token OpenParen => (Token)green.OpenParen.ToRed(FullPosition, this);
    
    public SeparatedSyntaxList<ExpressionSyntax> Expressions => (SeparatedSyntaxList<ExpressionSyntax>)green.Expressions.ToRed(FullPosition + green.OpenParen.GetFullWidth(), this);
    
    public Token CloseParen => (Token)green.CloseParen.ToRed(FullPosition + green.OpenParen.GetFullWidth() + green.Expressions.GetFullWidth(), this);
    
    internal TupleExpressionSyntax(Green.TupleExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
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
}

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ParenthesizedExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token OpenParen => (Token)green.OpenParen.ToRed(FullPosition, this);
    
    public ExpressionSyntax Expression => (ExpressionSyntax)green.Expression.ToRed(FullPosition + green.OpenParen.GetFullWidth(), this);
    
    public Token CloseParen => (Token)green.CloseParen.ToRed(FullPosition + green.OpenParen.GetFullWidth() + green.Expression.GetFullWidth(), this);
    
    internal ParenthesizedExpressionSyntax(Green.ParenthesizedExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return OpenParen;
            yield return Expression;
            yield return CloseParen;
            yield break;
        }
    }
}

public sealed class IfExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.IfExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token If => (Token)green.If.ToRed(FullPosition, this);
    
    public ExpressionSyntax Condition => (ExpressionSyntax)green.Condition.ToRed(FullPosition + green.If.GetFullWidth(), this);
    
    public BlockExpressionSyntax Body => (BlockExpressionSyntax)green.Body.ToRed(FullPosition + green.If.GetFullWidth() + green.Condition.GetFullWidth(), this);
    
    public ElseClauseSyntax? Else => (ElseClauseSyntax?)green.Else?.ToRed(FullPosition + green.If.GetFullWidth() + green.Condition.GetFullWidth() + green.Body.GetFullWidth(), this);
    
    internal IfExpressionSyntax(Green.IfExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
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
}

public sealed class ElseClauseSyntax : SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ElseClauseSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Else => (Token)green.Else.ToRed(FullPosition, this);
    
    public BlockExpressionSyntax Body => (BlockExpressionSyntax)green.Body.ToRed(FullPosition + green.Else.GetFullWidth(), this);
    
    internal ElseClauseSyntax(Green.ElseClauseSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Else;
            yield return Body;
            yield break;
        }
    }
}

public sealed class LoopExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.LoopExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Loop => (Token)green.Loop.ToRed(FullPosition, this);
    
    public BlockExpressionSyntax Body => (BlockExpressionSyntax)green.Body.ToRed(FullPosition + green.Loop.GetFullWidth(), this);
    
    internal LoopExpressionSyntax(Green.LoopExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Loop;
            yield return Body;
            yield break;
        }
    }
}

public sealed class ReturnExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ReturnExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Return => (Token)green.Return.ToRed(FullPosition, this);
    
    public ExpressionSyntax? Value => (ExpressionSyntax?)green.Value?.ToRed(FullPosition + green.Return.GetFullWidth(), this);
    
    internal ReturnExpressionSyntax(Green.ReturnExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Return;
            if (Value is not null) yield return Value;
            yield break;
        }
    }
}

public sealed class BreakExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.BreakExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Break => (Token)green.Break.ToRed(FullPosition, this);
    
    public ExpressionSyntax? Value => (ExpressionSyntax?)green.Value?.ToRed(FullPosition + green.Break.GetFullWidth(), this);
    
    internal BreakExpressionSyntax(Green.BreakExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Break;
            if (Value is not null) yield return Value;
            yield break;
        }
    }
}

public sealed class ContinueExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.ContinueExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Continue => (Token)green.Continue.ToRed(FullPosition, this);
    
    internal ContinueExpressionSyntax(Green.ContinueExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Continue;
            yield break;
        }
    }
}

public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.UnaryExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Operator => (Token)green.Operator.ToRed(FullPosition, this);
    
    public ExpressionSyntax Operand => (ExpressionSyntax)green.Operand.ToRed(FullPosition + green.Operator.GetFullWidth(), this);
    
    internal UnaryExpressionSyntax(Green.UnaryExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Operator;
            yield return Operand;
            yield break;
        }
    }
}

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.BinaryExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public ExpressionSyntax Left => (ExpressionSyntax)green.Left.ToRed(FullPosition, this);
    
    public Token Operator => (Token)green.Operator.ToRed(FullPosition + green.Left.GetFullWidth(), this);
    
    public ExpressionSyntax Right => (ExpressionSyntax)green.Right.ToRed(FullPosition + green.Left.GetFullWidth() + green.Operator.GetFullWidth(), this);
    
    internal BinaryExpressionSyntax(Green.BinaryExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
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
}

public sealed class IdentifierExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.IdentifierExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Identifier => (Token)green.Identifier.ToRed(FullPosition, this);
    
    internal IdentifierExpressionSyntax(Green.IdentifierExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Identifier;
            yield break;
        }
    }
}

public sealed class StringExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.StringExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token OpenQuote => (Token)green.OpenQuote.ToRed(FullPosition, this);
    
    public SyntaxList<StringPartSyntax> Parts => (SyntaxList<StringPartSyntax>)green.Parts.ToRed(FullPosition + green.OpenQuote.GetFullWidth(), this);
    
    public Token CloseQuote => (Token)green.CloseQuote.ToRed(FullPosition + green.OpenQuote.GetFullWidth() + green.Parts.GetFullWidth(), this);
    
    internal StringExpressionSyntax(Green.StringExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
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
}

public abstract class StringPartSyntax : SyntaxNode
{
    internal StringPartSyntax(int fullPosition, SyntaxNode parent) : base(fullPosition, parent) {}
}

public sealed class TextStringPartSyntax : StringPartSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.TextStringPartSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Text => (Token)green.Text.ToRed(FullPosition, this);
    
    internal TextStringPartSyntax(Green.TextStringPartSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Text;
            yield break;
        }
    }
}

public sealed class InterpolationStringPartSyntax : StringPartSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.InterpolationStringPartSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token OpenDelimiter => (Token)green.OpenDelimiter.ToRed(FullPosition, this);
    
    public ExpressionSyntax Expression => (ExpressionSyntax)green.Expression.ToRed(FullPosition + green.OpenDelimiter.GetFullWidth(), this);
    
    public Token CloseDelimiter => (Token)green.CloseDelimiter.ToRed(FullPosition + green.OpenDelimiter.GetFullWidth() + green.Expression.GetFullWidth(), this);
    
    internal InterpolationStringPartSyntax(Green.InterpolationStringPartSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
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
}

public sealed class BoolExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.BoolExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Value => (Token)green.Value.ToRed(FullPosition, this);
    
    internal BoolExpressionSyntax(Green.BoolExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Value;
            yield break;
        }
    }
}

public sealed class NumberExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.NumberExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token Value => (Token)green.Value.ToRed(FullPosition, this);
    
    internal NumberExpressionSyntax(Green.NumberExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Value;
            yield break;
        }
    }
}

public sealed class NilExpressionSyntax : ExpressionSyntax
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Green.NilExpressionSyntax green;

    internal override Green.SyntaxNode Green => green;

    public Token OpenParen => (Token)green.OpenParen.ToRed(FullPosition, this);
    
    public Token CloseParen => (Token)green.CloseParen.ToRed(FullPosition + green.OpenParen.GetFullWidth(), this);
    
    internal NilExpressionSyntax(Green.NilExpressionSyntax green, int fullPosition, SyntaxNode parent) : base(fullPosition, parent) =>
        this.green = green;
    
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return OpenParen;
            yield return CloseParen;
            yield break;
        }
    }
}
