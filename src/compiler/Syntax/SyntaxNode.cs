using System.Diagnostics;
using TextMappingUtils;

namespace Noa.Compiler.Syntax;

/// <summary>
/// A concrete syntax node. Holds exact information about the syntax of a program.
/// </summary>
public abstract class SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal readonly int position;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private TextSpan? span = null;
    
    /// <summary>
    /// The parent syntax node.
    /// </summary>
    public SyntaxNode Parent { get; }

    /// <summary>
    /// The span of the syntax node in the corresponding source text.
    /// </summary>
    public TextSpan Span => span ??= TextSpan.FromLength(position, GetWidth());

    /// <summary>
    /// The nodes which are direct children of the node.
    /// </summary>
    public abstract IEnumerable<SyntaxNode> Children { get; }

    internal SyntaxNode(int position, SyntaxNode parent)
    {
        this.position = position;
        Parent = parent;
    }

    protected abstract int GetWidth();
}
