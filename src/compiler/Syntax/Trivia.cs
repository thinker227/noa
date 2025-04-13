using TextMappingUtils;

namespace Noa.Compiler.Syntax;

/// <summary>
/// Trivia associated with a <see cref="Token"/>
/// which doesn't have any significance syntactically.
/// </summary>
public abstract class Trivia
{
    internal readonly Green.Trivia green;
    private readonly int fullPosition;

    /// <summary>
    /// The parent token which the trivia is trivia of.
    /// </summary>
    public Token ParentToken { get; }

    /// <summary>
    /// The span of the trivia.
    /// Compared to syntax nodes, this is already the "full" span.
    /// </summary>
    public TextSpan Span => TextSpan.FromLength(fullPosition, green.GetFullWidth());

    internal Trivia(Green.Trivia green, int fullPosition, Token parent)
    {
        this.green = green;
        this.fullPosition = fullPosition;
        ParentToken = parent;
    }

    public override bool Equals(object? obj) =>
        obj is Trivia trivia &&
        trivia.green == green;

    public override int GetHashCode() => green.GetHashCode();
}

/// <summary>
/// Trivia about whitespace.
/// </summary>
/// <param name="whitespace">The whitespace of the token.</param>
public sealed class WhitespaceTrivia : Trivia
{
    private new readonly Green.WhitespaceTrivia green;

    /// <summary>
    /// The whitespace text.
    /// </summary>
    public string WhitespaceText => green.WhitespaceText;

    internal WhitespaceTrivia(Green.WhitespaceTrivia green, int fullPosition, Token parent)
        : base(green, fullPosition, parent) =>
        this.green = green;
}

/// <summary>
/// Trivia about a single comment.
/// </summary>
/// <remarks>
/// Comments on consecutive lines do not count as the same comment,
/// they will be multiple comment trivias with whitespace trivias separating them.
/// </remarks>
public sealed class CommentTrivia : Trivia
{
    private new readonly Green.CommentTrivia green;

    /// <summary>
    /// The full text of the comment, including the leading <c>//</c>.
    /// </summary>
    public string FullText => green.FullText;

    /// <summary>
    /// The comment text, not including the leading <c>//</c>.
    /// This does include any whitespace immediately after the <c>//</c>, however.
    /// </summary>
    public string CommentText => FullText[2..];

    internal CommentTrivia(Green.CommentTrivia green, int fullPosition, Token parent)
        : base(green, fullPosition, parent) =>
        this.green = green;
}

/// <summary>
/// Trivia about a single unexpected token.
/// </summary>
// Note: the implementation of ISyntaxNavigable is in SyntaxNavigation.cs.
public sealed partial class UnexpectedTokenTrivia : Trivia, ITokenLike
{
    private new readonly Green.UnexpectedTokenTrivia green;

    public TokenKind Kind => green.Kind;

    public string Text => green.Text;

    SyntaxNode ISyntaxNavigable.ParentNode => ParentToken.Parent;

    internal UnexpectedTokenTrivia(Green.UnexpectedTokenTrivia green, int fullPosition, Token parent)
        : base(green, fullPosition, parent) =>
        this.green = green;
}

/// <summary>
/// Trivia about a single skipped token.
/// </summary>
// Note: the implementation of ISyntaxNavigable is in SyntaxNavigation.cs.
public sealed partial class SkippedTokenTrivia : Trivia, ITokenLike
{
    private new readonly Green.SkippedTokenTrivia green;

    public TokenKind Kind => green.Kind;

    public string Text => green.Text;

    SyntaxNode ISyntaxNavigable.ParentNode => ParentToken.Parent;

    internal SkippedTokenTrivia(Green.SkippedTokenTrivia green, int fullPosition, Token parent)
        : base(green, fullPosition, parent) =>
        this.green = green;
}

/// <summary>
/// Trivia about a single unexpected character.
/// </summary>
public sealed class UnexpectedCharacterTrivia : Trivia
{
    private new readonly Green.UnexpectedCharacterTrivia green;

    /// <summary>
    /// The unexpected character.
    /// Yes, this is a string and not a char, because emojis.
    /// </summary>
    public string Character => green.Character;

    internal UnexpectedCharacterTrivia(Green.UnexpectedCharacterTrivia green, int fullPosition, Token parent)
        : base(green, fullPosition, parent) =>
        this.green = green;
}
