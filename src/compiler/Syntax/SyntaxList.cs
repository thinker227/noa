using System.Collections;

namespace Noa.Compiler.Syntax;

public sealed class SyntaxList<TNode> : SyntaxNode, IReadOnlyList<TNode> where TNode : SyntaxNode
{
    private readonly TNode?[] constructed;
    private readonly IReadOnlyList<Green.SyntaxNode> elements;

    public int Count => elements.Count;


    public TNode this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

            if (constructed[index] is { } x) return x;

            return this.ElementAt(index);
        }
    }

    internal SyntaxList(
        int position,
        Syntax.SyntaxNode parent,
        IReadOnlyList<Green.SyntaxNode> elements)
        : base(position, parent)
    {
        constructed = new TNode[elements.Count];
        this.elements = elements;
    }

    protected override int GetWidth() => elements.Sum(x => x.GetWidth());

    public IEnumerator<TNode> GetEnumerator()
    {
        var offset = 0;

        for (var i = 0; i < Count; i++)
        {
            TNode node;
            if (constructed[i] is { } x) node = x;
            else
            {
                node = (TNode)elements[i].ToRed(position + offset, this);
                constructed[i] = node;
            }

            offset += node.Span.Length;

            yield return node;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
