using System.Collections;

namespace Noa.Compiler.Syntax.Green;

internal sealed class SeparatedSyntaxList<TNode>
    : SyntaxNode, ISeparatedSyntaxList<SyntaxNode, TNode, Token>
    where TNode : SyntaxNode
{    
    private readonly ImmutableArray<SyntaxNode> elements;

    public override IEnumerable<SyntaxNode> Children => elements;

    public int Count => elements.Length;

    public int NodesCount => (Count + 1) / 2;

    public int TokensCount => Count / 2;

    public bool HasTrailingSeparator => Count % 2 == 0;

    public SyntaxNode this[int index] => elements[index];

    public static SeparatedSyntaxList<TNode> Empty { get; } = new([]);

    private SeparatedSyntaxList(ImmutableArray<SyntaxNode> elements) =>
        this.elements = elements;

    public static SeparatedSyntaxList<TNode> Create(IEnumerable<TNode> nodes, IEnumerable<Token> tokens)
    {
        var ne = nodes.GetEnumerator();
        var te = tokens.GetEnumerator();

        var elements = ImmutableArray.CreateBuilder<SyntaxNode>();

        while (ne.MoveNext())
        {
            elements.Add(ne.Current);

            if (!te.MoveNext())
            {
                if (ne.MoveNext()) throw new ArgumentException(
                    "Amount of tokens has to be equal to the amount of nodes or 1 less.",
                    nameof(tokens));

                break;
            }
            else elements.Add(te.Current);
        }

        if (te.MoveNext())
        {
            elements.Add(te.Current);

            if (te.MoveNext()) throw new ArgumentException(
                "Amount of tokens has to be equal to the amount of nodes or 1 less.",
                nameof(tokens));
        }

        return new(elements.ToImmutable());
    }


    public IEnumerable<TNode> Nodes() => this.OfType<TNode>();
    
    public IEnumerable<Token> Tokens() => this.OfType<Token>();

    public TNode GetNodeAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, NodesCount);
        return (TNode)elements[index];
    }

    public Token GetTokenAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, TokensCount);
        return (Token)elements[index];
    }

    public override int GetWidth() =>
        elements.Sum(x => x.GetWidth());

    public IEnumerator<SyntaxNode> GetEnumerator() => elements.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        (Syntax.SyntaxNode)ReflectionInfo<TNode>.RedSeparatedSyntaxListConstructor.Invoke([this, position, parent, elements]);
}
