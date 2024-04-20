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
    /// The parameters to the function.
    /// </summary>
    IReadOnlyList<ParameterSymbol> Parameters { get; }
}

/// <summary>
/// Represents a function declared by a function declaration.
/// </summary>
public sealed class NomialFunction : IFunction, IDeclaredSymbol
{
    internal readonly List<ParameterSymbol> parameters = [];
    
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
    
    public override string ToString()
    {
        var parameters = string.Join(", ", Parameters.Select(p => p.Name));
        return $"{Name}({parameters}) declared at {Declaration.Location}";
    }
}

/// <summary>
/// A semantic representation of a lambda expression.
/// </summary>
public sealed class LambdaFunction : IFunction
{
    internal readonly List<ParameterSymbol> parameters = [];
    
    /// <summary>
    /// The declaration of the function.
    /// </summary>
    public required LambdaExpression Declaration { get; init; }

    Node IFunction.Declaration => Declaration;

    public IReadOnlyList<ParameterSymbol> Parameters => parameters;
}

/// <summary>
/// The top-level function of a program.
/// </summary>
public sealed class TopLevelFunction : IFunction
{
    /// <summary>
    /// The declaration of the function.
    /// </summary>
    public required Root Declaration { get; init; }

    Node IFunction.Declaration => Declaration;
    
    IReadOnlyList<ParameterSymbol> IFunction.Parameters { get; } = [];
}
