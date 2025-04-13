using Noa.Compiler.Parsing;

namespace Noa.Compiler.Syntax;

public abstract partial class SyntaxNode
{
    private Token? GetFirstToken(
        bool includeInvisible,
        Func<SyntaxNode, IEnumerable<SyntaxNode>> getChildren)
    {
        // Tokens override this method, this should never happen.
        if (this is Token) throw new InvalidOperationException("Cannot call GetFirstToken on a Token.");

        foreach (var child in getChildren(this))
        {
            if (child is Token token)
            {
                if (!token.IsInvisible || includeInvisible) return token;
            }
            else if (child.GetFirstToken(includeInvisible, getChildren) is { } childToken) return childToken;

            // If we failed to find a first token in the child, continue onto the next child.
        }

        // If we got here then either the node had no children (which should be impossible unless it's a token,
        // which it also shouldn't be), or we skipped all tokens because they were invisible.
        return null;
    }

    public virtual ITokenLike? GetFirstToken(bool includeInvisible = false) =>
        GetFirstToken(includeInvisible, node => node.Children);

    public virtual ITokenLike? GetLastToken(bool includeInvisible = false) =>
        // This is technically just the same as GetFirstToken, but we iterate the children in reverse.
        GetFirstToken(includeInvisible, node => node.Children.Reverse());

    public virtual ITokenLike? GetPreviousToken(bool includeInvisible = false) =>
        SyntaxNavigation.GetPreviousTokenForNodeOrToken(this, includeInvisible);
}

public sealed partial class Token
{
    public override ITokenLike? GetFirstToken(bool includeInvisible = false)
    {
        for (var i = 0; i < LeadingTrivia.Length; i++)
        {
            if (LeadingTrivia[i] is not UnexpectedTokenTrivia unexpectedToken) continue;

            if (!unexpectedToken.Kind.IsInvisible() || includeInvisible) return unexpectedToken;
        }

        return null;
    }

    public override ITokenLike? GetLastToken(bool includeInvisible = false)
    {
        for (var i = LeadingTrivia.Length - 1; i >= 0 ; i--)
        {
            if (LeadingTrivia[i] is not ITokenLike tokenTrivia) continue;

            if (!tokenTrivia.Kind.IsInvisible() || includeInvisible) return tokenTrivia;
        }

        return null;
    }

    public override ITokenLike? GetPreviousToken(bool includeInvisible = false)
    {
        // Check for unexpected tokens within trivia.
        if (GetLastToken(includeInvisible) is { } unexpectedToken) return unexpectedToken;

        // Delegate to the common utility method.
        return SyntaxNavigation.GetPreviousTokenForNodeOrToken(this, includeInvisible);
    }
}

public sealed partial class UnexpectedTokenTrivia
{
    // Unexpected token trivias do not have "first" or "last" tokens
    // since they do not contain other tokens.

    public ITokenLike? GetFirstToken(bool includeInvisible = false) => null;

    public ITokenLike? GetLastToken(bool includeInvisible = false) => null;

    public ITokenLike? GetPreviousToken(bool includeInvisible = false)
    {
        foreach (var sibling in SyntaxNavigation.IteratePreviousTokenTrivias(this))
        {
            if (!sibling.Kind.IsInvisible() || includeInvisible) return sibling;
        }

        return ParentToken.GetPreviousToken(includeInvisible);
    }
}

public sealed partial class SkippedTokenTrivia
{
    public ITokenLike? GetFirstToken(bool includeInvisible = false) => null;
    
    public ITokenLike? GetLastToken(bool includeInvisible = false) => null;
    
    public ITokenLike? GetPreviousToken(bool includeInvisible = false)
    {
        foreach (var sibling in SyntaxNavigation.IteratePreviousTokenTrivias(this))
        {
            if (!sibling.Kind.IsInvisible() || includeInvisible) return sibling;
        }

        return ParentToken.GetPreviousToken(includeInvisible);
    }
}

public static class SyntaxNavigation
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
    /// Enumerates the previous token-like trivias of a piece of trivia in reverse order.
    /// </summary>
    internal static IEnumerable<ITokenLike> IteratePreviousTokenTrivias(Trivia trivia)
    {
        var previous = new Stack<ITokenLike>();
        foreach (var sibling in trivia.ParentToken.LeadingTrivia.OfType<ITokenLike>())
        {
            if (!sibling.Equals(trivia))
            {
                previous.Push(sibling);
                continue;
            }

            return previous;
        }

        // This *should* be unreachable.
        throw new UnreachableException(
            "Failed to find source unexpected token among the trivia of its parent token.");
    }

    /// <summary>
    /// Gets the token preceding a node (or token).
    /// </summary>
    /// <param name="node">The node to get the token preceding.</param>
    /// <returns>The preceding token, or <see langword="null"/> if none could be found.</returns>
    /// <param name="includeInvisible">
    /// Whether to include invisible tokens.
    /// </param>
    internal static ITokenLike? GetPreviousTokenForNodeOrToken(this SyntaxNode node, bool includeInvisible)
    {
        // There logically is no previous token if this is the root.
        if (node.Parent is null) return null;

        foreach (var sibling in IteratePreviousSiblings(node))
        {
            if (sibling.GetLastToken(includeInvisible) is { } token) return token;
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
    /// <param name="inTrivia">
    /// Whether to count trivia as if the position is "inside" the token.
    /// Since trivia is always attached to the start of a token,
    /// if this parameter is <see langword="true"/> and <paramref name="position"/>
    /// is inside whitespace trivia, the returned token will be the token immediately
    /// to the right of the position.
    /// Note that <see cref="UnexpectedTokenTrivia"/> will still be returned
    /// even if this parameter is <see langword="false"/>.
    /// </param>
    /// <returns>
    /// The token at the position, or <see langword="null"/> if none could be found.
    /// </returns>
    public static ITokenLike? GetTokenAt(
        this SyntaxNode root,
        int position,
        bool inTrivia = true)
    {
        // Little special case to ensure that the end-of-file token is always handled as sticky.
        // This is necessary because the span of a file is always 1 greater than its actual length,
        // so if we don't have this special-case handling and we try to get the token at the
        // very end of the file, it'll return null while we're actually interested in the
        // end-of-file token.
        if (root is RootSyntax realRoot && position == root.FullSpan.End)
            return realRoot.EndOfFile;

        // This node doesn't contain the position.
        if (!root.FullSpan.Contains(position)) return null;

        var node = root;

        next:

        if (node is Token token)
        {
            // If the token directly contains the position, return it.
            if (token.Span.Contains(position)) return token;

            var returnNextTriviaToken = false;

            // Look for an unexpected token within the token's trivia.
            for (var i = 0; i < token.LeadingTrivia.Length; i++)
            {
                var trivia = token.LeadingTrivia[i];

                if (trivia.Span.Contains(position))
                {
                    // If we have found a trivia token which contains the span,
                    // then we've found our desired token.
                    // Otherwise, this must be some kind of whitespace or comment trivia,
                    // so we mark that the next encountered trivia token is the one we're looking for.
                    if (trivia is ITokenLike triviaToken) return triviaToken;
                    else if (inTrivia) returnNextTriviaToken = true;
                }
                else if (trivia is ITokenLike triviaToken && returnNextTriviaToken) return triviaToken;
            }

            // The token didn't directly contain the position,
            // and we couldn't find an unexpected token within the trivia.
            // We know that the position is within the trivia of this token,
            // so we return based on whether we allow returning a token in trivia.
            return inTrivia
                ? token
                : null;
        }

        foreach (var child in node.Children)
        {
            // If the child contains the position, search the child.
            if (child.FullSpan.Contains(position))
            {
                node = child;
                goto next;
            }
        }

        // We searched all the children of the node but didn't find anything.
        // This probably implies we skipped over the relevant token.
        return null;
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
    public static ITokenLike? GetLeftTokenAt(this SyntaxNode root, int position, bool inTrivia = true) =>
        root.GetTokenAt(position, inTrivia)?.GetPreviousToken();
}
