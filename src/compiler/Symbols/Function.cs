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
    /// The parameters to the function.
    /// </summary>
    IReadOnlyList<ParameterSymbol> Parameters { get; }
    
    /// <summary>
    /// Whether the function has an expression body or a block body.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ExpressionBody))]
    [MemberNotNullWhen(false, nameof(BlockBody))]
    bool HasExpressionBody { get; }
    
    /// <summary>
    /// The expression body of the function.
    /// </summary>
    Expression? ExpressionBody { get; }
    
    /// <summary>
    /// The block body of the function.
    /// </summary>
    BlockExpression? BlockBody { get; }

    /// <summary>
    /// The body of the function.
    /// This may be a non-block expression in the case of the body being an expression body,
    /// or a block expression in the case of the body being a block body or an expression body
    /// where the expression is a block expression.
    /// </summary>
    Expression Body => HasExpressionBody
        ? ExpressionBody
        : BlockBody;
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

    public bool HasExpressionBody => Declaration.ExpressionBody is not null;

    public Expression? ExpressionBody => Declaration.ExpressionBody;

    public BlockExpression? BlockBody => Declaration.BlockBody;
    
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

    bool IFunction.HasExpressionBody => true;

    Expression? IFunction.ExpressionBody => Body;

    BlockExpression? IFunction.BlockBody => null;

    /// <summary>
    /// The body of the lambda.
    /// </summary>
    public Expression Body => Declaration.Body;
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

    bool IFunction.HasExpressionBody => false;

    Expression? IFunction.ExpressionBody => null;

    BlockExpression? IFunction.BlockBody => Declaration;
}
