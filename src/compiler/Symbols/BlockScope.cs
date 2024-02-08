using System.Diagnostics.CodeAnalysis;
using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols;

/// <summary>
/// A scope for block-like nodes which declares variables in sequential order
/// while providing global access to functions.
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

        // Check if the symbol can be found in a parent scope.
        if (Parent?.LookupSymbol(name, statement, predicate) is { } parentLookup) return parentLookup;
        
        // We know at this point that the symbol is not accessible, but to provide better error reporting
        // we also check future points in the variable timeline to see if the symbol is accessible there.
        for (var i = timelineIndex + 1; i < variableTimeline.Count; i++)
        {
            var futureVariables = variableTimeline[i];

            if (futureVariables.TryGetValue(name, out var futureSymbol))
            {
                return new(futureSymbol, SymbolAccessibility.DeclaredLater);
            }
        }

        return null;
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
