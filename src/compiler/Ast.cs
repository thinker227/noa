using Noa.Compiler.Nodes;
using Noa.Compiler.Parsing;

namespace Noa.Compiler;

/// <summary>
/// A representation of a single source file.
/// </summary>
public sealed class Ast
{
    private readonly Root? root;
    private IReadOnlyDictionary<Node, Node>? parents = null;

    /// <summary>
    /// The root of the syntax tree.
    /// </summary>
    // The root is exposed to the public API as non-nullable because it won't be
    // null when control is returned to the caller, the field is only null
    // within the constructor.
    public Root Root => root!;

    /// <summary>
    /// The diagnostics in the AST.
    /// </summary>
    public IReadOnlyCollection<IDiagnostic> Diagnostics { get; }
    
    /// <summary>
    /// The global scope in which all top-level symbols are declared.
    /// </summary>
    public Semantic<Scope> GlobalScope { get; internal set; }

    private Ast(Source source)
    {
        (root, Diagnostics) = Parser.Parse(source, this);
    }

    /// <summary>
    /// This constructor exists for tests only.
    /// Otherwise <see cref="Ast(Source)"/> should be used.
    /// </summary>
    internal Ast()
    {
        root = null;
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
    internal Semantic<Node?> GetParent(Node node)
    {
        if (root is null) return new();

        parents ??= ComputeParents(root);
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
