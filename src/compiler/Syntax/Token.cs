
namespace Noa.Compiler.Syntax;

/// <summary>
/// A syntax token, a single unit of syntax.
/// </summary>
public sealed class Token : SyntaxNode
{
    private readonly Green.Token green;

    internal override Green.SyntaxNode Green => green;

    public override IEnumerable<SyntaxNode> Children => [];

    /// <summary>
    /// The kind of the token.
    /// </summary>
    public TokenKind Kind => green.Kind;
    
    /// <summary>
    /// The text of the token.
    /// </summary>
    public string Text => green.Text;

    /// <summary>
    /// The leading trivia of the token.
    /// Trivia counts as whitespace and any invalid characters encountered
    /// between the previous and current token.
    /// </summary>
    public string LeadingTrivia => green.LeadingTrivia;

    /// <summary>
    /// Whether the token is invisible, i.e. does not consist of any text.
    /// This is only the case for tokens with the kind
    /// <see cref="TokenKind.Error"/> or <see cref="TokenKind.EndOfFile"/>.
    /// </summary>
    public bool IsInvisible => Kind is TokenKind.Error or TokenKind.EndOfFile;

    internal Token(Green.Token green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;

    public override bool Equals(object? obj) =>
        obj is Token other &&
        other.green == green;

    public override int GetHashCode() =>
        green.GetHashCode();
}
