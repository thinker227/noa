using System.Collections;
using System.Reflection;

namespace Noa.Compiler.Syntax.Green;

internal sealed class SeparatedSyntaxList<TNode> : SyntaxNode, IReadOnlyList<SyntaxNode>
    where TNode : SyntaxNode
{
    // Hack because C# doesn't have associated types.
    // Get a reference to the ConstructorInfo for the (Green.SyntaxNode, int, Syntax.SyntaxNode, IReadOnlyList<Green.SyntaxNode>)
    // constructor of Syntax.SeparatedSyntaxList<TRed> so we can instantiate it without statically knowing
    // the type of TRed.

    private static Type RedElementType { get; } = Assembly
        .GetExecutingAssembly()
        .GetType($"Noa.Compiler.Syntax.{typeof(TNode).Name}")!;
    
    private static Type RedNodeType { get; } = typeof(Syntax.SeparatedSyntaxList<>).MakeGenericType(RedElementType);

    private static ConstructorInfo RedNodeConstructor { get; } = RedNodeType.GetConstructor(
        BindingFlags.NonPublic | BindingFlags.Instance,
        [
            typeof(Green.SyntaxNode),
            typeof(int),
            typeof(Syntax.SyntaxNode),
            typeof(IReadOnlyList<Green.SyntaxNode>)
        ])!;
    
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
        (Syntax.SyntaxNode)RedNodeConstructor.Invoke([this, position, parent, elements]);
}
