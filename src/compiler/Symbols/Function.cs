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
    /// The body expression of the function.
    /// </summary>
    Expression Body { get; }
    
    /// <summary>
    /// The parameter of the function.
    /// </summary>
    IReadOnlyList<ParameterSymbol> Parameters { get; }
    
    /// <summary>
    /// Gets the local variables declared by the function.
    /// </summary>
    IReadOnlyList<VariableSymbol> GetLocals();
}

/// <summary>
/// Represents a function declared by a function declaration.
/// </summary>
public sealed class NomialFunction : IDeclaredSymbol, IFunction
{
    private readonly List<ParameterSymbol> parameters = [];
    private IReadOnlyList<VariableSymbol>? locals = null;
    
    public required string Name { get; init; }

    /// <summary>
    /// The parameters of the function.
    /// </summary>
    public IReadOnlyList<ParameterSymbol> Parameters => parameters;
    
    /// <summary>
    /// The declaration of the function.
    /// </summary>
    public required FunctionDeclaration Declaration { get; init; }

    Node IDeclaredSymbol.Declaration => Declaration;

    Node IFunction.Declaration => Declaration;

    public Expression Body => Declaration.ExpressionBody ?? Declaration.BlockBody!;

    public IReadOnlyList<VariableSymbol> GetLocals()
    {
        locals ??= LocalsHelper.GetLocals(Body);
        return locals;
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
public sealed class Lambda(LambdaExpression expression) : IFunction
{
    private IReadOnlyList<VariableSymbol>? locals = null;
    
    /// <summary>
    /// The source lambda expression.
    /// </summary>
    public LambdaExpression Expression { get; } = expression;

    Node IFunction.Declaration => Expression;

    public Expression Body => Expression.Body;

    public IReadOnlyList<ParameterSymbol> Parameters { get; } =
        expression.Parameters.Select(x => x.Symbol.Value).ToList();

    public IReadOnlyList<VariableSymbol> GetLocals()
    {
        locals ??= LocalsHelper.GetLocals(Body);
        return locals;
    }
}

file static class LocalsHelper
{
    public static IReadOnlyList<VariableSymbol> GetLocals(Node node) =>
        node
            .Descendants(x =>
                x is not (LambdaExpression or Expression { Parent.Value: FunctionDeclaration }))
            .OfType<LetDeclaration>()
            .Select(x => x.Symbol.Value)
            .ToList();
}
