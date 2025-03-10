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
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
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
    /// The position of the node in the tree.
    /// This does <b>not</b> include leading trivia.
    /// </summary>
    public int Position
    {
        get
        {
            var position = FullPosition;
            var leadingTrivia = Green.FirstToken?.LeadingTrivia;
            if (leadingTrivia is not null) position += leadingTrivia.Length;
            return position;
        }
    }

    /// <summary>
    /// The span of the syntax node in the corresponding source text,
    /// <i>excluding</i> its leading trivia.
    /// </summary>
    public TextSpan Span => span ??= TextSpan.FromLength(Position, CalculateNonTriviaWidth());

    /// <summary>
    /// The full span of the syntax node in the corresponding source text,
    /// <i>including</i> its leading trivia.
    /// </summary>
    public TextSpan FullSpan => TextSpan.FromLength(FullPosition, Green.GetFullWidth());

    private int CalculateNonTriviaWidth()
    {
        var width = Green.GetFullWidth();
        var leadingTrivia = Green.FirstToken?.LeadingTrivia;
        if (leadingTrivia is not null) width -= leadingTrivia.Length;
        return width;
    }

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
