namespace Noa.Compiler.Syntax;

public static class SyntaxUtilities
{
    /// <summary>
    /// Gets the first ancestor of a specified type of a node and optionally matching a filter.
    /// </summary>
    /// <typeparam name="T">The type of the ancestor node to get.</typeparam>
    /// <param name="node">The descendant node to get the ancestor of.</param>
    /// <param name="filter">An optional filter to filter for the desired ancestor node.</param>
    /// <returns>
    /// The ancestor of the specified type and matching the filter if provided,
    /// or <see langword="null"/> if none could be found.
    /// </returns>
    public static T? GetFirstAncestorOfType<T>(this SyntaxNode node, Func<T, bool>? filter = null)
        where T : SyntaxNode
    {
        var current = node;

        while (current is not null)
        {
            if (current is T x && (filter?.Invoke(x) ?? true)) return x;
            
            current = current.Parent;
        }

        return null;
    }

    /// <summary>
    /// Gets the first token within a syntax node.
    /// This is used as common method for both <see cref="GetFirstToken(SyntaxNode, bool)"/>
    /// and <see cref="GetLastToken(SyntaxNode, bool)"/> since they both do pretty much the same thing,
    /// except <see cref="GetLastToken(SyntaxNode, bool)"/> enumerates the children in reverse.
    /// </summary>
    private static Token? GetFirstToken(
        SyntaxNode node,
        bool includeInvisible,
        Func<SyntaxNode, IEnumerable<SyntaxNode>> getChildren)
    {
        if (node is Token t) return t;

        foreach (var child in getChildren(node))
        {
            if (child is Token token)
            {
                // Continue if the token is invisible and we should not include invisible tokens.
                if (token.IsInvisible && !includeInvisible) continue;
                else return token;
            }

            if (GetFirstToken(child, includeInvisible, getChildren) is { } childToken) return childToken;

            // If we failed to find a first token in the child, continue onto the next child.
        }

        // If we got here then either the node had no children (which should be impossible unless it's a token,
        // which it also shouldn't be), or we skipped all tokens because they were invisible.
        return null;
    }

    /// <summary>
    /// Gets the first token within a syntax node.
    /// </summary>
    /// <param name="node">The node to get the first token token of.</param>
    /// <param name="includeInvisible">
    /// Whether to include invisible tokens.
    /// If <see langword="true"/>, the method will return <see langword="null"/>
    /// if the node does not have any visible tokens.
    /// </param>
    public static Token? GetFirstToken(this SyntaxNode node, bool includeInvisible = false) =>
        GetFirstToken(node, includeInvisible, n => n.Children);

    /// <summary>
    /// Gets the last token within a syntax node.
    /// </summary>
    /// <param name="node">The node to get the last token token of.</param>
    /// <param name="includeInvisible">
    /// Whether to include invisible tokens.
    /// If <see langword="true"/>, the method will return <see langword="null"/>
    /// if the node does not have any visible tokens.
    /// </param>
    public static Token? GetLastToken(this SyntaxNode node, bool includeInvisible = false) =>
        // This is technically just the same as GetFirstToken, but we iterate the children in reverse.
        GetFirstToken(node, includeInvisible, n => n.Children.Reverse());
    
    /// <summary>
    /// Gets the index of a node within its parent.
    /// For instance, the <c>let</c> token of a <see cref="LetDeclarationSyntax"/>
    /// will have index 0, the name index 1, <c>=</c> index 2, expression index 3, and <c>;</c> index 4.
    /// </summary>
    /// <param name="node">The node to get the index of.</param>
    /// <exception cref="InvalidOperationException">
    /// The node doesn't have a parent.
    /// </exception>
    public static int GetIndexInParent(this SyntaxNode node)
    {
        if (node.Parent is null) throw new InvalidOperationException(
            "Cannot get index in parent of the root node.");
        
        var index = 0;
        foreach (var sibling in node.Parent.Children)
        {
            if (sibling.Equals(node)) break;
            index += 1;
        }

        return index;
    }
    
    /// <summary>
    /// Enumerates the previous siblings of a node in reverse order.
    /// </summary>
    private static IEnumerable<SyntaxNode> IteratePreviousSiblings(SyntaxNode node)
    {
        if (node.Parent is null) return [];

        var previous = new Stack<SyntaxNode>();
        foreach (var sibling in node.Parent.Children)
        {
            if (!sibling.Equals(node))
            {
                previous.Push(sibling);
                continue;
            }

            return previous;
        }

        // This *should* be unreachable.
        throw new UnreachableException(
            "Failed to find source node among the children of its parent.");
    }

    /// <summary>
    /// Gets the token preceding a node (or token).
    /// </summary>
    /// <param name="node">The node to get the token preceding.</param>
    /// <returns>The preceding token, or <see langword="null"/> if none could be found.</returns>
    /// <param name="includeInvisible">
    /// Whether to include invisible tokens.
    /// </param>
    public static Token? GetPreviousToken(this SyntaxNode node, bool includeInvisible = false)
    {
        // There logically is no previous token if this is the root.
        if (node.Parent is null) return null;

        foreach (var sibling in IteratePreviousSiblings(node))
        {
            if (sibling.GetLastToken(includeInvisible) is Token token) return token;
        }

        // If we (for some reason) couldn't find a previous token in the node's preceding siblings,
        // try get the previous token of the parent.
        return node.Parent.GetPreviousToken(includeInvisible);
    }
    
    /// <summary>
    /// Gets the token at a specified position in a syntax node.
    /// </summary>
    /// <param name="root">The root node to search from.</param>
    /// <param name="position">The position to find the token at.</param>
    /// <param name="inTrivia">Whether to count trivia as if the position is "inside" the token.</param>
    /// <returns>
    /// The token at the position, or <see langword="null"/> if none could be found.
    /// </returns>
    public static Token? GetTokenAt(
        this SyntaxNode root,
        int position,
        bool inTrivia = true)
    {
        // This node doesn't contain the position.
        if (!WithinSpan(root)) return null;

        var node = root;

        next:

        // If the current node is a token, then we've reached an end
        // since it contains no children to search.
        if (node is Token token) return token;

        foreach (var child in node.Children)
        {
            // If the child contains the position, search the child.
            if (WithinSpan(child))
            {
                node = child;
                goto next;
            }
        }

        // We searched all the children of the node but didn't find anything.
        // This probably implies we skipped over the relevant token.
        return null;

        bool WithinSpan(SyntaxNode node)
        {
            var span = inTrivia
                ? node.FullSpan
                : node.Span;
            
            return span.Contains(position);
        }
    }

    /// <summary>
    /// Gets the token immediately to the left of a position in a syntax node.
    /// </summary>
    /// <param name="root">The root node to search from.</param>
    /// <param name="position">The position to find the token immediately to the left of.</param>
    /// <param name="inTrivia">Whether to count trivia as if the position is "inside" the token.</param>
    /// <returns>
    /// The token immediately to the left of the position, or <see langword="null"/> if none could be found.
    /// </returns>
    public static Token? GetLeftTokenAt(this SyntaxNode root, int position, bool inTrivia = true) =>
        root.GetTokenAt(position, inTrivia)?.GetPreviousToken();
}
