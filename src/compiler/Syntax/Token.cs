using Noa.Compiler.Nodes;

namespace Noa.Compiler.Syntax;

/// <summary>
/// A syntax token, a single unit of syntax.
/// </summary>
public sealed class Token : SyntaxNode
{
    private readonly Green.Token green;

    /// <summary>
    /// The kind of the token.
    /// </summary>
    public TokenKind Kind => green.Kind;
    
    /// <summary>
    /// The text of the token.
    /// </summary>
    public string Text => green.Text;

    internal Token(Green.Token green, int position, SyntaxNode parent) : base(position, parent) =>
        this.green = green;

    protected override int GetWidth() => green.Width;
}
