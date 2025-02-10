using System.Collections;

namespace Noa.Compiler.Syntax.Green;

internal sealed class SeparatedSyntaxList<TNode> : IReadOnlyList<NodeOrToken<TNode>>
    where TNode : SyntaxNode
{
    public ImmutableArray<TNode> Nodes { get; }

    public ImmutableArray<Token> Tokens { get; }

    public int Count => Nodes.Length + Tokens.Length;


    public NodeOrToken<TNode> this[int index] => index % 2 == 0
        ? Nodes[index]
        : Tokens[index];

    public SeparatedSyntaxList(ImmutableArray<TNode> nodes, ImmutableArray<Token> tokens)
    {
        if (tokens.Length != nodes.Length && tokens.Length != nodes.Length - 1)
            throw new ArgumentOutOfRangeException(nameof(tokens),
                "Amount of tokens have to be equal to the amount of nodes or 1 less.");
        
        Nodes = nodes;
        Tokens = tokens;
    }

    public int GetWidth() =>
        Nodes.Sum(x => x.GetWidth()) + Tokens.Sum(x => x.GetWidth());

    public IEnumerator<NodeOrToken<TNode>> GetEnumerator()
    {
        for (var i = 0; i < Nodes.Length; i++)
        {
            yield return Nodes[i];

            if (i < Tokens.Length) yield return Tokens[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
