using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;
using TextMappingUtils;

namespace Noa.Compiler.Workspaces;

/// <summary>
/// Represents a single version of a document in a <see cref="Workspace{TUri}"/>.
/// </summary>
/// <typeparam name="TUri">The type of the URI of the document.</typeparam>
/// <param name="Ast">The AST of the document.</param>
/// <param name="Source"></param>
/// <param name="LineMap"></param>
/// <param name="Uri"></param>
public sealed record NoaDocument<TUri>(Ast Ast, Source Source, LineMap LineMap, TUri Uri)
    where TUri : notnull
{
    private Dictionary<ISymbol, IReadOnlyCollection<IdentifierExpression>>? references = null;

    /// <summary>
    /// Gets all references to a specified symbol within the document.
    /// </summary>
    /// <param name="symbol">The symbol to get the references to.</param>
    public IReadOnlyCollection<IdentifierExpression> GetReferences(ISymbol symbol)
    {
        if (references is not null &&
            references.TryGetValue(symbol, out var cached))
            return cached;
        
        references ??= [];

        var root = symbol switch
        {
            IFunctionNested { ContainingFunction: IDeclaredFunction declaredFunction } =>
                declaredFunction.Declaration,
            _ => Ast.Root
        };

        var nodes = root.DescendantsAndSelf()
            .OfType<IdentifierExpression>()
            .Where(x => x.ReferencedSymbol.Value == symbol)
            .ToList();

        references.Add(symbol, nodes);
        
        return nodes;
    }
}
