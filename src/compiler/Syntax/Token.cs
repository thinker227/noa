using Noa.Compiler.Nodes;

namespace Noa.Compiler.Syntax;

public sealed class Token : SyntaxNode
{
    internal Green.Token Green => (Green.Token)green;

    public TokenKind Kind => Green.Kind;
    
    public string Text => Green.Text;

    internal Token(Green.Token green, int position, SyntaxNode parent)
        : base(green, position, parent) {}
}
