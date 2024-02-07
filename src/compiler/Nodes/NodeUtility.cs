namespace Noa.Compiler.Nodes;

public static class NodeUtility
{
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
