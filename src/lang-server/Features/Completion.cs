using Draco.Lsp.Model;
using Draco.Lsp.Server.Language;
using Noa.Compiler.Nodes;
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
        
        // Todo:
        // In certain situations, the node here will be (for instance) the surrounding block
        // and the completions will therefore be incomplete.
        var document = workspace.GetOrCreateDocument(documentUri, cancellationToken);
        var position = ToAbsolutePosition(param.Position, document.LineMap);
        var node = document.Ast.Root.FindNodeAt(position);

        IEnumerable<CompletionItem> symbolCompletions;
        if (node is null)
        {
            logger.Warning("Node at cursor position is null, " +
                           "cannot access current scope so can't get full completion list");
            symbolCompletions = [];
        }
        else
        {
            symbolCompletions = node.Scope.Value.AccessibleAt(node)
                .Select(x =>
                {
                    var item = new CompletionItem()
                    {
                        Label = x.Name,
                        SortText = $"0{x.Name}",
                        Kind = GetCompletionKind(x)
                    };
                    
                    var markup = GetMarkupForSymbol(x);
                    if (markup is not null) item.Documentation = new(markup);

                    return item;
                });
        }

        // Todo: suggest keywords based on context
        var keywords = new[]
        {
            "func",
            "let",
            "mut",
            "if",
            "else",
            "loop",
            "return",
            "break",
            "continue",
            "true",
            "false"
        };
        var keywordCompletions = keywords.Select(x => new CompletionItem()
        {
            Label = x,
            SortText = $"1{x}",
            Kind = CompletionItemKind.Keyword
        });

        var completions = symbolCompletions.Concat(keywordCompletions).ToList();
        
        return Task.FromResult<IList<CompletionItem>>(completions);

        static CompletionItemKind GetCompletionKind(ISymbol symbol) => symbol switch
        {
            NomialFunction => CompletionItemKind.Function,
            ParameterSymbol => CompletionItemKind.Variable,
            VariableSymbol => CompletionItemKind.Variable,
            _ => CompletionItemKind.Text
        };
    }

    public CompletionRegistrationOptions CompletionRegistrationOptions => new()
    {
        DocumentSelector = DocumentSelector,
        ResolveProvider = false
    };
}
