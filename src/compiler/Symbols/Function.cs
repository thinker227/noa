using System.Diagnostics.CodeAnalysis;
using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols;

/// <summary>
/// A semantic representation of a function.
/// </summary>
public interface IFunction
{
    /// <summary>
    /// The declaration of the function.
    /// </summary>
    Node Declaration { get; }
    
    /// <summary>
    /// The body of the function.
    /// </summary>
    FunctionBody Body { get; }
    
    /// <summary>
    /// The parameter of the function.
    /// </summary>
    IReadOnlyList<ParameterSymbol> Parameters { get; }
}

/// <summary>
/// Represents a function declared by a function declaration.
/// </summary>
public sealed class NomialFunction : IDeclaredSymbol, IFunction
{
    private readonly List<ParameterSymbol> parameters = [];

    public string Name { get; }

    /// <summary>
    /// The declaration of the function.
    /// </summary>
    public FunctionDeclaration Declaration { get; }

    Node IDeclaredSymbol.Declaration => Declaration;

    Node IFunction.Declaration => Declaration;

    /// <summary>
    /// Whether the function has an expression body or block body.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ExpressionBody))]
    [MemberNotNullWhen(false, nameof(BlockBody))]
    public bool HasExpressionBody => Declaration.ExpressionBody is not null;

    /// <summary>
    /// The expression body of the function.
    /// </summary>
    public Expression? ExpressionBody => Declaration.ExpressionBody;

    /// <summary>
    /// The block body of the function.
    /// </summary>
    public BlockExpression? BlockBody => Declaration.BlockBody;

    /// <summary>
    /// The body of the function.
    /// </summary>
    public Expression BodyExpression => HasExpressionBody
        ? ExpressionBody
        : BlockBody;

    public FunctionBody Body { get; }

    /// <summary>
    /// The parameters of the function.
    /// </summary>
    public IReadOnlyList<ParameterSymbol> Parameters => parameters;

    public NomialFunction(string name, FunctionDeclaration declaration)
    {
        Name = name;
        Declaration = declaration;

        if (declaration.ExpressionBody is not null)
        {
            var body = declaration.ExpressionBody;
            var implicitReturn = (body, ReturnKind.Implicit);
            
            Body = new FunctionBody(
                body,
                [],
                FunctionBody.GetReturnExpressions(body)
                    .Select(x => ((Expression)x, ReturnKind.Explicit))
                    .Prepend(implicitReturn),
                FunctionBody.GetLocals(body));
        }
        else
        {
            var body = declaration.BlockBody!;
            IEnumerable<(Expression, ReturnKind)> trailingReturn = body.TrailingExpression is not null
                ? [(body.TrailingExpression, ReturnKind.Implicit)]
                : []; 
                
            Body = new FunctionBody(
                body,
                body.Statements,
                FunctionBody.GetReturnExpressions(body)
                    .Select(x => ((Expression)x, ReturnKind.Explicit))
                    .Concat(trailingReturn),
                FunctionBody.GetLocals(body));
        }
    }
    
    /// <summary>
    /// Adds a parameter to the function.
    /// </summary>
    /// <param name="parameter">The parameter to add.</param>
    internal void AddParameter(ParameterSymbol parameter) =>
        parameters.Add(parameter);
    
    public override string ToString()
    {
        var parameters = string.Join(", ", Parameters.Select(p => p.Name));
        return $"{Name}({parameters}) declared at {Declaration.Location}";
    }
}

/// <summary>
/// A semantic representation of a lambda expression.
/// </summary>
/// <param name="expression">The source lambda expression.</param>
public sealed class LambdaFunction(LambdaExpression expression) : IFunction
{
    private IReadOnlyList<VariableSymbol>? locals = null;
    
    /// <summary>
    /// The source lambda expression.
    /// </summary>
    public LambdaExpression Expression { get; } = expression;

    Node IFunction.Declaration => Expression;

    /// <summary>
    /// The body of the function.
    /// </summary>
    public Expression BodyExpression => Expression.Body;

    public FunctionBody Body { get; } = new(
        expression.Body,
        [],
        FunctionBody.GetReturnExpressions(expression.Body)
            .Select(x => ((Expression)x, ReturnKind.Explicit)),
        FunctionBody.GetLocals(expression.Body));

    public IReadOnlyList<ParameterSymbol> Parameters { get; } =
        expression.Parameters.Select(x => x.Symbol.Value).ToList();
}

/// <summary>
/// A semantic representation of a function body.
/// </summary>
public sealed class FunctionBody(
    Node node,
    ImmutableArray<Statement> statements,
    IEnumerable<(Expression, ReturnKind)> returns,
    IEnumerable<VariableSymbol> locals)
{
    /// <summary>
    /// The node of the block.
    /// </summary>
    public Node Node { get; } = node;

    /// <summary>
    /// The scope of the body.
    /// </summary>
    public Semantic<IScope> Scope { get; internal set; }

    /// <summary>
    /// The statements directly inside the body.
    /// </summary>
    public ImmutableArray<Statement> Statements { get; } = statements;

    /// <summary>
    /// The return expressions within the function.
    /// </summary>
    public ImmutableArray<(Expression expression, ReturnKind kind)> Returns { get; } = returns.ToImmutableArray();

    /// <summary>
    /// The local variables declared by the function.
    /// </summary>
    public ImmutableArray<VariableSymbol> Locals { get; } = locals.ToImmutableArray();

    private static IEnumerable<Node> GetNodesWithinFunction(Node node) =>
        node.Descendants(x =>
            x is not (LambdaExpression or Expression { Parent.Value: FunctionDeclaration }));

    internal static IEnumerable<VariableSymbol> GetLocals(Node node) =>
        GetNodesWithinFunction(node)
            .OfType<LetDeclaration>()
            .Select(x => x.Symbol.Value);

    internal static IEnumerable<ReturnExpression> GetReturnExpressions(Node node) =>
        GetNodesWithinFunction(node)
            .OfType<ReturnExpression>();
}

public enum ReturnKind
{
    Explicit,
    Implicit,
}
