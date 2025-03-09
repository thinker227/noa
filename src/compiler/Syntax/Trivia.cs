namespace Noa.Compiler.Syntax;

/// <summary>
/// Trivia associated with a <see cref="Token"/>
/// which doesn't have any significance syntactically.
/// </summary>
public abstract class Trivia : SyntaxNode
{
    internal Trivia(int fullPosition, SyntaxNode parent) : base(fullPosition, parent) { }
}

/// <summary>
/// Trivia about whitespace.
/// </summary>
/// <param name="whitespace">The whitespace of the token.</param>
public sealed class WhitespaceTrivia : Trivia
{
    private readonly Green.WhitespaceTrivia green;

    internal override Green.SyntaxNode Green => green;

    public override IEnumerable<SyntaxNode> Children => [];

    /// <summary>
    /// The whitespace text.
    /// </summary>
    public string WhitespaceText => green.WhitespaceText;

    internal WhitespaceTrivia(Green.WhitespaceTrivia green, int fullPosition, SyntaxNode parent)
        : base(fullPosition, parent) =>
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
    private readonly Green.CommentTrivia green;

    internal override Green.SyntaxNode Green => green;

    public override IEnumerable<SyntaxNode> Children => [];

    /// <summary>
    /// The full text of the comment, including the leading <c>//</c>.
    /// </summary>
    public string FullText => green.FullText;

    /// <summary>
    /// The comment text, not including the leading <c>//</c>.
    /// This does include any whitespace immediately after the <c>//</c>, however.
    /// </summary>
    public string CommentText => FullText[2..];

    internal CommentTrivia(Green.CommentTrivia green, int fullPosition, SyntaxNode parent)
        : base(fullPosition, parent) =>
        this.green = green;
}

/// <summary>
/// Trivia about a single unexpected token.
/// </summary>
public sealed class UnexpectedTokenTrivia : Trivia
{
    private readonly Green.UnexpectedTokenTrivia green;

    internal override Green.SyntaxNode Green => green;

    public override IEnumerable<SyntaxNode> Children => [Token];

    /// <summary>
    /// The unexpected token.
    /// This token might have trivia of itself, potentially forming a sub-tree of trivia.
    /// </summary>
    public Token Token => new(green.Token, FullPosition, this);

    internal UnexpectedTokenTrivia(Green.UnexpectedTokenTrivia green, int fullPosition, SyntaxNode parent)
        : base(fullPosition, parent) =>
        this.green = green;
}

/// <summary>
/// Trivia about a single unexpected character.
/// </summary>
public sealed class UnexpectedCharacterTrivia : Trivia
{
    private readonly Green.UnexpectedCharacterTrivia green;

    internal override Green.SyntaxNode Green => green;

    public override IEnumerable<SyntaxNode> Children => [];

    /// <summary>
    /// The unexpected character.
    /// Yes, this is a string and not a char, because emojis.
    /// </summary>
    public string Character => green.Character;

    internal UnexpectedCharacterTrivia(Green.UnexpectedCharacterTrivia green, int fullPosition, SyntaxNode parent)
        : base(fullPosition, parent) =>
        this.green = green;
}
