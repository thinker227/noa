using System.Collections;

namespace Noa.Compiler.Syntax.Green;

internal sealed class SeparatedSyntaxList<TNode> : SyntaxNode, IReadOnlyList<SyntaxNode>
    where TNode : SyntaxNode
{    
    private readonly ImmutableArray<SyntaxNode> elements;

    public int Count => elements.Length;

    public SyntaxNode this[int index] => elements[index];

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

    public override int GetWidth() =>
        elements.Sum(x => x.GetWidth());

    public IEnumerator<SyntaxNode> GetEnumerator() => elements.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        (Syntax.SyntaxNode)ReflectionInfo<TNode>.RedSeparatedSyntaxListConstructor.Invoke([this, position, parent, elements]);
}
