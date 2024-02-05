using System.Collections.Frozen;

namespace Noa.Compiler.Parsing;

internal static class SyntaxFacts
{
    /// <summary>
    /// Returns whether a character is a whitespace character.
    /// </summary>
    public static bool IsWhitespace(char c) => char.IsWhiteSpace(c);

    /// <summary>
    /// Returns whether a character can begin a name.
    /// </summary>
    public static bool CanBeginName(char c) =>
        char.IsLetter(c);

    /// <summary>
    /// Returns whether a character can be inside of a name.
    /// </summary>
    public static bool CanBeInName(char c) =>
        CanBeginName(c) || char.IsNumber(c);

    /// <summary>
    /// Returns whether a character is a digit.
    /// </summary>
    public static bool IsDigit(char c) =>
        c is >= '0' and <= '9';

    /// <summary>
    /// The set of tokens which can begin a declaration.
    /// </summary>
    public static FrozenSet<TokenKind> CanBeginDeclaration { get; } = new[]
    {
        TokenKind.Func,
        TokenKind.Let
    }.ToFrozenSet();

    /// <summary>
    /// The set of tokens which can begin a primary expression,
    /// i.e. any expression excluding unary expressions.
    /// </summary>
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

    /// <summary>
    /// The set of tokens which can begin a unary expression.
    /// This is just the set of tokens for unary expression operators.
    /// </summary>
    public static FrozenSet<TokenKind> CanBeginUnaryExpression { get; } = new[]
    {
        TokenKind.Plus,
        TokenKind.Dash,
        TokenKind.Bang
    }.ToFrozenSet();

    /// <summary>
    /// The set of tokens which are binary expression operators.
    /// </summary>
    public static FrozenSet<TokenKind> BinaryExpressionOperator { get; } = new[]
    {
        TokenKind.Plus,
        TokenKind.Dash,
        TokenKind.Star,
        TokenKind.Slash,
        TokenKind.EqualsEquals,
        TokenKind.BangEquals,
        TokenKind.LessThan,
        TokenKind.GreaterThan,
        TokenKind.LessThanEquals,
        TokenKind.GreaterThanEquals
    }.ToFrozenSet();

    /// <summary>
    /// The set of tokens which can begin an expression.
    /// </summary>
    public static FrozenSet<TokenKind> CanBeginExpression { get; } =
        CanBeginPrimaryExpression
            .Concat(CanBeginUnaryExpression)
            .ToFrozenSet();

    /// <summary>
    /// The set of tokens which can appear after a <see cref="TokenKind.Name"/> token, continuing an expression.
    /// For instance, <c>(</c> is counted because it can appear directly after a name in a call expression,
    /// but <c>{</c> is not because, while it may appear syntactically after a name in an if-expression,
    /// it's not part of the <i>same</i> expression.
    /// </summary>
    public static FrozenSet<TokenKind> CanAppearAfterNameInSameExpression { get; } = 
        BinaryExpressionOperator
            .Concat([
                TokenKind.OpenParen
            ]).ToFrozenSet();

    /// <summary>
    /// The set of tokens which can begin a declaration or expression.
    /// </summary>
    public static FrozenSet<TokenKind> CanBeginDeclarationOrExpression { get; } =
        CanBeginDeclaration
            .Concat(CanBeginExpression)
            .ToFrozenSet();

    /// <summary>
    /// The set of tokens to synchronize with inside a syntax root.
    /// </summary>
    public static FrozenSet<TokenKind> RootSynchronize { get; } = CanBeginDeclarationOrExpression;

    /// <summary>
    /// The set of tokens to synchronize with inside a block expression.
    /// </summary>
    public static FrozenSet<TokenKind> BlockExpressionSynchronize { get; } =
        RootSynchronize
            .Append(TokenKind.CloseBrace)
            .ToFrozenSet();

    /// <summary>
    /// The set of tokens to synchronize with inside a lambda parameter list.
    /// </summary>
    public static FrozenSet<TokenKind> LambdaParameterListSynchronize { get; } = new[]
    {
        TokenKind.Mut,
        TokenKind.Name,
        TokenKind.Comma,
        TokenKind.CloseParen,
        TokenKind.EqualsGreaterThan
    }.ToFrozenSet();
}
