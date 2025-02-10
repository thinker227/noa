using System.Collections;

namespace Noa.Compiler.Syntax;

public readonly struct SeparatedSyntaxList<T> : IReadOnlyList<NodeOrToken<T>> where T : SyntaxNode
{
    private readonly int position;
    private readonly SyntaxNode parent;
    private readonly IReadOnlyList<Green.SyntaxNode> nodes;
    private readonly IReadOnlyList<Green.Token> tokens;

    public NodeOrToken<T> this[int index] => throw new NotImplementedException();

    public int Count => throw new NotImplementedException();

    internal SeparatedSyntaxList(
        int position,
        SyntaxNode parent,
        IReadOnlyList<Green.SyntaxNode> nodes,
        IReadOnlyList<Green.Token> tokens)
    {
        this.position = position;
        this.parent = parent;
        this.nodes = nodes;
        this.tokens = tokens;
    }

    public IEnumerator<NodeOrToken<T>> GetEnumerator() => throw new NotImplementedException();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
