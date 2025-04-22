// ReSharper disable LocalVariableHidesMember

using Noa.Compiler.ControlFlow;
using Noa.Compiler.Diagnostics;
using Noa.Compiler.Emit;
using Noa.Compiler.FlowAnalysis;
using Noa.Compiler.Nodes;
using Noa.Compiler.Parsing;
using Noa.Compiler.Symbols;
using Noa.Compiler.Syntax;

namespace Noa.Compiler;

/// <summary>
/// A representation of a single source file.
/// </summary>
public sealed class Ast
{
    private readonly List<IDiagnostic> diagnostics;
    private IReadOnlyDictionary<Node, Node>? parents = null;
    private IReadOnlyDictionary<SyntaxNode, Node>? astNodes = null;
    
    /// <summary>
    /// The source for the AST.
    /// </summary>
    public Source Source { get; }

    /// <summary>
    /// The root of the AST.
    /// </summary>
    public Root Root { get; }

    /// <summary>
    /// The root of the syntax tree.
    /// </summary>
    public RootSyntax SyntaxRoot => (RootSyntax)Root.Syntax;

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

    private Ast(Source source, RootSyntax root, CancellationToken cancellationToken)
    {
        diagnostics = [];
        Source = source;
        Root = new IntoAst(this).FromRoot(root);
    }

    /// <summary>
    /// Creates a new AST from source without running any additional semantic passes after parsing.
    /// </summary>
    /// <param name="source">The source to create the AST from.</param>
    internal static Ast Parse(Source source)
    {
        var greenRoot = Parser.Parse(source, default);
        var redRoot = (RootSyntax)greenRoot.ToRed(0, null!);

        return new Ast(source, redRoot, default);
    }

    /// <summary>
    /// Creates a new AST from source.
    /// </summary>
    /// <param name="source">The source to create the AST from.</param>
    /// <param name="cancellationToken">The cancellation token for the AST creation.</param>
    public static Ast Create(Source source, CancellationToken cancellationToken = default)
    {
        var greenRoot = Parser.Parse(source, cancellationToken);
        var redRoot = (RootSyntax)greenRoot.ToRed(0, null!);

        var ast = new Ast(source, redRoot, cancellationToken);

        var parseDiagnostics = CollectParseDiagnostics(source, redRoot);
        ast.diagnostics.AddRange(parseDiagnostics);
        
        cancellationToken.ThrowIfCancellationRequested();

        var symbolDiagnostics = SymbolResolution.ResolveSymbols(ast, cancellationToken);
        ast.diagnostics.AddRange(symbolDiagnostics);
        
        cancellationToken.ThrowIfCancellationRequested();

        var flowDiagnostics = FlowAnalyzer.Analyze(ast, cancellationToken);
        ast.diagnostics.AddRange(flowDiagnostics);
        
        ControlFlowMarker.Mark(ast, cancellationToken);

        return ast;
    }

    private static IEnumerable<IDiagnostic> CollectParseDiagnostics(Source source, SyntaxNode node)
    {
        var diags = node.GetDiagnostics(source);
        var childDiags = node.Children.SelectMany(child => CollectParseDiagnostics(source, child));
        return diags.Concat(childDiags);
    }

    /// <summary>
    /// Gets the parent of a node, or null if the node is the root node.
    /// </summary>
    /// <param name="node">The node to get the parent of.</param>
    internal Semantic<Node?> GetParent(Node node)
    {
        if (Root is null) return new();

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

    /// <summary>
    /// Gets the AST node corresponding to a syntax node.
    /// </summary>
    /// <param name="syntaxNode">The syntax node to get the corresponding AST node of.</param>
    /// <remarks>
    /// Note that the AST node corresponding to a syntax node is not always a 1:1 relation.
    /// For instance, <see cref="SyntaxList{T}"/> and <see cref="SeparatedSyntaxList{T}"/>
    /// have no corresponding AST representation, so they always fall back to looking up the
    /// corresponding AST node in their <i>parents</i>.
    /// </remarks>
    public Node GetAstNode(SyntaxNode syntaxNode)
    {
        astNodes ??= ComputeAstNodes(Root);

        if (astNodes.TryGetValue(syntaxNode, out var node)) return node;

        if (syntaxNode.Parent is null) throw new InvalidOperationException(
            "Attempted get AST node corresponding to a syntax node which has no parent " +
            "and no corresponding AST node by itself.");
        
        return GetAstNode(syntaxNode.Parent);
    }

    private static Dictionary<SyntaxNode, Node> ComputeAstNodes(Node root)
    {
        var parents = new Dictionary<SyntaxNode, Node>();
        Visit(root);
        return parents;

        void Visit(Node node)
        {
            parents[node.Syntax] = node;

            foreach (var child in node.Children)
                Visit(child);
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
}
