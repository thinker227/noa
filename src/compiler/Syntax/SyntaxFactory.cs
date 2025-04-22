using Noa.Compiler.Parsing;

namespace Noa.Compiler.Syntax;

/// <summary>
/// Factory methods for constructing syntax nodes.
/// </summary>
public static partial class SyntaxFactory
{
    private static string GetText(TokenKind kind) =>
        kind.ConstantString()
            ?? throw new InvalidOperationException($"Kind {kind} requires text.");

    /// <summary>
    /// Constructs a <see cref="Syntax.Token"/>.
    /// </summary>
    /// <param name="kind">
    // The kind of the token.
    // Throws an exception if the kind does not have a constant string.
    // </param>
    /// <param name="trivia">The leading trivia of the token.</param>
    public static Token Token(TokenKind kind, params ImmutableArray<Trivia> trivia) =>
        Token(kind, GetText(kind), trivia);

    /// <summary>
    /// Constructs a <see cref="Syntax.Token"/>.
    /// </summary>
    /// <param name="kind">The kind of the token.</param>
    /// <param name="text">The text of the token.</param>
    /// <param name="trivia">The leading trivia of the token.</param>
    public static Token Token(TokenKind kind, string text, params ImmutableArray<Trivia> trivia)
    {   
        var green = new Green.Token(kind, text, trivia.Select(x => x.green).ToImmutableArray(), text.Length);

        return (Token)green.ToRed(0, null!);
    }

    /// <summary>
    /// Constructs a <see cref="WhitespaceTrivia"/>.
    /// </summary>
    /// <param name="whitespace">The whitespace.</param>
    public static WhitespaceTrivia Whitespace(string whitespace)
    {
        var green = new Green.WhitespaceTrivia(whitespace);

        return green.ToRed(0, null!);
    }

    /// <summary>
    /// Constructs an <see cref="UnexpectedTokenTrivia"/>.
    /// </summary>
    /// <param name="kind">
    // The kind of the token.
    // Throws an exception if the kind does not have a constant string.
    // </param>
    /// <param name="trivia">The leading trivia of the token.</param>
    public static UnexpectedTokenTrivia UnexpectedToken(TokenKind kind) =>
        UnexpectedToken(kind, GetText(kind));

    /// <summary>
    /// Constructs an <see cref="UnexpectedTokenTrivia"/>.
    /// </summary>
    /// <param name="kind">The kind of the token.</param>
    /// <param name="text">The text of the token.</param>
    /// <param name="trivia">The leading trivia of the token.</param>
    public static UnexpectedTokenTrivia UnexpectedToken(TokenKind kind, string text)
    {
        var green = new Green.UnexpectedTokenTrivia(kind, text, text.Length);

        return green.ToRed(0, null!);
    }

    /// <summary>
    /// Constructs a <see cref="SkippedTokenTrivia"/>.
    /// </summary>
    /// <param name="kind">
    // The kind of the token.
    // Throws an exception if the kind does not have a constant string.
    // </param>
    /// <param name="trivia">The leading trivia of the token.</param>
    public static SkippedTokenTrivia SkippedToken(TokenKind kind) =>
        SkippedToken(kind, GetText(kind));

    /// <summary>
    /// Constructs a <see cref="SkippedTokenTrivia"/>.
    /// </summary>
    /// <param name="kind">The kind of the token.</param>
    /// <param name="text">The text of the token.</param>
    /// <param name="trivia">The leading trivia of the token.</param>
    public static SkippedTokenTrivia SkippedToken(TokenKind kind, string text)
    {
        var green = new Green.SkippedTokenTrivia(kind, text, text.Length);

        return green.ToRed(0, null!);
    }
}
