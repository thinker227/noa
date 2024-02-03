using System.Collections.Frozen;

namespace Noa.Compiler.Parsing;

internal static class SyntaxFacts
{
    public static bool IsWhitespace(char c) => char.IsWhiteSpace(c);

    public static bool CanBeginName(char c) =>
        char.IsLetter(c);

    public static bool CanBeInName(char c) =>
        CanBeginName(c) || char.IsNumber(c);

    public static bool IsDigit(char c) =>
        c is >= '0' and <= '9';

    public static FrozenSet<TokenKind> CanBeginDeclaration { get; } = new[]
    {
        TokenKind.Func,
        TokenKind.Let
    }.ToFrozenSet();

    public static FrozenSet<TokenKind> CanBeginPrimaryExpression { get; } = new[]
    {
        TokenKind.OpenParen,
        TokenKind.OpenBrace,
        TokenKind.If,
        TokenKind.Loop,
        TokenKind.Return,
        TokenKind.Break,
        TokenKind.Continue,
        TokenKind.Name,
        TokenKind.True,
        TokenKind.False,
        TokenKind.Number
    }.ToFrozenSet();

    public static FrozenSet<TokenKind> CanBeginExpression { get; } =
        CanBeginPrimaryExpression
            .Concat([
                TokenKind.Plus,
                TokenKind.Dash
            ]).ToFrozenSet();

    public static FrozenSet<TokenKind> CanBeginDeclarationOrExpression { get; } =
        CanBeginDeclaration
            .Concat(CanBeginExpression)
            .ToFrozenSet();

    public static FrozenSet<TokenKind> RootSynchronize { get; } = CanBeginDeclarationOrExpression;

    public static FrozenSet<TokenKind> BlockExpressionSynchronize { get; } =
        RootSynchronize
            .Append(TokenKind.CloseBrace)
            .ToFrozenSet();
}
