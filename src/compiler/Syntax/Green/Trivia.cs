
namespace Noa.Compiler.Syntax.Green;

/// <summary>
/// Trivia associated with a <see cref="Token"/>
/// which doesn't have any significance syntactically.
/// </summary>
internal abstract class Trivia : SyntaxNode;

internal sealed class WhitespaceTrivia(string whitespace) : Trivia
{
    public string WhitespaceText { get; } = whitespace;

    public override IEnumerable<SyntaxNode> Children => [];

    public override Syntax.WhitespaceTrivia ToRed(int fullPosition, Syntax.SyntaxNode parent) =>
        new(this, fullPosition, parent);

    public override int GetFullWidth() => WhitespaceText.Length;
}

internal sealed class CommentTrivia(string fullText) : Trivia
{
    public string FullText { get; } = fullText;

    public override IEnumerable<SyntaxNode> Children => [];

    public override Syntax.CommentTrivia ToRed(int fullPosition, Syntax.SyntaxNode parent) =>
        new(this, fullPosition, parent);

    public override int GetFullWidth() => FullText.Length;
}

internal sealed class UnexpectedTokenTrivia(Token token) : Trivia
{
    public Token Token { get; } = token;

    public override IEnumerable<SyntaxNode> Children => [Token];

    public override Syntax.UnexpectedTokenTrivia ToRed(int fullPosition, Syntax.SyntaxNode parent) =>
        new(this, fullPosition, parent);

    public override int GetFullWidth() => Token.FullWidth;
}

internal sealed class UnexpectedCharacterTrivia(string character) : Trivia
{
    public string Character { get; } = character;

    public override IEnumerable<SyntaxNode> Children => [];

    public override Syntax.UnexpectedCharacterTrivia ToRed(int fullPosition, Syntax.SyntaxNode parent) =>
        new(this, fullPosition, parent);

    public override int GetFullWidth() => Character.Length;
}
