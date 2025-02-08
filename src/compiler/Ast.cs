// ReSharper disable LocalVariableHidesMember

using Noa.Compiler.ControlFlow;
using Noa.Compiler.Diagnostics;
using Noa.Compiler.Emit;
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
    /// The source for the AST.
    /// </summary>
    public Source Source { get; }

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
    /// Whether the AST contains any errors.
    /// </summary>
    public bool HasErrors => Diagnostics.Any(x => x.Severity == Severity.Error);
    
    /// <summary>
    /// The global scope in which all top-level symbols are declared.
    /// </summary>
    public Semantic<IScope> GlobalScope { get; internal set; }

    /// <summary>
    /// The top-level function.
    /// </summary>
    public TopLevelFunction TopLevelFunction => Root.Function.Value;

    private Ast(Source source, CancellationToken cancellationToken)
    {
        var (root, diagnostics) = Parser.Parse(source, this, cancellationToken);

        this.root = root;
        this.diagnostics = diagnostics.ToList();
        Source = source;
    }

    /// <summary>
    /// This constructor exists for tests only.
    /// Otherwise <see cref="Ast(Source, CancellationToken)"/> should be used.
    /// </summary>
    internal Ast()
    {
        root = null;
        diagnostics = [];
        Source = default;
    }

    /// <summary>
    /// Creates a new AST by parsing a source.
    /// </summary>
    /// <param name="source">The source file to parse.</param>
    /// <param name="cancellationToken">The cancellation token for the parser.</param>
    internal static Ast Parse(Source source, CancellationToken cancellationToken = default) =>
        new(source, cancellationToken);

    /// <summary>
    /// Creates a new AST from source.
    /// </summary>
    /// <param name="source">The source to create the AST from.</param>
    /// <param name="cancellationToken">The cancellation token for the AST creation.</param>
    public static Ast Create(Source source, CancellationToken cancellationToken = default)
    {
        var ast = Parse(source, cancellationToken);
        
        cancellationToken.ThrowIfCancellationRequested();

        var symbolDiagnostics = SymbolResolution.ResolveSymbols(ast, cancellationToken);
        ast.diagnostics.AddRange(symbolDiagnostics);
        
        cancellationToken.ThrowIfCancellationRequested();

        var flowDiagnostics = FlowAnalyzer.Analyze(ast, cancellationToken);
        ast.diagnostics.AddRange(flowDiagnostics);
        
        ControlFlowMarker.Mark(ast, cancellationToken);

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

    /// <summary>
    /// Emits the AST as bytecode into a stream.
    /// </summary>
    /// <remarks>
    /// The AST must not contain any errors when this method is called (see <see cref="HasErrors"/>).
    /// The stream to emit into must be writable,
    /// and its length will be set to be exactly the length of the emitted bytecode.
    /// </remarks>
    /// <param name="stream">The stream to emit into.</param>
    public void Emit(Stream stream)
    {
        if (HasErrors) throw new InvalidOperationException("Cannot emit an AST which contains errors.");

        if (!stream.CanWrite) throw new ArgumentException("Emit stream has to be writable.", nameof(stream));
        
        Emitter.Emit(this, stream);
        
        // If the stream already had contents (such as the stream being that of a file which already existed
        // and is being overwritten), then in case the stream was longer than the bytes emitted into it,
        // this should ensure any remaining bytes are cleared away.
        stream.SetLength(stream.Position);
    }

    /// <summary>
    /// Gets the symbols accessible at a specified postition within the AST.
    /// </summary>
    /// <param name="position">The position to look up the symbols accessible at.</param>
    public IEnumerable<ISymbol> GetAccessibleSymbolsAt(int position)
    {
        // Start by just looking up the node at the position.
        var nodeAtPos = Root.FindNodeAt(position);

        // If we couldn't find a node at the position,
        // it must be either before or after the entire span of the root.
        if (nodeAtPos is null)
        {
            // Before the root?
            if (position < Root.Span.Start)
            {
                // Find the first child of the root.
                // If there is no child, look up symbols at the end of the root
                // (which there won't be any declared at, but there might be global symbols).
                // Otherwise, look up symbols at the child.
                var firstInRoot = Root.Children.FirstOrDefault();
                if (firstInRoot is null) Root.DeclaredScope.Value.AccessibleAt(null);
                return Root.DeclaredScope.Value.AccessibleAt(firstInRoot);
            }
            // After the root. Look up symbols at the very end of it.
            else return Root.DeclaredScope.Value.AccessibleAt(null);
        }

        // If the node is not a block, we don't have to do any kind of safety lookups
        // to ensure we're looking up stuff at the correct node.
        // We can just return the symbols accessible at the node.
        if (nodeAtPos is not BlockExpression block)
            return nodeAtPos.Scope.Value.AccessibleAt(nodeAtPos);
        
        // The node at the position is a block, probably in the middle of whitespace inside the block.
        // We iterate through its children to find the first one which span begins after the position
        // and look up the symbols there. If there is no node after the position,
        // we'll look up the symbols at the very end of the block.
        var lookupNode = block.Children.FirstOrDefault(child => child.Span.Start > position);
        return block.DeclaredScope.Value.AccessibleAt(lookupNode);
    }
}
