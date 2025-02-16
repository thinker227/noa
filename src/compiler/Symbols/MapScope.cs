using System.Diagnostics;
using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols;

/// <summary>
/// A scope which contains a map of symbols.
/// </summary>
/// <param name="parent">The parent scope, or null if the scope is the global scope.</param>
/// <param name="declaration">The node which declares the scope.</param>
[DebuggerDisplay("{GetDebuggerDisplay()}")]
internal sealed class MapScope(IScope? parent, Node declaration) : IScope
{
    private readonly Dictionary<string, IDeclaredSymbol> symbols = new();
    
    public IScope? Parent { get; } = parent;

    // Lookup locations are irrelevant to map scopes because they're essentially just flat maps of symbols.
    
    public LookupResult? LookupSymbol(string name, LookupLocation location, Func<ISymbol, bool>? predicate = null) =>
        symbols.TryGetValue(name, out var symbol) && (predicate?.Invoke(symbol) ?? true)
            ? new(symbol, SymbolAccessibility.Accessible)
            : Parent?.LookupSymbol(name, LookupLocation.AtNode(declaration), predicate);
 
    public IEnumerable<IDeclaredSymbol> DeclaredAt(LookupLocation location) =>
        symbols.Values;

    public IEnumerable<ISymbol> AccessibleAt(LookupLocation location) =>
        DeclaredAt(location).Concat(Parent?.AccessibleAt(LookupLocation.AtNode(declaration)) ?? []);

    /// <summary>
    /// Declares a symbol within the scope.
    /// </summary>
    /// <param name="symbol">The symbol to declare.</param>
    /// <returns>The result of the declaration.</returns>
    public DeclarationResult Declare(IDeclaredSymbol symbol)
    {
        var name = symbol.Name;
        
        if (symbols.TryGetValue(name, out var conflicting))
        {
            return new(conflicting);
        }

        symbols[name] = symbol;
        return new(null);
    }

    private string GetDebuggerDisplay() =>
        $"Map scope {{ Symbols = {symbols.Count} }}";
}
