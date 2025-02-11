using System.Diagnostics;
using TextMappingUtils;

namespace Noa.Compiler.Syntax;

/// <summary>
/// A concrete syntax node. Holds exact information about the syntax of a program.
/// </summary>
public abstract class SyntaxNode
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private TextSpan? span = null;

    /// <summary>
    /// The corresponding node in the green tree.
    /// </summary>
    internal abstract Green.SyntaxNode Green { get; }
    
    /// <summary>
    /// The parent syntax node.
    /// </summary>
    public SyntaxNode Parent { get; }

    /// <summary>
    /// The full position of the node in the tree.
    /// If the node has leading trivia, this is at the start of that trivia.
    /// </summary>
    public int FullPosition { get; }

    /// <summary>
    /// The span of the syntax node in the corresponding source text.
    /// </summary>
    public TextSpan Span => span ??= TextSpan.FromLength(FullPosition, Green.GetWidth());

    /// <summary>
    /// The nodes which are direct children of the node.
    /// </summary>
    public abstract IEnumerable<SyntaxNode> Children { get; }

    internal SyntaxNode(int fullPosition, SyntaxNode parent)
    {
        FullPosition = fullPosition;
        Parent = parent;
    }
}
