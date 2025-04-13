using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Noa.Compiler.Syntax;

/// <summary>
/// A list which holds syntax nodes separated by tokens.
/// </summary>
/// <typeparam name="TNode">The type of the nodes in the list.</typeparam>
[CollectionBuilder(typeof(SeparatedSyntaxListBuilder), nameof(SeparatedSyntaxListBuilder.Build))]
public sealed class SeparatedSyntaxList<TNode>
    : SyntaxNode, ISeparatedSyntaxList<SyntaxNode, TNode, Token>
    where TNode : SyntaxNode
{
    private readonly SyntaxNode?[] constructed;
    private readonly IReadOnlyList<Green.SyntaxNode> elements;

    internal override Green.SyntaxNode Green { get; }

    public override IEnumerable<SyntaxNode> Children => this;

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

    public int NodesCount => (Count + 1) / 2;

    public int TokensCount => Count / 2;

    public bool HasTrailingSeparator => Count % 2 == 0;

    internal SeparatedSyntaxList(
        Green.SyntaxNode green,
        int position,
        Syntax.SyntaxNode parent,
        IReadOnlyList<Green.SyntaxNode> elements)
        : base(position, parent)
    {
        Green = green;
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
                elem = elements[i].ToRed(FullPosition + offset, this);
                constructed[i] = elem;
            }

            offset += elem.FullSpan.Length;

            yield return elem;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override bool Equals(object? obj) =>
        obj is SeparatedSyntaxList<TNode> other &&
        other.Green == Green;

    public override int GetHashCode() =>
        Green.GetHashCode();
}

public static class SeparatedSyntaxListBuilder
{
    public static SeparatedSyntaxList<T> Build<T>(ReadOnlySpan<SyntaxNode> xs) where T : SyntaxNode
    {
        var greenElementsArray = new Green.SyntaxNode[xs.Length];
        for (var i = 0; i < xs.Length; i++) greenElementsArray[i] = xs[i].Green;
        var greenElements = ImmutableCollectionsMarshal.AsImmutableArray(greenElementsArray);
        
        var green = (Green.SyntaxNode)ReflectionInfo<T>.GreenSeparatedSyntaxListCreate.Invoke(null, [greenElements])!;

        return new SeparatedSyntaxList<T>(green, 0, null!, greenElements);
    }
}
