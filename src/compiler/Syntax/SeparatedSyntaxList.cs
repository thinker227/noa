using System.Collections;

namespace Noa.Compiler.Syntax;

/// <summary>
/// A list which holds syntax nodes separated by tokens.
/// </summary>
/// <typeparam name="TNode">The type of the nodes in the list.</typeparam>
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

    /// <summary>
    /// The amount of nodes in the list.
    /// </summary>
    public int NodesCount => (Count + 1) / 2;

    /// <summary>
    /// The amount of tokens in the list.
    /// </summary>
    public int TokensCount => Count / 2;

    internal SeparatedSyntaxList(
        int position,
        Syntax.SyntaxNode parent,
        IReadOnlyList<Green.SyntaxNode> elements)
        : base(position, parent)
    {
        constructed = new SyntaxNode[elements.Count];
        this.elements = elements;
    }

    protected override int GetWidth() => elements.Sum(x => x.GetWidth());

    /// <summary>
    /// Enumerates the nodes in the list.
    /// </summary>
    public IEnumerable<TNode> Nodes() =>
        this.OfType<TNode>();
    
    /// <summary>
    /// Enumerates the tokens in the list.
    /// </summary>
    public IEnumerable<Token> Tokens() =>
        this.OfType<Token>();

    /// <summary>
    /// Gets the node at the specified index.
    /// </summary>
    /// <param name="index">The index to get the node at.</param>
    public TNode GetNodeAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, NodesCount);
        return (TNode)GetElemAt(index * 2);
    }

    /// <summary>
    /// Gets the token at the specified index.
    /// </summary>
    /// <param name="index">The index to get the token at.</param>
    public Token GetTokenAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, TokensCount);
        return (Token)GetElemAt(index * 2 + 1);
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
