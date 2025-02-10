using Noa.Compiler.Nodes;

namespace Noa.Compiler.Syntax.Green;

internal readonly record struct Token
{
    private readonly TokenKind kind;
    private readonly int width;

    public string Text { get; }
    
    public Token(TokenKind kind, string? text, int width)
    {
        this.kind = kind;
        this.width = width;

        Text = text ?? kind.ConstantString() ?? throw new InvalidOperationException(
            $"Cannot create a token with kind '{kind}' without explicitly " +
            $"specifying its text because the kind does not have a constant string");
    }

    public int Width() => width;

    public override string ToString() => Text;
}
