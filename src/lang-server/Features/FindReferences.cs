using Draco.Lsp.Model;
using Draco.Lsp.Server.Language;
using Noa.Compiler.Nodes;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer : IFindReferences
{
    public Task<IList<Location>> FindReferencesAsync(ReferenceParams param, CancellationToken cancellationToken)
    {
        var documentUri = param.TextDocument.Uri;
        
        logger.Debug(
            "Fetching all references for {documentUri} at position {line}:{character}",
            documentUri,
            param.Position.Line,
            param.Position.Character);
        
        var document = GetOrCreateDocument(documentUri, cancellationToken);
        var position = ToAbsolutePosition(param.Position, document.LineMap);
        var node = document.Ast.Root.FindNodeAt(position);
        
        var symbol = node switch
        {
            Identifier { Parent.Value: FunctionDeclaration x } => x.Symbol.Value,
            Identifier { Parent.Value: LetDeclaration x } => x.Symbol.Value,
            Identifier { Parent.Value: Parameter x } => x.Symbol.Value,
            IdentifierExpression x => x.ReferencedSymbol.Value,
            _ => null
        };
        if (symbol is null) return Task.FromResult<IList<Location>>([]);

        var references = document.GetReferences(symbol, param.Context.IncludeDeclaration);
        var locations = references
            .Select(x => ToLspLocation(x, document))
            .ToList();

        return Task.FromResult<IList<Location>>(locations);
    }

    public ReferenceRegistrationOptions FindReferencesRegistrationOptions => new()
    {
        DocumentSelector = DocumentSelector
    };
}
