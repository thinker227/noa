namespace Noa.Compiler.Syntax.Green;

internal abstract class SyntaxNode
{
    public abstract int GetWidth();

    public abstract Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent);
}
