
namespace Noa.Compiler.Syntax;

/// <summary>
/// A syntax token, a single unit of syntax.
/// </summary>
public sealed class Token : SyntaxNode
{
    private readonly Green.Token green;
    private ImmutableArray<Trivia>? leadingTrivia = null;

    internal override Green.SyntaxNode Green => green;

    public override IEnumerable<SyntaxNode> Children => LeadingTrivia;

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
    /// </summary>
    public ImmutableArray<Trivia> LeadingTrivia => leadingTrivia ??=
        green.LeadingTrivia
            .Select(x => (Trivia)x.ToRed(FullPosition, this))
            .ToImmutableArray();

    /// <summary>
    /// The full leading trivia of the token.
    /// This includes any leading trivia <i>within</i> the trivia of the current token,
    /// namely within <see cref="UnexpectedTokenTrivia"/>.
    /// </summary>
    public IEnumerable<Trivia> FullLeadingTrivia => LeadingTrivia
        .OfType<UnexpectedTokenTrivia>()
        .SelectMany(x => x.Token.FullLeadingTrivia.Prepend(x));

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
