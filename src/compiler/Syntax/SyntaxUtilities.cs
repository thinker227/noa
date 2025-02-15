namespace Noa.Compiler.Syntax;

public static class SyntaxUtilities
{
    /// <summary>
    /// Gets the first token within a syntax node.
    /// </summary>
    /// <param name="node">The node to get the first token token of.</param>
    public static Token GetFirstToken(this SyntaxNode node)
    {
        while (true)
        {
            if (node is Token token) return token;

            // Only tokens should be leaves and not have any children.
            node = node.Children.FirstOrDefault()
                ?? throw new InvalidOperationException("Node has no children.");
        }
    }

    /// <summary>
    /// Gets the last token within a syntax node.
    /// </summary>
    /// <param name="node">The node to get the last token token of.</param>
    public static Token GetLastToken(this SyntaxNode node)
    {
        while (true)
        {
            if (node is Token token) return token;

            // Only tokens should be leaves and not have any children.
            node = node.Children.LastOrDefault()
                ?? throw new InvalidOperationException("Node has no children.");
        }
    }

    /// <summary>
    /// Gets the token preceding a node (or token).
    /// </summary>
    /// <param name="node">The node to get the token preceding.</param>
    /// <returns>The preceding token, or <see langword="null"/> if none could be found.</returns>
    public static Token? GetPreviousToken(this SyntaxNode node)
    {
        // Find the previous node in the tree by navigating up one level
        // and trying to find the previous sibling to the node.
        // If the node is the first among its siblings then we have to recurse
        // upwards one level and do the same search on the parent node.

        if (node.Parent is null) return null;

        var previous = null as SyntaxNode;
        foreach (var sibling in node.Parent.Children)
        {
            if (!sibling.Equals(node))
            {
                previous = sibling;
                continue;
            }

            if (previous is null)
            {
                // This was the first sibling in the parent.
                // We have to recurse upwards to try find the previous node.
                return node.Parent.GetPreviousToken();
            }
            else
            {
                // We found the previous sibling!
                return previous.GetLastToken();
            }
        }

        // We couldn't find a previous node in any of the children.
        // Something probably went wrong, this should be unreachable.
        throw new UnreachableException();
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
