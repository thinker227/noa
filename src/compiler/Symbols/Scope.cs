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
    /// <param name="at">
    /// The node to look up the symbol at.
    /// If not specified, will look up symbols at the end of the scope.
    /// </param>
    /// <param name="predicate">
    /// A predicate which determines whether to return a given symbol with the specified name.
    /// </param>
    /// <returns>
    /// A result containing the symbol as well as its accessibility, or null if no symbol could not be found.
    /// </returns>
    LookupResult? LookupSymbol(string name, Node? at, Func<ISymbol, bool>? predicate = null);

    /// <summary>
    /// Gets the declared symbols within the scope at a specific point.
    /// </summary>
    /// <param name="at">
    /// The node at which to find the declared symbols.
    /// If not specified, will get the declared symbols at the end of the scope.
    /// </param>
    IEnumerable<IDeclaredSymbol> DeclaredAt(Node? at);

    /// <summary>
    /// Gets the accessible symbols at a specific point in the scope.
    /// </summary>
    /// <param name="at">
    /// The node at which to find the accessible symbols.
    /// If not specified, will get the accessible symbols at the end of the scope.
    /// </param>
    IEnumerable<LookupResult> AccessibleAt(Node? at);
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

/// <summary>
/// A scope for block-like nodes.
/// </summary>
/// <param name="parent">The parent scope, or null if the scope is the global scope.</param>
/// <param name="block">The block which declares the scope.</param>
/// <param name="functions">The functions declared in the scope.</param>
/// <param name="variableTimeline">A timeline of variables declared in the scope.</param>
/// <param name="timelineIndexMap">
/// A mapping between statements and their indices in the variable timeline.
/// </param>
internal sealed class BlockScope(
    IScope? parent,
    Node block,
    IReadOnlyDictionary<string, FunctionSymbol> functions,
    IReadOnlyList<ImmutableDictionary<string, VariableSymbol>> variableTimeline,
    IReadOnlyDictionary<Statement, int> timelineIndexMap)
    : IScope
{
    public IScope? Parent { get; } = parent;

    /// <summary>
    /// Tries to get the timeline index of a node.
    /// </summary>
    private bool TryGetTimelineIndex(
        Node? node,
        [NotNullWhen(true)] out Statement? statement,
        out int timelineIndex)
    {
        var stmt = node is not null
            // Find the closest statement.
            ? node as Statement ?? node.FindAncestor<Statement>()
            // If the node is null, try find the parent statement of the block itself.
            : block.FindAncestor<Statement>();

        if (stmt is null)
        {
            // If we can't find a statement, we can't really do anything useful.
            statement = null;
            timelineIndex = -1;
            return false;
        }

        statement = stmt;

        if (node is null)
        {
            // If the node is null, we're looking for the last step of the timeline.
            timelineIndex = variableTimeline.Count - 1;
            return true;
        }

        if (timelineIndexMap.TryGetValue(stmt, out timelineIndex)) return true;

        // If the statement doesn't belong to this block, check its ancestors to find one which does.
        return TryGetAncestorTimelineIndex(stmt, out statement, out timelineIndex);
    }

    private bool TryGetAncestorTimelineIndex(
        Node node,
        [NotNullWhen(true)] out Statement? statement,
        out int timelineIndex)
    {
        foreach (var ancestor in node.Ancestors())
        {
            if (ancestor is not Statement stmt) continue;
            
            statement = stmt;
            if (timelineIndexMap.TryGetValue(stmt, out timelineIndex)) return true;
        }

        statement = null;
        timelineIndex = -1;
        return false;
    }
    
    public LookupResult? LookupSymbol(string name, Node? at, Func<ISymbol, bool>? predicate = null)
    {
        // If there is a function with the name then we don't need to look up anything else.
        if (functions.TryGetValue(name, out var function) &&
            (predicate?.Invoke(function) ?? true))
        {
            return new(function, SymbolAccessibility.Accessible);
        }

        // If we can't find the timeline index for the node, we can't do anything.
        if (!TryGetTimelineIndex(at, out var statement, out var timelineIndex)) return null;

        var variables = variableTimeline[timelineIndex];
        if (variables.TryGetValue(name, out var variable) &&
            (predicate?.Invoke(variable) ?? true))
        {
            return new(variable, SymbolAccessibility.Accessible);
        }

        // We can't find the symbol in this scope.

        return Parent?.LookupSymbol(name, statement, predicate);
    }

    public IEnumerable<IDeclaredSymbol> DeclaredAt(Node? at)
    {
        if (!TryGetTimelineIndex(at, out _, out var timelineIndex)) return [];

        var variables = variableTimeline[timelineIndex];

        return functions.Values
            .Concat((IEnumerable<IDeclaredSymbol>)variables.Values);
    }

    public IEnumerable<LookupResult> AccessibleAt(Node? at)
    {
        if (!TryGetTimelineIndex(at, out var statement, out var timelineIndex)) return [];

        var variables = variableTimeline[timelineIndex];

        var declared = functions.Values
            .Concat((IEnumerable<IDeclaredSymbol>)variables.Values)
            .Select(s => new LookupResult(s, SymbolAccessibility.Accessible));

        var parentAccessible = Parent?.AccessibleAt(statement) ?? [];

        return declared.Concat(parentAccessible);
    }
}
