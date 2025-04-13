using TextMappingUtils;

namespace Noa.Compiler.Syntax;

/// <summary>
/// Anything token-like, including <see cref="Token"/> and <see cref="UnexpectedTokenTrivia"/>.
/// </summary>
public interface ITokenLike : ISyntaxNavigable
{
    /// <summary>
    /// The kind of the token.
    /// </summary>
    TokenKind Kind { get; }

    /// <summary>
    /// The text of the token.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// The span of the token, <b>not</b> including leading trivia.
    /// That is, the span of the text of the token.
    /// </summary>
    TextSpan Span { get; }
}
