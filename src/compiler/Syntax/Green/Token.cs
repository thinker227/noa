using Noa.Compiler.Nodes;

namespace Noa.Compiler.Syntax.Green;

internal readonly record struct Token(TokenKind Kind, string? Text, int Width)
{
    public string Text { get; } = Text ?? Kind.ConstantString() ?? throw new InvalidOperationException(
        $"Cannot create a token with kind '{Kind}' without explicitly " +
        $"specifying its text because the kind does not have a constant string");

    public int GetWidth() => Width;

    public Syntax.Token ToRed(int position, Syntax.SyntaxNode parent) => new(position, parent, this);

    public override string ToString() => Text;
}
