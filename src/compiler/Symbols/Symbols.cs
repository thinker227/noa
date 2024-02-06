using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols;

/// <summary>
/// A named semantic element of code.
/// </summary>
public interface ISymbol
{
    /// <summary>
    /// The name of the symbol.
    /// </summary>
    string Name { get; }
}

/// <summary>
/// A symbol which is declared in source and has a corresponding AST node.
/// </summary>
public interface IDeclaredSymbol : ISymbol
{
    /// <summary>
    /// The declaring node of the symbol.
    /// </summary>
    Node Declaration { get; }
}

/// <summary>
/// Represents a function declared by a function declaration.
/// </summary>
public sealed class FunctionSymbol : IDeclaredSymbol
{
    public required string Name { get; init; }
    
    /// <summary>
    /// The parameters of the function.
    /// </summary>
    public required IReadOnlyList<ParameterSymbol> Parameters { get; init; }
    
    /// <summary>
    /// The declaration of the function.
    /// </summary>
    public required FunctionDeclaration Declaration { get; init; }

    Node IDeclaredSymbol.Declaration => Declaration;
    
    public override string ToString()
    {
        var parameters = string.Join(", ", Parameters.Select(p => p.Name));
        return $"{Name}({parameters}) declared at {Declaration.Location}";
    }
}

/// <summary>
/// Represents a variable declared by a let declaration.
/// </summary>
public sealed class VariableSymbol : IDeclaredSymbol
{
    public required string Name { get; init; }

    /// <summary>
    /// Whether the variable is declared as mutable.
    /// </summary>
    public bool IsMutable => Declaration.IsMutable;
    
    /// <summary>
    /// The declaration of the variable.
    /// </summary>
    public required LetDeclaration Declaration { get; init; }
    
    Node IDeclaredSymbol.Declaration => Declaration;

    public override string ToString() => $"Variable {Name} declared at {Declaration.Location}";
}

/// <summary>
/// Represents a parameter declared by a function expression.
/// </summary>
public sealed class ParameterSymbol : IDeclaredSymbol
{
    public required string Name { get; init; }

    /// <summary>
    /// Whether the parameter is declared as mutable.
    /// </summary>
    public bool IsMutable => Declaration.IsMutable;
    
    /// <summary>
    /// The function symbol which the parameter belongs to,
    /// or null if the parameter belongs to a lambda expression.
    /// </summary>
    public FunctionSymbol? Function { get; init; }
    
    /// <summary>
    /// The declaration of the parameter.
    /// </summary>
    public required Parameter Declaration { get; init; }
    
    Node IDeclaredSymbol.Declaration => Declaration;
    
    public override string ToString() => $"Parameter {Name} declared at {Declaration.Location}";
}

/// <summary>
/// Represents a symbol which is the result of some kind of error.
/// </summary>
public sealed class ErrorSymbol : ISymbol
{
    public string Name => "<error>";

    public override string ToString() => "Error symbol";
}
