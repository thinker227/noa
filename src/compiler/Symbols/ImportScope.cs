using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols;

/// <summary>
/// A scope which contains a map of imported symbols.
/// </summary>
/// <remarks>
/// Import scopes are always the root scope, so they don't have a parent.
/// </remarks>
public sealed class ImportScope : IScope
{
    private readonly Dictionary<string, ISymbol> symbols = new();
    
    IScope? IScope.Parent => null;

    /// <summary>
    /// Declares a symbol in the scope.
    /// </summary>
    /// <param name="symbol">The symbol to declare.</param>
    public void Declare(ISymbol symbol)
    {
        if (!symbols.TryAdd(symbol.Name, symbol))
            throw new InvalidOperationException($"Cannot import symbol {symbol} because another symbol with " +
                                                "the same name has already been imported into the scope.");
    }
    
    public LookupResult? LookupSymbol(string name, Node? at, Func<ISymbol, bool>? predicate = null) =>
        symbols.TryGetValue(name, out var symbol) && (predicate?.Invoke(symbol) ?? true)
            ? new(symbol, SymbolAccessibility.Accessible)
            : null;

    public IEnumerable<ISymbol> AccessibleAt(Node? at) => symbols.Values;

    // An import scope doesn't declare any symbols because all of its symbols are externally defined.
    IEnumerable<IDeclaredSymbol> IScope.DeclaredAt(Node? at) => [];
}
