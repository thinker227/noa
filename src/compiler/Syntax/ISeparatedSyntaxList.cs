namespace Noa.Compiler.Syntax;

internal interface ISeparatedSyntaxList<T, TNode, TToken> : IReadOnlyList<T>
    where TNode : T
    where TToken : T
{
    /// <summary>
    /// The amount of nodes in the list.
    /// </summary>
    int NodesCount { get; }

    /// <summary>
    /// The amount of tokens in the list.
    /// </summary>
    int TokensCount { get; }

    /// <summary>
    /// Whether the list has a trailing separator.
    /// </summary>
    bool HasTrailingSeparator { get; }

    /// <summary>
    /// Enumerates the nodes in the list.
    /// </summary>
    IEnumerable<TNode> Nodes();
    
    /// <summary>
    /// Enumerates the tokens in the list.
    /// </summary>
    IEnumerable<TToken> Tokens();

    /// <summary>
    /// Gets the node at the specified index.
    /// </summary>
    /// <param name="index">The index to get the node at.</param>
    TNode GetNodeAt(int index);

    /// <summary>
    /// Gets the token at the specified index.
    /// </summary>
    /// <param name="index">The index to get the token at.</param>
    TToken GetTokenAt(int index);
}
