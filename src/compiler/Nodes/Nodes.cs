using System.Diagnostics.CodeAnalysis;
using Noa.Compiler.Symbols;
using Noa.Compiler.ControlFlow;
using TextMappingUtils;

namespace Noa.Compiler.Nodes;

/// <summary>
/// An abstract syntax node.
/// </summary>
public abstract class Node
{
    /// <summary>
    /// The AST the node belongs to.
    /// </summary>
    public required Ast Ast { get; init; }

    /// <summary>
    /// The parent of the node, or null if the node is the root node.
    /// </summary>
    public Semantic<Node> Parent => Ast.GetParent(this)!;
    
    /// <summary>
    /// The span of the node within the text.
    /// </summary>
    public required TextSpan Span { get; init; }

    /// <summary>
    /// The source location of the node.
    /// </summary>
    public Location Location => new(Ast.Source.Name, Span);
    
    /// <summary>
    /// The child nodes of the node.
    /// </summary>
    public abstract IEnumerable<Node> Children { get; }
    
    /// <summary>
    /// The semantic scope of the node.
    /// </summary>
    public Semantic<IScope> Scope { get; internal set; }

    /// <summary>
    /// Returns an empty collection if the passed in node is null,
    /// otherwise returns a singleton collection containing the node.
    /// </summary>
    protected IEnumerable<Node> EmptyIfNull(Node? node) =>
        node is not null
            ? [node]
            : [];
}

public partial class Block
{
    /// <summary>
    /// The scope <i>declared</i> by the scope,
    /// different from the scope the block is <i>in</i>.
    /// </summary>
    public Semantic<IScope> DeclaredScope { get; internal set; }
    
    /// <summary>
    /// The reachability of the very end of the block, past the last statement or the trailing expression.
    /// </summary>
    public Semantic<Reachability> TailReachability { get; internal set; }
}

public sealed partial class Root
{
    public Semantic<TopLevelFunction> Function { get; internal set; }
}

public abstract partial class Statement
{
    public Semantic<Reachability> Reachability { get; internal set; }
}

public sealed partial class Parameter
{
    public Semantic<ParameterSymbol> Symbol { get; internal set; }
}

public sealed partial class FunctionDeclaration
{
    public Semantic<NomialFunction> Symbol { get; internal set; }
}

public sealed partial class LetDeclaration
{

    public Semantic<VariableSymbol> Symbol { get; internal set; }
}

public abstract partial class Expression
{
    public Semantic<Reachability> Reachability { get; internal set; }
}

public sealed partial class LambdaExpression
{
    public Semantic<LambdaFunction> Function { get; internal set; }
}

public sealed partial class ReturnExpression
{
    public Semantic<IFunction?> Function { get; internal set; }
}

public sealed partial class BreakExpression
{
    public Semantic<LoopExpression?> Loop { get; internal set; }
}

public sealed partial class ContinueExpression
{
    public Semantic<LoopExpression?> Loop { get; internal set; }
}

public sealed partial class IdentifierExpression
{
    public Semantic<ISymbol> ReferencedSymbol { get; internal set; }
}

public enum AssignmentKind
{
    Assign,
    Plus,
    Minus,
    Mult,
    Div,
}

public enum UnaryKind
{
    Identity,
    Negate,
    Not,
}

public enum BinaryKind
{
    Plus,
    Minus,
    Mult,
    Div,
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
}
