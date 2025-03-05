using Noa.Compiler.Parsing;
using Noa.Compiler.Services.Context;
using Noa.Compiler.Syntax;

namespace Noa.Compiler.Services.Completion.Providers;

/// <summary>
/// An <see cref="ICompletionProvider"/> which provides base functionality
/// for providing completions for keywords.
/// </summary>
public abstract class BaseKeywordProvider : ICompletionProvider
{
    public abstract IEnumerable<TokenKind> Keywords { get; }

    public abstract bool IsApplicable(SyntaxContext ctx);

    public IEnumerable<CompletionItem> GetCompletionItems(SyntaxContext ctx) =>
        IsApplicable(ctx)
            ? Keywords.Select(kind =>
                new CompletionItem.Keyword(kind.ConstantString()
                    ?? throw new InvalidOperationException($"{kind} does not have a constant string.")))
            : [];
}

/// <summary>
/// Provider for <c>mut</c>.
/// </summary>
public sealed class MutKeywordProvider : BaseKeywordProvider
{
    public override IEnumerable<TokenKind> Keywords => [TokenKind.Mut];

    public override bool IsApplicable(SyntaxContext ctx) =>
        ctx.Kind.HasFlag(SyntaxContextKind.ParameterOrVariable);
}

/// <summary>
/// Provider for <c>else</c>.
/// </summary>
public sealed class ElseKeywordProvider : BaseKeywordProvider
{
    public override IEnumerable<TokenKind> Keywords => [TokenKind.Else];

    public override bool IsApplicable(SyntaxContext ctx) =>
        ctx.Kind.HasFlag(SyntaxContextKind.PostIfBodyWithoutElse);
}

/// <summary>
/// Provides keywords for the start of statements.
/// </summary>
public sealed class StatementKeywordProvider : BaseKeywordProvider
{
    public override IEnumerable<TokenKind> Keywords => [
        TokenKind.Let,
        TokenKind.Func,
    ];

    public override bool IsApplicable(SyntaxContext ctx) =>
        ctx.Kind.HasFlag(SyntaxContextKind.Statement);
}

/// <summary>
/// Provides keywords for the start of expressions or expression statements.
/// </summary>
public sealed class ExpressionStatementKeywordProvider : BaseKeywordProvider
{
    public override IEnumerable<TokenKind> Keywords => [
        TokenKind.Return,
        TokenKind.If,
        TokenKind.Loop,
    ];

    public override bool IsApplicable(SyntaxContext ctx) =>
        ctx.Kind.HasFlag(SyntaxContextKind.Expression) ||
        ctx.Kind.HasFlag(SyntaxContextKind.Statement);
}

/// <summary>
/// Provides keywords for the start of expression or expression statements
/// within loops.
/// </summary>
public sealed class LoopExpressionStatementKeywordProvider : BaseKeywordProvider
{
    public override IEnumerable<TokenKind> Keywords => [
        TokenKind.Break,
        TokenKind.Continue
    ];

    public override bool IsApplicable(SyntaxContext ctx) =>
        ctx.Kind.HasFlag(SyntaxContextKind.InLoop) &&
        (ctx.Kind.HasFlag(SyntaxContextKind.Expression) ||
         ctx.Kind.HasFlag(SyntaxContextKind.Statement));
}

/// <summary>
/// Provides keywords for the start of expressions.
/// </summary>
public sealed class ExpressionKeywordProvider : BaseKeywordProvider
{
    public override IEnumerable<TokenKind> Keywords => [
        TokenKind.True,
        TokenKind.False,
    ];

    public override bool IsApplicable(SyntaxContext ctx) =>
        ctx.Kind.HasFlag(SyntaxContextKind.Expression);
}

/// <summary>
/// Provides keywords for the continuation of an expression.
/// </summary>
public sealed class ExpressionContinuationKeywordProvider : BaseKeywordProvider
{
    public override IEnumerable<TokenKind> Keywords => [];

    public override bool IsApplicable(SyntaxContext ctx) =>
        ctx.Kind.HasFlag(SyntaxContextKind.PostExpression);
}

/// <summary>
/// Provides symbol completions.
/// </summary>
public sealed class SymbolProvider : ICompletionProvider
{
    public IEnumerable<CompletionItem> GetCompletionItems(SyntaxContext ctx) =>
        ctx.Kind.HasFlag(SyntaxContextKind.Expression) ||
        ctx.Kind.HasFlag(SyntaxContextKind.Statement)
            ? ctx.AccessibleSymbols.Select(symbol => new CompletionItem.Symbol(symbol))
            : [];
}
