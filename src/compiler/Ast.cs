using Noa.Compiler.Nodes;
using Noa.Compiler.Parsing;

namespace Noa.Compiler;

/// <summary>
/// A representation of a single source file.
/// </summary>
public sealed class Ast
{
    private IReadOnlyDictionary<Node, Node>? parents = null;

    /// <summary>
    /// The root of the syntax tree.
    /// </summary>
    public Root Root { get; }

    /// <summary>
    /// The diagnostics in the AST.
    /// </summary>
    public IReadOnlyCollection<IDiagnostic> Diagnostics { get; }
    
    /// <summary>
    /// The global scope in which all top-level symbols are declared.
    /// </summary>
    public Scope GlobalScope { get; internal set; } = null!;

    private Ast(Source source)
    {
        (Root, Diagnostics) = Parser.Parse(source, this);
    }

    /// <summary>
    /// This constructor exists for tests only.
    /// Otherwise <see cref="Ast(Source)"/> should be used.
    /// </summary>
    internal Ast()
    {
        Root = null!;
        Diagnostics = [];
    }

    /// <summary>
    /// Creates a new AST by parsing a source.
    /// This leaves <see cref="GlobalScope"/> as null.
    /// </summary>
    /// <param name="source">The source file to parse.</param>
    internal static Ast Parse(Source source)
    {
        return new(source);
    }

    /// <summary>
    /// Creates a new AST from source.
    /// </summary>
    /// <param name="source">The source to create the AST from.</param>
    public static Ast Create(Source source)
    {
        return Parse(source);
    }

    /// <summary>
    /// Gets the parent of a node, or null if the node is the root node.
    /// </summary>
    /// <param name="node">The node to get the parent of.</param>
    internal Node? GetParent(Node node)
    {
        parents ??= ComputeParents(Root);
        return parents.GetValueOrDefault(node);
    }

    private static Dictionary<Node, Node> ComputeParents(Node root)
    {
        var parents = new Dictionary<Node, Node>();
        Visit(root);
        return parents;

        void Visit(Node node)
        {
            foreach (var child in node.Children)
            {
                parents[child] = node;
                Visit(child);
            }
        }
    }
}
