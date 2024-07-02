// ReSharper disable LocalVariableHidesMember

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
/// A semantic element which is nested within a function.
/// </summary>
public interface IFunctionNested
{
    /// <summary>
    /// The function which contains the element.
    /// </summary>
    IFunction ContainingFunction { get; }
}

/// <summary>
/// A symbol which is declared in source and has a corresponding AST node.
/// </summary>
public interface IDeclaredSymbol : ISymbol, IFunctionNested
{
    /// <summary>
    /// The declaring node of the symbol.
    /// </summary>
    Node Declaration { get; }
    
    /// <summary>
    /// The definition location for the symbol.
    /// This differs from the <see cref="Node.Location"/> of <see cref="Declaration"/>
    /// since the definition location may be, for instance, the identifier of a let declaration
    /// and not the let declaration itself.
    /// </summary>
    Location DefinitionLocation { get; }
}

/// <summary>
/// Represents a variable-like symbol.
/// </summary>
public interface IVariableSymbol : ISymbol, IFunctionNested
{
    /// <summary>
    /// Whether the variable is declared as mutable.
    /// </summary>
    bool IsMutable { get; }
}

/// <summary>
/// Represents a variable declared by a let declaration.
/// </summary>
public sealed class VariableSymbol : IVariableSymbol, IDeclaredSymbol
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
    
    public required IFunction ContainingFunction { get; init; }
    
    Node IDeclaredSymbol.Declaration => Declaration;

    public Location DefinitionLocation => Declaration.Identifier.Location;

    public override string ToString() => $"Variable {Name} declared at {Declaration.Location}";
}

/// <summary>
/// Represents a parameter declared by a function expression.
/// </summary>
public sealed class ParameterSymbol : IVariableSymbol, IDeclaredSymbol
{
    public required string Name { get; init; }

    /// <summary>
    /// Whether the parameter is declared as mutable.
    /// </summary>
    public bool IsMutable => Declaration.IsMutable;
    
    /// <summary>
    /// The function symbol which the parameter belongs to.
    /// </summary>
    public required IFunction Function { get; init; }
    
    /// <summary>
    /// The index of the parameter in its function.
    /// </summary>
    public required int ParameterIndex { get; init; }
    
    /// <summary>
    /// The declaration of the parameter.
    /// </summary>
    public required Parameter Declaration { get; init; }

    public Location DefinitionLocation => Declaration.Location;

    IFunction IFunctionNested.ContainingFunction => Function;
    
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
