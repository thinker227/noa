using System.Diagnostics;
using Noa.Compiler.Diagnostics;
using TextMappingUtils;

namespace Noa.Compiler.Syntax;

/// <summary>
/// A concrete syntax node. Holds exact information about the syntax of a program.
/// </summary>
// Note: the implementation of ISyntaxNavigable is in SyntaxNavigation.cs.
public abstract partial class SyntaxNode : ISyntaxNavigable
{
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
    public int Position => FullPosition + GetTriviaWidth();

    /// <summary>
    /// The span of the syntax node in the corresponding source text,
    /// <i>excluding</i> its leading trivia.
    /// </summary>
    public TextSpan Span => TextSpan.FromLength(Position, CalculateNonTriviaWidth());

    /// <summary>
    /// The full span of the syntax node in the corresponding source text,
    /// <i>including</i> its leading trivia.
    /// </summary>
    public TextSpan FullSpan => TextSpan.FromLength(FullPosition, Green.GetFullWidth());

    private int CalculateNonTriviaWidth() =>
        Green.GetFullWidth() - GetTriviaWidth();

    private int GetTriviaWidth() => Green.FirstToken?.LeadingTrivia.Sum(x => x.GetFullWidth()) ?? 0;

    /// <summary>
    /// Gets all diagnostics associated with this node.
    /// </summary>
    /// <param name="source">The source of the node.</param>
    public IEnumerable<IDiagnostic> GetDiagnostics(Source source) =>
        Green.Diagnostics.Select(diag => diag.Format(source, FullPosition));

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
