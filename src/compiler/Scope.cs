using Noa.Compiler.Symbols;

namespace Noa.Compiler;

/// <summary>
/// A semantic scope containing named symbols.
/// </summary>
/// <param name="parent">The parent scope, or null if the scope is the global scope.</param>
public sealed class Scope(Scope? parent)
{
    private readonly Dictionary<string, ISymbol> symbols = new();
    
    /// <summary>
    /// The parent scope, or null if the scope is the global scope.
    /// </summary>
    public Scope? Parent { get; } = parent;

    /// <summary>
    /// The symbols declared in the scope.
    /// </summary>
    public IReadOnlyDictionary<string, ISymbol> DeclaredSymbols => symbols;

    /// <summary>
    /// The symbols accessible within the scope.
    /// </summary>
    public IEnumerable<ISymbol> AccessibleSymbols =>
        DeclaredSymbols.Values.Concat(Parent?.AccessibleSymbols ?? []);
    
    /// <summary>
    /// Looks up a symbol with a specified name in the scope.
    /// </summary>
    /// <param name="name">The name of the symbol to look up.</param>
    /// <param name="predicate">A predicate which determines whether to return
    /// a given symbol with the specified name.</param>
    /// <returns>The found symbol, or null if it could not be found.</returns>
    public ISymbol? LookupSymbol(string name, Func<ISymbol, bool>? predicate = null)
    {
        if (symbols.TryGetValue(name, out var symbol) && (predicate?.Invoke(symbol) ?? true))
            return symbol;

        return Parent?.LookupSymbol(name, predicate);
    }

    /// <summary>
    /// Declares a symbol in the scope.
    /// </summary>
    /// <param name="symbol">The symbol to declare.</param>
    internal void Declare(ISymbol symbol) =>
        symbols[symbol.Name] = symbol;
}
