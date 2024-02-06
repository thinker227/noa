// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace Noa.Compiler.Nodes;

public static class NodeUtility
{
    /// <summary>
    /// Finds an ancestor node of a specified type, or null if the node has no
    /// ancestor of the specified type. 
    /// </summary>
    /// <param name="node">The node to find an ancestor of.</param>
    /// <typeparam name="T">The type of the ancestor node to find.</typeparam>
    public static T? FindAncestor<T>(this Node node) where T : Node
    {
        var parent = node.Parent.Value;
        
        return parent switch
        {
            T x => x,
            null => null,
            _ => FindAncestor<T>(parent)
        };
    }

    /// <summary>
    /// Gets the ancestors of a node, in order from the node's parent to the root.
    /// </summary>
    /// <param name="node">The node to get the ancestors of.</param>
    public static IEnumerable<Node> Ancestors(this Node node) =>
        node.Parent.Value is not null
            ? node.AncestorsAndSelf()
            : [];

    /// <summary>
    /// Gets the ancestors of a node and the node itself, in order from the node to the root.
    /// </summary>
    /// <param name="node">The node to get the ancestors of.</param>
    public static IEnumerable<Node> AncestorsAndSelf(this Node node) =>
        node.Ancestors().Prepend(node);
    
    /// <summary>
    /// Gets the descendants of a node, in depth-first order.
    /// </summary>
    /// <param name="node">The node to get the descendants of.</param>
    public static IEnumerable<Node> Descendants(this Node node) =>
        node.Children.SelectMany(DescendantsAndSelf);

    /// <summary>
    /// Gets the descendants of a node and the node itself, in depth-first order.
    /// </summary>
    /// <param name="node">The node to get the descendants of.</param>
    public static IEnumerable<Node> DescendantsAndSelf(this Node node) =>
        node.Descendants().Prepend(node);
}
