using Noa.Compiler.Parsing;

namespace Noa.Compiler.Syntax.Green;

internal sealed class Token(TokenKind kind, string? text, ImmutableArray<Trivia> leadingTrivia, int width) : SyntaxNode
{
    public override IEnumerable<SyntaxNode> Children => [];

    public TokenKind Kind { get; } = kind;

    public int FullWidth { get; } = width + leadingTrivia.Sum(x => x.GetFullWidth());

    public string Text { get; } = text ?? kind.ConstantString() ?? throw new InvalidOperationException(
        $"Cannot create a token with kind '{kind}' without explicitly " +
        $"specifying its text because the kind does not have a constant string");
    
    public ImmutableArray<Trivia> LeadingTrivia { get; } = leadingTrivia;

    public override int GetFullWidth() => FullWidth;

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) => new Syntax.Token(this, position, parent);

    /// <summary>
    /// Returns the token with a new set of leading trivia.
    /// </summary>
    /// <param name="trivia">The new leading trivia.</param>
    public Token WithTrivia(ImmutableArray<Trivia> trivia) =>
        new(Kind, Text, trivia, width);
    
    private Token AddTokenAsTrivia(Token token, Func<Token, Trivia> createTrivia)
    {
        var trivia = createTrivia(token);
        ReadOnlySpan<Trivia> appendTrivia = [..token.LeadingTrivia, trivia];
        var newTrivia = LeadingTrivia.InsertRange(0, appendTrivia);

        var newToken = WithTrivia(newTrivia);

        if (token.Diagnostics.Count > 0)
        {
            var existingTriviaWidth = token.LeadingTrivia.Sum(x => x.GetFullWidth());

            foreach (var diagnostic in token.Diagnostics)
            {
                newToken.AddDiagnostic(diagnostic.AddOffset(-existingTriviaWidth));
            }
        }

        return newToken;
    }

    /// <summary>
    /// Returns the token with another token appended as unexpected token trivia.
    /// The other token will have its own leading trivia appended to the new token's trivia as well.
    /// </summary>
    /// <param name="unexpected">The token to append as an unexpected token.</param>
    public Token AddTokenAsUnexpectedTrivia(Token unexpected) =>
        AddTokenAsTrivia(unexpected, t => t.ToUnexpectedTokenTrivia());

    /// <summary>
    /// Returns the token with another token appended as skipped token trivia.
    /// The other token will have its own leading trivia appended to the new token's trivia as well.
    /// </summary>
    /// <param name="unexpected">The token to append as a skipped token.</param>
    public Token AddTokenAsSkippedTrivia(Token skipped) =>
        AddTokenAsTrivia(skipped, t => t.ToSkippedTokenTrivia());

    /// <summary>
    /// Converts the token into an <see cref="UnexpectedTokenTrivia"/>.
    /// </summary>
    public UnexpectedTokenTrivia ToUnexpectedTokenTrivia() =>
        new(Kind, Text, width);

    /// <summary>
    /// Converts the token into an <see cref="SkippedTokenTrivia"/>.
    /// </summary>
    public SkippedTokenTrivia ToSkippedTokenTrivia() =>
        new(Kind, Text, width);

    /// <summary>
    /// Returns a sequence of the token's trivia
    /// followed by an <see cref="UnexpectedTokenTrivia"/> constructed from the token.
    /// </summary>
    public IEnumerable<Trivia> ToTriviaFollowedByUnexpectedToken() =>
        LeadingTrivia.Append(new UnexpectedTokenTrivia(Kind, Text, width));

    public override string ToString() => Text;
}
