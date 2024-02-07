using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols;

/// <summary>
/// A semantic scope containing named symbols.
/// </summary>
public interface IScope
{
    /// <summary>
    /// The parent scope, or null if the scope is the global scope.
    /// </summary>
    IScope? Parent { get; }

    /// <summary>
    /// Looks up a symbol with a specified name at a specific point in the scope.
    /// </summary>
    /// <param name="name">The name of the symbol to look up.</param>
    /// <param name="at">The node to look up the symbol at.</param>
    /// <param name="predicate">
    /// A predicate which determines whether to return a given symbol with the specified name.
    /// </param>
    /// <returns>
    /// A result containing the symbol as well as its accessibility, or null if no symbol could not be found.
    /// </returns>
    LookupResult? LookupSymbol(string name, Node at, Func<ISymbol, bool>? predicate = null);

    /// <summary>
    /// Gets the declared symbols within the scope at a specific point.
    /// </summary>
    /// <param name="at">The node at which to find the declared symbols.</param>
    IEnumerable<LookupResult> DeclaredAt(Node at);

    /// <summary>
    /// Gets the accessible symbols at a specific point in the scope.
    /// </summary>
    /// <param name="at">The node at which to find the accessible symbols.</param>
    IEnumerable<LookupResult> AccessibleAt(Node at);
}

internal interface IMutableScope : IScope
{
    /// <inheritdoc cref="IScope.Parent"/>
    new IMutableScope? Parent { get; }

    /// <summary>
    /// Declares a symbol within the scope.
    /// </summary>
    /// <param name="symbol">The symbol to declare.</param>
    /// <returns>The result of the declaration.</returns>
    DeclarationResult Declare(IDeclaredSymbol symbol);
}

/// <summary>
/// The result of looking up a symbol in a scope.
/// </summary>
/// <param name="Symbol">The symbol which was found.</param>
/// <param name="Accessibility">The found symbol's accessibility.</param>
public readonly record struct LookupResult(
    ISymbol Symbol,
    SymbolAccessibility Accessibility);

public readonly record struct DeclarationResult(
    ISymbol? ConflictingSymbol);

/// <summary>
/// Defines how a symbol is accessible.
/// </summary>
public enum SymbolAccessibility
{
    /// <summary>
    /// The symbol is accessible as normal.
    /// </summary>
    Accessible,
    /// <summary>
    /// The symbol is accessible in a parent scope but access to it is blocked.
    /// </summary>
    Blocked,
}
