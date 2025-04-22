
using System.Collections;

namespace Noa.Compiler.Syntax.Green;

internal sealed class SyntaxList<TNode>(ImmutableArray<TNode> nodes)
    : SyntaxNode, IReadOnlyList<TNode>
    where TNode : SyntaxNode
{
    public TNode this[int index] => nodes[index];

    public int Count => nodes.Length;

    public override IEnumerable<SyntaxNode> Children => nodes;

    public static SyntaxList<TNode> Create(IEnumerable<SyntaxNode> nodes) =>
        new(nodes.Select(x => (TNode)x).ToImmutableArray());

    public override int GetFullWidth() => nodes.Sum(x => x.GetFullWidth());

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        (Syntax.SyntaxNode)ReflectionInfo<TNode>.RedSyntaxListConstructor.Invoke([this, position, parent, nodes]);

    public IEnumerator<TNode> GetEnumerator() => nodes.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
