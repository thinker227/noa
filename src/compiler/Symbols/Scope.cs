using System.Diagnostics.CodeAnalysis;
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
    /// <param name="location">
    /// The location to look up the symbol at.
    /// </param>
    /// <param name="predicate">
    /// A predicate which determines whether to return a given symbol with the specified name.
    /// </param>
    /// <returns>
    /// A result containing the symbol as well as its accessibility, or null if no symbol could not be found.
    /// </returns>
    LookupResult? LookupSymbol(string name, LookupLocation location, Func<ISymbol, bool>? predicate = null);

    /// <summary>
    /// Gets the declared symbols within the scope at a specific point.
    /// </summary>
    /// <param name="location">
    /// The location at which to find the declared symbols.
    /// </param>
    IEnumerable<IDeclaredSymbol> DeclaredAt(LookupLocation location);

    /// <summary>
    /// Gets all accessible symbols at a specific point in the scope.
    /// </summary>
    /// <param name="location">
    /// The location at which to find the declared symbols.
    /// </param>
    IEnumerable<ISymbol> AccessibleAt(LookupLocation location);
}

/// <summary>
/// A location to look up a symbol at.
/// </summary>
public readonly struct LookupLocation
{
    /// <summary>
    /// The node the lookup is at.
    /// </summary>
    public Node? Node { get; }

    /// <summary>
    /// Whether the lookup is at the end of its scope.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Node))]
    public bool IsAtEnd => Node is null;

    private LookupLocation(Node? node) => Node = node;

    /// <summary>
    /// Creates a new lookup location at a specified node.
    /// </summary>
    /// <param name="node">The node to perform the lookup at.</param>
    public static LookupLocation AtNode(Node node) => new(node);

    /// <summary>
    /// Creates a new lookup location at the end of a scope.
    /// </summary>
    public static LookupLocation AtEnd() => new();
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
    /// <summary>
    /// The symbol is declared later in the scope and is only accessible thereafter.
    /// </summary>
    DeclaredLater,
}
