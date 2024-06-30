using Draco.Lsp.Model;
using Noa.Compiler;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;
using Location = Noa.Compiler.Location;

namespace Noa.LangServer;

public sealed record NoaDocument(Ast Ast, LineMap LineMap, DocumentUri Uri)
{
    private Dictionary<ISymbol, IReadOnlyCollection<Location>>? references = null;

    public IReadOnlyCollection<Location> GetReferences(ISymbol symbol, bool includeDeclaration)
    {
        references ??= [];

        if (references.TryGetValue(symbol, out var cached)) return cached;

        var nodes = new List<Location>();
        if (includeDeclaration && symbol is IDeclaredSymbol declared) nodes.Add(declared.DefinitionLocation);

        var referenceLocations = Ast.Root.DescendantsAndSelf()
            .OfType<IdentifierExpression>()
            .Where(x => x.ReferencedSymbol.Value == symbol)
            .Select(x => x.Location);
        nodes.AddRange(referenceLocations);

        references.Add(symbol, nodes);
        
        return nodes;
    }
}
