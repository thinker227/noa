namespace Noa.Compiler.Nodes;

/// <summary>
/// Visits AST nodes.
/// </summary>
/// <typeparam name="T">The type the visitor returns.</typeparam>
public abstract partial class Visitor<T>
{
    /// <summary>
    /// Gets the default return value for a node, or a general default value if the node is null.
    /// </summary>
    /// <param name="node">The node to get the default value for, or null to get a general default value.</param>
    protected abstract T GetDefault(Node node);
    
    /// <summary>
    /// Called before visiting each node.
    /// </summary>
    /// <param name="node">The node being visited.</param>
    protected virtual void BeforeVisit(Node node) {}
    
    /// <summary>
    /// Called after visiting each node.
    /// </summary>
    /// <param name="node">The node being visited.</param>
    /// <param name="result">The result of visiting the node.</param>
    protected virtual void AfterVisit(Node node, T result) {}
    
    /// <summary>
    /// Visits a collection of nodes.
    /// </summary>
    /// <param name="nodes">The nodes to visit.</param>
    /// <param name="useReturn">Specifies whether the method should return the results of visiting the nodes.</param>
    /// <returns>
    /// Either the results of visiting the nodes if <paramref name="useReturn"/> is true,
    /// otherwise returns <see cref="ImmutableArray{T}.Empty"/>.
    /// </returns>
    public ImmutableArray<T> Visit(IEnumerable<Node> nodes, bool useReturn = false)
    {
        var builder = useReturn
            ? ImmutableArray.CreateBuilder<T>()
            : null;

        foreach (var node in nodes)
        {
            var x = Visit(node);
            builder?.Add(x);
        }

        return builder?.ToImmutable() ?? ImmutableArray<T>.Empty;
    }
}
