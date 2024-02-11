// ReSharper disable LocalVariableHidesMember

using Noa.Compiler.Diagnostics;
using Noa.Compiler.FlowAnalysis;
using Noa.Compiler.Nodes;
using Noa.Compiler.Parsing;
using Noa.Compiler.Symbols;

namespace Noa.Compiler;

/// <summary>
/// A representation of a single source file.
/// </summary>
public sealed class Ast
{
    private readonly Root? root;
    private readonly List<IDiagnostic> diagnostics;
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
    public IReadOnlyCollection<IDiagnostic> Diagnostics => diagnostics;
    
    /// <summary>
    /// The global scope in which all top-level symbols are declared.
    /// </summary>
    public Semantic<IScope> GlobalScope { get; internal set; }

    private Ast(Source source)
    {
        var (root, diagnostics) = Parser.Parse(source, this);
        
        this.root = root;
        this.diagnostics = diagnostics.ToList();
    }

    /// <summary>
    /// This constructor exists for tests only.
    /// Otherwise <see cref="Ast(Source)"/> should be used.
    /// </summary>
    internal Ast()
    {
        root = null;
        diagnostics = [];
    }

    /// <summary>
    /// Creates a new AST by parsing a source.
    /// </summary>
    /// <param name="source">The source file to parse.</param>
    internal static Ast Parse(Source source) =>
        new(source);

    /// <summary>
    /// Creates a new AST from source.
    /// </summary>
    /// <param name="source">The source to create the AST from.</param>
    public static Ast Create(Source source)
    {
        var ast = Parse(source);

        var symbolDiagnostics = SymbolResolution.ResolveSymbols(ast);
        ast.diagnostics.AddRange(symbolDiagnostics);

        var flowDiagnostics = FlowAnalyzer.Analyze(ast);
        ast.diagnostics.AddRange(flowDiagnostics);

        return ast;
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
