using Draco.Lsp.Model;
using Noa.Compiler;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.LangServer;

public sealed record NoaDocument(Ast Ast, LineMap LineMap, DocumentUri Uri)
{
    private Dictionary<ISymbol, IReadOnlyCollection<Node>>? references = null;

    public IReadOnlyCollection<Node> GetReferences(ISymbol symbol, bool includeDeclaration)
    {
        references ??= [];

        if (references.TryGetValue(symbol, out var cached)) return cached;

        var nodes = new List<Node>();
        if (includeDeclaration && symbol is IDeclaredSymbol declared) nodes.Add(declared.Declaration);

        var referenceNodes = Ast.Root.DescendantsAndSelf()
            .OfType<IdentifierExpression>()
            .Where(x => x.ReferencedSymbol.Value == symbol);
        nodes.AddRange(referenceNodes);

        references.Add(symbol, nodes);
        
        return nodes;
    }
}
