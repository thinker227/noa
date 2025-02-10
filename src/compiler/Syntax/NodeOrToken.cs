using TextMappingUtils;

namespace Noa.Compiler.Syntax;

public readonly struct NodeOrToken<TNode> where TNode : SyntaxNode
{
    private readonly TNode? node;
    private readonly Token? token;
    
    public TNode Node =>
        node is not null
            ? node
            : throw new InvalidOperationException("Node or token is not a node.");
    
    public Token Token =>
        token is {} t
            ? t
            : throw new InvalidOperationException("Node or token is not a token.");
    
    public bool IsNode => Node is not null;

    public TextSpan Span => IsNode
        ? Node.Span
        : Token.Span;

    public NodeOrToken(TNode node)
    {
        this.node = node;
        token = null;
    }

    public NodeOrToken(Token token)
    {
        node = null;
        this.token = token;
    }

    public static implicit operator NodeOrToken<TNode>(TNode node) => new(node);

    public static implicit operator NodeOrToken<TNode>(Token token) => new(token);
}
