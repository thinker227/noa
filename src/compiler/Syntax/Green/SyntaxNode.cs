using System.Runtime.CompilerServices;
using Noa.Compiler.Diagnostics;

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
    private static readonly ConditionalWeakTable<SyntaxNode, List<IPartialDiagnostic>> diagnostics = [];

    /// <summary>
    /// The nodes which are direct children of the node.
    /// </summary>
    public abstract IEnumerable<SyntaxNode> Children { get; }

    /// <summary>
    /// The first token which is a descendant of this node,
    /// or this node if it happens to be a token.
    /// </summary>
    public Token? FirstToken
    {
        get
        {
            var node = this;
            while (true)
            {
                if (node is Token token) return token;
                node = node.Children.FirstOrDefault();
                if (node is null) return null;
            }
        }
    }

    /// <summary>
    /// The last token which is a descendant of this node,
    /// or this node if it happens to be a token.
    /// </summary>
    public Token? LastToken
    {
        get
        {
            var node = this;
            while (true)
            {
                if (node is Token token) return token;
                node = node.Children.LastOrDefault();
                if (node is null) return null;
            }
        }
    }

    /// <summary>
    /// The diagnostics for the node.
    /// </summary>
    public IReadOnlyCollection<IPartialDiagnostic> Diagnostics =>
        diagnostics.TryGetValue(this, out var ds)
            ? ds
            : [];
    
    /// <summary>
    /// Adds a diagnostic to the node.
    /// </summary>
    /// <param name="diagnostic">The partial diagnostic to add.</param>
    public void AddDiagnostic(IPartialDiagnostic diagnostic)
    {
        if (!diagnostics.TryGetValue(this, out var ds))
        {
            ds = [];
            diagnostics.Add(this, ds);
        }

        ds.Add(diagnostic);
    }

    /// <summary>
    /// Gets the width of the node, excluding trivia.
    /// </summary>
    public int GetSnugWidth()
    {
        var width = 0;

        if (this is Token token) return token.Width;

        var first = true;
        foreach (var child in Children)
        {
            if (first)
            {
                width += child.GetSnugWidth();
                first = false;
            }
            else
            {
                width += child.GetFullWidth();
            }
        }

        return width;
    }

    /// <summary>
    /// Gets the width of the node, including trivia.
    /// </summary>
    public abstract int GetFullWidth();

    /// <summary>
    /// Creates a red node from this green node.
    /// </summary>
    /// <param name="position">The position of the red node.</param>
    /// <param name="parent">The parent of the red node.</param>
    public abstract Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent);
}
