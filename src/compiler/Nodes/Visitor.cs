using System.Diagnostics.CodeAnalysis;

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
    protected abstract T GetDefault(Node? node);

    /// <summary>
    /// Filters for nodes before visiting them.
    /// </summary>
    /// <param name="node">The node to filter.</param>
    /// <param name="result">The return value of the visit if the method return false.</param>
    /// <returns>True if the node should be visited, otherwise false.</returns>
    protected virtual bool Filter(Node node, [MaybeNullWhen(true)] out T result)
    {
        result = default;
        return true;
    }
    
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
    /// Visits a node. Handles calling <see cref="Filter"/>, <see cref="BeforeVisit"/>,
    /// and <see cref="AfterVisit"/>. This method should preferably always be called
    /// instead of any specialized visit methods.
    /// </summary>
    /// <param name="node">The node to visit.</param>
    /// <returns>
    /// The result of visiting the node, or <see cref="GetDefault"/> if null.
    /// </returns>
    public T Visit(Node? node)
    {
        if (node is null) return GetDefault(null);

        if (!Filter(node, out var result)) return result;

        BeforeVisit(node);
        result = CoreVisit(node);
        AfterVisit(node, result);

        return result;
    }
    
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

/// <summary>
/// Visits AST nodes.
/// </summary>
public abstract partial class Visitor
{
    /// <summary>
    /// Filters for nodes before visiting them.
    /// </summary>
    /// <param name="node">The node to filter.</param>
    /// <returns>True if the node should be visited, otherwise false.</returns>
    protected virtual bool Filter(Node node) => true;
    
    /// <summary>
    /// Called before visiting each node.
    /// </summary>
    /// <param name="node">The node being visited.</param>
    protected virtual void BeforeVisit(Node node) {}
    
    /// <summary>
    /// Called after visiting each node.
    /// </summary>
    /// <param name="node">The node being visited.</param>
    protected virtual void AfterVisit(Node node) {}

    /// <summary>
    /// Visits a node. Handles calling <see cref="Filter"/>, <see cref="BeforeVisit"/>,
    /// and <see cref="AfterVisit"/>. This method should preferably always be called
    /// instead of any specialized visit methods.
    /// </summary>
    /// <param name="node">The node to visit.</param>
    public void Visit(Node? node)
    {
        if (node is null) return;

        if (!Filter(node)) return;
        
        BeforeVisit(node);
        CoreVisit(node);
        AfterVisit(node);
    }
    
    /// <summary>
    /// Visits a collection of nodes.
    /// </summary>
    /// <param name="nodes">The nodes to visit.</param>
    public void Visit(IEnumerable<Node> nodes)
    {
        foreach (var node in nodes) Visit(node);
    }
}
