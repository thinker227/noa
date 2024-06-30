using System.Diagnostics;
using Draco.Lsp.Model;
using Draco.Lsp.Server.Language;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer : IHover
{
    public Task<Hover?> HoverAsync(HoverParams param, CancellationToken cancellationToken)
    {
        var documentUri = param.TextDocument.Uri;
        
        logger.Debug(
            "Fetching hover for {documentUri} at position {line}:{character}",
            documentUri,
            param.Position.Line,
            param.Position.Character);
        
        var document = GetOrCreateDocument(documentUri, cancellationToken);
        var position = ToAbsolutePosition(param.Position, document.LineMap);
        var node = document.Ast.Root.FindNodeAt(position);
        
        var hover = node switch
        {
            _ when GetSymbol(node) is {} symbol => GetHoverForSymbol(symbol),
            _ => null
        };

        return Task.FromResult(hover);
    }

    private static Hover? GetHoverForSymbol(ISymbol symbol)
    {
        var markup = GetMarkupForSymbol(symbol);
        if (markup is null) return null;
        
        return new()
        {
            Contents = new(markup)
        };
    }

    public HoverRegistrationOptions HoverRegistrationOptions => new()
    {
        DocumentSelector = DocumentSelector
    };
}
