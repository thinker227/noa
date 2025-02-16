using System.Diagnostics;
using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols;

/// <summary>
/// A scope which blocks access to variables and parameters from its parent scope.
/// </summary>
[DebuggerDisplay("Blocking scope")]
internal sealed class BlockingScope(IScope parent, Node declaration) : IScope
{
    public IScope Parent { get; } = parent;

    // Blocking scopes do not declare symbols of their own, they only forward symbols from their parent scope.
    
    public LookupResult? LookupSymbol(string name, LookupLocation location, Func<ISymbol, bool>? predicate = null)
    {
        if (Parent.LookupSymbol(name, LookupLocation.AtNode(declaration), predicate) is not { } lookup) return null;

        return lookup.Symbol switch
        {
            VariableSymbol or ParameterSymbol => lookup with { Accessibility = SymbolAccessibility.Blocked },
            _ => lookup
        };
    }
    
    public IEnumerable<IDeclaredSymbol> DeclaredAt(LookupLocation location) => [];

    public IEnumerable<ISymbol> AccessibleAt(LookupLocation location) =>
        Parent.AccessibleAt(LookupLocation.AtNode(declaration));
}
