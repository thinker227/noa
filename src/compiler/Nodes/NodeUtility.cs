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
            ? node.Parent.Value.AncestorsAndSelf()
            : [];

    /// <summary>
    /// Gets the ancestors of a node and the node itself, in order from the node to the root.
    /// </summary>
    /// <param name="node">The node to get the ancestors of.</param>
    public static IEnumerable<Node> AncestorsAndSelf(this Node node) =>
        node.Ancestors().Prepend(node);

    /// <summary>
    /// Gets the descendants of a node, in depth-first order.
    /// If <paramref name="descend"/> is false for the node, no nodes are returned.
    /// </summary>
    /// <param name="node">The node to get the descendants of.</param>
    /// <param name="descend">
    /// A function which determines whether to return a node and continue descending into its children.
    /// </param>
    public static IEnumerable<Node> Descendants(this Node node, Func<Node, bool>? descend = null) =>
        descend?.Invoke(node) ?? true
            ? node.Children
                .SelectMany(child => child.Descendants(descend))
                .Prepend(node)
            : [];

    /// <summary>
    /// Gets the descendants of a node and the node itself, in depth-first order.
    /// </summary>
    /// <param name="node">The node to get the descendants of.</param>
    /// <param name="descend">
    /// A function which determines whether to return a node and continue descending into its children.
    /// </param>
    public static IEnumerable<Node> DescendantsAndSelf(this Node node, Func<Node, bool>? descend = null) =>
        node.Descendants(descend).Prepend(node);
    
    /// <summary>
    /// Finds a node at a specified position in source.
    /// </summary>
    /// <param name="node">The node to find the node at the position within.</param>
    /// <param name="position">The position to find the node at.</param>
    /// <returns>
    /// The node at <paramref name="position"/>, or null if the position is outside the node.
    /// </returns>
    public static Node? FindNodeAt(this Node node, int position)
    {
        // This node doesn't contain the position.
        if (!node.Location.Contains(position)) return null;

        foreach (var child in node.Children)
        {
            // If the child contains the position, search the child.
            if (child.Location.Contains(position)) return child.FindNodeAt(position);
        }

        // If no child contains the position, it must be in this node.
        return node;
    }
}
