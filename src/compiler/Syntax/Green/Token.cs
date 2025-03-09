using Noa.Compiler.Parsing;

namespace Noa.Compiler.Syntax.Green;

internal sealed class Token(TokenKind kind, string? text, ImmutableArray<Trivia> leadingTrivia, int width) : SyntaxNode
{
    public override IEnumerable<SyntaxNode> Children => LeadingTrivia;

    public TokenKind Kind { get; } = kind;

    public int FullWidth { get; } = width + leadingTrivia.Length;

    public string Text { get; } = text ?? kind.ConstantString() ?? throw new InvalidOperationException(
        $"Cannot create a token with kind '{kind}' without explicitly " +
        $"specifying its text because the kind does not have a constant string");
    
    public ImmutableArray<Trivia> LeadingTrivia { get; } = leadingTrivia;

    public override int GetFullWidth() => FullWidth;

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) => new Syntax.Token(this, position, parent);

    public override string ToString() => Text;
}
