using System.Diagnostics;
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
[DebuggerDisplay("{GetDebuggerDisplay()}")]
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
    /// The functions declared in the scope.
    /// </summary>
    public IReadOnlyDictionary<string, FunctionSymbol> Functions { get; } = functions;

    /// <summary>
    /// A timeline of variables declared in the scope.
    /// </summary>
    public IReadOnlyList<ImmutableDictionary<string, VariableSymbol>> VariableTimeline { get; } = variableTimeline;

    /// <summary>
    /// Tries to get the timeline index of a node.
    /// </summary>
    private bool TryGetTimelineIndex(
        Node? node,
        [NotNullWhen(true)] out Node? parentLookupNode,
        out int timelineIndex)
    {
        if (node is null)
        {
            // If the node we're trying to get the timeline index for is null,
            // that means we want to get the timeline index and parent lookup node for the very end of the scope.
            parentLookupNode = block;
            timelineIndex = VariableTimeline.Count - 1;
            return true;
        }
        
        var statement = node as Statement ?? node.FindAncestor<Statement>();

        if (statement is null)
        {
            // If we can't find a statement, we can't really do anything useful.
            parentLookupNode = null;
            timelineIndex = -1;
            return false;
        }

        if (timelineIndexMap.TryGetValue(statement, out timelineIndex))
        {
            // The statement exists in this scope.
            parentLookupNode = statement;
            return true;
        }

        // If the statement doesn't belong to this block, check its ancestors to find one which does.
        if (TryGetAncestorTimelineIndex(statement, out var parentStatement, out timelineIndex))
        {
            parentLookupNode = parentStatement;
            return true;
        }

        parentLookupNode = null;
        timelineIndex = -1;
        return false;
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
        if (Functions.TryGetValue(name, out var function) &&
            (predicate?.Invoke(function) ?? true))
        {
            return new(function, SymbolAccessibility.Accessible);
        }

        if (!TryGetTimelineIndex(at, out var parentLookupNode, out var timelineIndex))
        {
            // If we can't find the timeline index for the node we're looking up at, we're heading to
            // the parent scope to check if it has a symbol available at the location of this block.
            return Parent?.LookupSymbol(name, block, predicate);
        }

        var variables = VariableTimeline[timelineIndex];
        if (variables.TryGetValue(name, out var variable) &&
            (predicate?.Invoke(variable) ?? true))
        {
            return new(variable, SymbolAccessibility.Accessible);
        }

        // We can't find the symbol in this scope.

        // Check if the symbol can be found in a parent scope.
        if (Parent?.LookupSymbol(name, parentLookupNode, predicate) is { } parentLookup) return parentLookup;
        
        // We know at this point that the symbol is not accessible, but to provide better error reporting
        // we also check future points in the variable timeline to see if the symbol is accessible there.
        for (var i = timelineIndex + 1; i < VariableTimeline.Count; i++)
        {
            var futureVariables = VariableTimeline[i];

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

        var variables = VariableTimeline[timelineIndex];

        return Functions.Values
            .Concat((IEnumerable<IDeclaredSymbol>)variables.Values);
    }

    public IEnumerable<ISymbol> AccessibleAt(Node? at)
    {
        if (!TryGetTimelineIndex(at, out var statement, out var timelineIndex)) return [];

        var variables = VariableTimeline[timelineIndex];

        var declared = Functions.Values
            .Concat((IEnumerable<IDeclaredSymbol>)variables.Values);

        var parentAccessible = Parent?.AccessibleAt(statement) ?? [];

        return declared.Concat(parentAccessible);
    }

    private string GetDebuggerDisplay() =>
        $"Block scope {{ Functions = {Functions.Count}, Variables = {VariableTimeline[^1].Count} }}";
}
