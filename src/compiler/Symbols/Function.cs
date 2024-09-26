using System.Diagnostics.CodeAnalysis;
using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols;

/// <summary>
/// A semantic representation of a function.
/// </summary>
public interface IFunction
{
    /// <summary>
    /// The parameters to the function.
    /// </summary>
    IReadOnlyList<IParameterSymbol> Parameters { get; }
}

/// <summary>
/// A semantic representation of a function declared in source.
/// </summary>
public interface IDeclaredFunction : IFunction, IDeclared
{
    /// <summary>
    /// The parameters to the function.
    /// </summary>
    new IReadOnlyList<ParameterSymbol> Parameters { get; }

    IReadOnlyList<IParameterSymbol> IFunction.Parameters => Parameters;
    
    /// <summary>
    /// The scope of the body of the function.
    /// </summary>
    IScope BodyScope { get; }
    
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

    /// <summary>
    /// Gets a sequence of every local variable declared in the function.
    /// </summary>
    /// <remarks>
    /// Implementations of this method implement caching,
    /// so calculations will only run the first time the method is called.
    /// </remarks>
    IReadOnlyCollection<VariableSymbol> GetLocals();
}

/// <summary>
/// Represents a function declared by a function declaration.
/// </summary>
public sealed class NomialFunction : IDeclaredFunction, IDeclaredSymbol
{
    internal readonly List<ParameterSymbol> parameters = [];
    private IReadOnlyCollection<VariableSymbol>? locals = null;
    
    public required string Name { get; init; }

    /// <summary>
    /// The parameters of the function.
    /// </summary>
    public IReadOnlyList<ParameterSymbol> Parameters => parameters;
    
    /// <summary>
    /// The declaration of the function.
    /// </summary>
    public required FunctionDeclaration Declaration { get; init; }

    Node IDeclared.Declaration => Declaration;

    Location IDeclared.DefinitionLocation => Declaration.Identifier.Location;

    public IScope BodyScope => HasExpressionBody
        ? ExpressionBody.Scope.Value
        : BlockBody.DeclaredScope.Value;

    [MemberNotNullWhen(true, nameof(ExpressionBody))]
    [MemberNotNullWhen(false, nameof(BlockBody))]
    public bool HasExpressionBody => Declaration.ExpressionBody is not null;

    public Expression? ExpressionBody => Declaration.ExpressionBody;

    public BlockExpression? BlockBody => Declaration.BlockBody;
    
    public Expression Body => HasExpressionBody
        ? ExpressionBody
        : BlockBody;
    
    public required IFunction ContainingFunction { get; init; }

    public IReadOnlyCollection<VariableSymbol> GetLocals()
    {
        locals ??= FunctionUtility.GetLocals(Body);
        return locals;
    }

    public override string ToString()
    {
        var parameters = string.Join(", ", Parameters.Select(p => p.Name));
        return $"{Name}({parameters}) declared at {Declaration.Location}";
    }
}

/// <summary>
/// A semantic representation of a lambda expression.
/// </summary>
public sealed class LambdaFunction : IDeclaredFunction, IFunctionNested
{
    internal readonly List<ParameterSymbol> parameters = [];
    private IReadOnlyCollection<VariableSymbol>? locals = null;
    private IReadOnlyCollection<IVariableSymbol>? captures = null;
    
    /// <summary>
    /// The declaration of the function.
    /// </summary>
    public required LambdaExpression Declaration { get; init; }

    Node IDeclared.Declaration => Declaration;

    Location IDeclared.DefinitionLocation => Declaration.Location with { Span = Declaration.ArrowToken.Span };

    public IReadOnlyList<ParameterSymbol> Parameters => parameters;

    public IScope BodyScope => Body.Scope.Value;

    bool IDeclaredFunction.HasExpressionBody => true;

    Expression? IDeclaredFunction.ExpressionBody => Body;

    BlockExpression? IDeclaredFunction.BlockBody => null;

    /// <summary>
    /// The body of the lambda.
    /// </summary>
    public Expression Body => Declaration.Body;
    
    /// <summary>
    /// The function which contains the lambda.
    /// </summary>
    public required IFunction ContainingFunction { get; init; }

    public IReadOnlyCollection<VariableSymbol> GetLocals()
    {
        locals = FunctionUtility.GetLocals(Body);
        return locals;
    }

    /// <summary>
    /// Gets all variables and parameters captured by the lambda.
    /// </summary>
    public IReadOnlyCollection<IVariableSymbol> GetCaptures()
    {
        if (captures is not null) return captures;
        
        var identifierExpressions = Body
            .DescendantNodesAndSelfInFunction()
            .OfType<IdentifierExpression>();
        
        var referencedSymbols = identifierExpressions
            .Select(x => x.ReferencedSymbol.Value);

        // Logically, referenced symbols which are not from the current lambda function
        // have to have been declared in some containing function, meaning it's been captured.
        captures = referencedSymbols
            .OfType<IVariableSymbol>()
            .Where(x => !x.ContainingFunction.Equals(this))
            .Distinct()
            .ToList();

        return captures;
    }
}

/// <summary>
/// The top-level function of a program.
/// </summary>
public sealed class TopLevelFunction : IDeclaredFunction
{
    private IReadOnlyCollection<VariableSymbol>? locals = null;
    
    /// <summary>
    /// The declaration of the function.
    /// </summary>
    public required Root Declaration { get; init; }

    Node IDeclared.Declaration => Declaration;

    Location IDeclared.DefinitionLocation => Declaration.Location;

    IReadOnlyList<ParameterSymbol> IDeclaredFunction.Parameters { get; } = [];

    public IScope BodyScope => Declaration.DeclaredScope.Value;

    bool IDeclaredFunction.HasExpressionBody => false;

    Expression? IDeclaredFunction.ExpressionBody => null;

    BlockExpression? IDeclaredFunction.BlockBody => Declaration;

    public IReadOnlyCollection<VariableSymbol> GetLocals()
    {
        locals ??= FunctionUtility.GetLocals(Declaration);
        return locals;
    }
}

public static class FunctionExtensions
{
    /// <summary>
    /// Gets the full name of a function.
    /// </summary>
    /// <param name="function">The function to get the name of.</param>
    public static string GetFullName(this IFunction function)
    {
        var upfrontName = UpfrontName(function);
        
        if (function is TopLevelFunction) return upfrontName;

        var containingFunction = ((IFunctionNested)function).ContainingFunction;

        if (containingFunction is TopLevelFunction) return upfrontName;

        var nestedName = containingFunction.GetFullName();
        
        return $"{nestedName} -> {upfrontName}";
    }

    private static string UpfrontName(IFunction function) => function switch
    {
        TopLevelFunction => "<main>",
        NomialFunction nomial => nomial.Name,
        LambdaFunction => "<lambda>",
        _ => throw new UnreachableException()
    };
}

file static class FunctionUtility
{
    public static IReadOnlyCollection<VariableSymbol> GetLocals(Node node) => node
        .DescendantNodesAndSelfInFunction()
        .OfType<LetDeclaration>()
        .Select(x => x.Symbol.Value)
        .ToList();
}
