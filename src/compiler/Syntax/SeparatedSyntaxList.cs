using System.Collections;

namespace Noa.Compiler.Syntax;

public sealed class SeparatedSyntaxList<TNode> : SyntaxNode, IReadOnlyList<SyntaxNode>
    where TNode : SyntaxNode
{
    private readonly SyntaxNode?[] constructed;
    private readonly IReadOnlyList<Green.SyntaxNode> elements;


    public SyntaxNode this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            return GetElemAt(index);
        }
    }

    public int Count => elements.Count;

    public int NodesCount => Count / 2 + 1;

    public int TokensCount => Count / 2;

    internal SeparatedSyntaxList(
        Green.SyntaxNode green,
        int position,
        Syntax.SyntaxNode parent,
        IReadOnlyList<Green.SyntaxNode> elements)
        : base(green, position, parent)
    {
        constructed = new SyntaxNode[elements.Count];
        this.elements = elements;
    }

    public IEnumerable<TNode> Nodes() =>
        this.OfType<TNode>();
    
    public IEnumerable<Token> Tokens() =>
        this.OfType<Token>();

    public TNode GetNodeAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, NodesCount);
        return (TNode)GetElemAt(index * 2);
    }

    public TNode GetTokenAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, TokensCount);
        return (TNode)GetElemAt(index * 2 + 1);
    }

    private SyntaxNode GetElemAt(int index) =>
        constructed[index] is { } x
            ? x
            : this.ElementAt(index);

    public IEnumerator<SyntaxNode> GetEnumerator()
    {
        var offset = 0;

        for (var i = 0; i < Count; i++)
        {
            SyntaxNode elem;
            if (constructed[i] is { } x) elem = x;
            else
            {
                elem = elements[i].ToRed(position + offset, this);
                constructed[i] = elem;
            }

            offset += elem.Span.Length;

            yield return elem;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
