
namespace Noa.Compiler.Syntax.Green;

internal sealed class SyntaxList<TNode>(ImmutableArray<TNode> nodes) : SyntaxNode
    where TNode : SyntaxNode
{
    public override IEnumerable<SyntaxNode> Children => nodes;

    public override int GetWidth() => nodes.Sum(x => x.GetWidth());

    public override Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent) =>
        (Syntax.SyntaxNode)ReflectionInfo<TNode>.RedSyntaxListConstructor.Invoke([this, position, parent, nodes]);
}
