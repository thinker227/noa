using System.Runtime.CompilerServices;

namespace Noa.Compiler.Syntax.Green;

/// <summary>
/// An internal "green" concrete syntax node.
/// <br/><br/>
/// Red nodes (<see cref="Syntax.SyntaxNode"/>) are public API wrappers around green nodes.
/// Red nodes have absolute spans and back-references to their parents, while green nodes don't.
/// Green nodes are also meant to be incrementally reusable.
/// </summary>
internal abstract class SyntaxNode
{
    private static readonly ConditionalWeakTable<SyntaxNode, List<PartialDiagnostic>> diagnostics = [];

    /// <summary>
    /// The nodes which are direct children of the node.
    /// </summary>
    public abstract IEnumerable<SyntaxNode> Children { get; }

    /// <summary>
    /// The diagnostics for the node.
    /// </summary>
    public IReadOnlyCollection<PartialDiagnostic> Diagnostics =>
        diagnostics.TryGetValue(this, out var ds)
            ? ds
            : [];
    
    /// <summary>
    /// Adds a diagnostic to the node.
    /// </summary>
    /// <param name="diagnostic">The partial diagnostic to add.</param>
    public void AddDiagnostic(PartialDiagnostic diagnostic)
    {
        if (!diagnostics.TryGetValue(this, out var ds))
        {
            ds = [];
            diagnostics.Add(this, ds);
        }

        ds.Add(diagnostic);
    }

    /// <summary>
    /// Gets the width of the node, including trivia.
    /// </summary>
    /// <returns></returns>
    public abstract int GetWidth();

    /// <summary>
    /// Creates a red node from this green node.
    /// </summary>
    /// <param name="position">The position of the red node.</param>
    /// <param name="parent">The parent of the red node.</param>
    public abstract Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent);
}
