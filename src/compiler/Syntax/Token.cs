using Noa.Compiler.Nodes;
using TextMappingUtils;

namespace Noa.Compiler.Syntax;

public readonly struct Token
{
    private readonly Green.Token green;

    public SyntaxNode Parent { get; }

    public TokenKind Kind => green.Kind;
    
    public string Text => green.Text;

    public TextSpan Span { get; }

    internal Token(int position, SyntaxNode parent, Green.Token green)
    {
        this.green = green;
        Parent = parent;
        Span = TextSpan.FromLength(position, green.Width);
    }
}
