using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;
using Noa.Compiler.Text;

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
    private Dictionary<(ISymbol, bool), IReadOnlyCollection<Location>>? references = null;

    /// <summary>
    /// Gets all references to a specified symbol within the document.
    /// </summary>
    /// <param name="symbol">The symbol to get the references to.</param>
    /// <param name="includeDeclaration">Whether to include the location of the declaration in the list of references.</param>
    public IReadOnlyCollection<Location> GetReferences(ISymbol symbol, bool includeDeclaration)
    {
        if (references is not null &&
            references.TryGetValue((symbol, includeDeclaration), out var cached))
            return cached;
        
        references ??= [];

        var nodes = new List<Location>();
        if (includeDeclaration && symbol is IDeclaredSymbol declared) nodes.Add(declared.DefinitionLocation);

        var root = symbol switch
        {
            IFunctionNested { ContainingFunction: IDeclaredFunction declaredFunction } =>
                declaredFunction.Declaration,
            _ => Ast.Root
        };

        var referenceLocations = root.DescendantsAndSelf()
            .OfType<IdentifierExpression>()
            .Where(x => x.ReferencedSymbol.Value == symbol)
            .Select(x => x.Location);
        nodes.AddRange(referenceLocations);

        references.Add((symbol, includeDeclaration), nodes);
        
        return nodes;
    }
}
