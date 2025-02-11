using System.Diagnostics;
using TextMappingUtils;

namespace Noa.Compiler.Syntax;

public abstract class SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal readonly int position;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private TextSpan? span = null;
    
    public SyntaxNode Parent { get; }

    public TextSpan Span => span ??= TextSpan.FromLength(position, GetWidth());

    internal SyntaxNode(int position, SyntaxNode parent)
    {
        this.position = position;
        Parent = parent;
    }

    protected abstract int GetWidth();
}
