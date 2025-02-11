using Noa.Compiler.Nodes;

namespace Noa.Compiler.Syntax;

public sealed class Token : SyntaxNode
{
    private readonly Green.Token green;

    public TokenKind Kind => green.Kind;
    
    public string Text => green.Text;

    internal Token(Green.Token green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;

    protected override int GetWidth() => green.Width;
}
