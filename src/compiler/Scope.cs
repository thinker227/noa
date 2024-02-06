using Noa.Compiler.Symbols;

namespace Noa.Compiler;

/// <summary>
/// A semantic scope containing named symbols.
/// </summary>
/// <param name="parent">The parent scope, or null if the scope is the global scope.</param>
public class Scope(Scope? parent)
{
    private readonly Dictionary<string, ISymbol> symbols = new();
    
    /// <summary>
    /// The parent scope, or null if the scope is the global scope.
    /// </summary>
    public Scope? Parent { get; } = parent;

    /// <summary>
    /// The symbols declared in the scope.
    /// These symbols are always completely accessible.
    /// </summary>
    public IReadOnlyDictionary<string, ISymbol> DeclaredSymbols => symbols;

    /// <summary>
    /// The symbols accessible within the scope, as well as their accessibility.
    /// </summary>
    public virtual IEnumerable<(ISymbol, SymbolAccessibility)> AccessibleSymbols =>
        DeclaredSymbols.Values
            .Select(s => (s, SymbolAccessibility.Accessible))
            .Concat(Parent?.AccessibleSymbols ?? []);
    
    /// <summary>
    /// Looks up a symbol with a specified name in the scope.
    /// </summary>
    /// <param name="name">The name of the symbol to look up.</param>
    /// <param name="predicate">
    /// A predicate which determines whether to return a given symbol with the specified name.
    /// </param>
    /// <returns>The found symbol as well as its accessibility, or null if no symbol could not be found.</returns>
    public virtual (ISymbol, SymbolAccessibility)? LookupSymbol(string name, Func<ISymbol, bool>? predicate = null)
    {
        if (symbols.TryGetValue(name, out var symbol) &&
            (predicate?.Invoke(symbol) ?? true))
            return (symbol, SymbolAccessibility.Accessible);

        return Parent?.LookupSymbol(name, predicate);
    }

    /// <summary>
    /// Declares a symbol in the scope.
    /// </summary>
    /// <param name="symbol">The symbol to declare.</param>
    internal void Declare(ISymbol symbol) =>
        symbols[symbol.Name] = symbol;
}

/// <summary>
/// A scope which returns <see cref="SymbolAccessibility.Blocked"/> for symbols in its parent scope.
/// </summary>
/// <param name="parent">The parent scope, or null if the scope is the global scope.</param>
public sealed class BlockingScope(Scope? parent) : Scope(parent)
{
    public override IEnumerable<(ISymbol, SymbolAccessibility)> AccessibleSymbols =>
        DeclaredSymbols.Values
            .Select(s => (s, SymbolAccessibility.Accessible))
            .Concat(Parent?.AccessibleSymbols
                .Select(s => (s.Item1, SymbolAccessibility.Blocked))
                ?? []);

    public override (ISymbol, SymbolAccessibility)? LookupSymbol(string name, Func<ISymbol, bool>? predicate = null)
    {
        if (DeclaredSymbols.TryGetValue(name, out var symbol) &&
            (predicate?.Invoke(symbol) ?? true))
            return (symbol, SymbolAccessibility.Accessible);

        if (Parent?.LookupSymbol(name, predicate) is not var (parentSymbol, _)) return null;
        return (parentSymbol, SymbolAccessibility.Blocked);
    }
}

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
