using System.Diagnostics;
using Draco.Lsp.Model;
using Draco.Lsp.Server.Language;
using Noa.Compiler.Symbols;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer : ICodeCompletion
{
    public Task<IList<CompletionItem>> CompleteAsync(CompletionParams param, CancellationToken cancellationToken)
    {
        var documentUri = param.TextDocument.Uri;
        
        logger.Debug(
            "Fetching code completion items for {documentUri} at position {line}:{character}",
            documentUri,
            param.Position.Line,
            param.Position.Character);
        
        var document = workspace.GetOrCreateDocument(documentUri, cancellationToken);
        var position = ToAbsolutePosition(param.Position, document.LineMap);

        var completions = completionService.GetCompletions(document.Ast, position);
        var items = completions.Select(CompletionItem (completion) => completion switch
        {
            Compiler.Services.Completion.CompletionItem.Keyword keyword => new()
            {
                Label = keyword.Word,
                SortText = $"1{keyword.Word}",
                Kind = CompletionItemKind.Keyword
            },
            Compiler.Services.Completion.CompletionItem.Symbol symbol => new()
            {
                Label = symbol.Sym.Name,
                SortText = $"0{symbol.Sym.Name}",
                Kind = GetCompletionKind(symbol.Sym)
            },
            _ => throw new UnreachableException()
        });
        
        return Task.FromResult<IList<CompletionItem>>(items.ToList());

        static CompletionItemKind GetCompletionKind(ISymbol symbol) => symbol switch
        {
            IFunction => CompletionItemKind.Function,
            IVariableSymbol => CompletionItemKind.Variable,
            _ => CompletionItemKind.Text
        };
    }

    public CompletionRegistrationOptions CompletionRegistrationOptions => new()
    {
        DocumentSelector = DocumentSelector,
        ResolveProvider = false
    };
}
