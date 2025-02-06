using System.Text.Json;
using System.Text.Json.Nodes;
using Draco.Lsp.Model;
using Noa.LangServer.Addon;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer : ICodeLens
{
    public CodeLensRegistrationOptions CodeLensRegistrationOptions => new()
    {
        DocumentSelector = DocumentSelector,
        ResolveProvider = false
    };

    public Task<IList<CodeLens>?> CodeLensAsync(CodeLensParams param, CancellationToken cancellationToken)
    {
        var documentUri = param.TextDocument.Uri;
        logger.Debug("Fetching code lens for {documentUri}", documentUri);

        var document = workspace.GetOrCreateDocument(documentUri, cancellationToken);

        // Make the code lens always show on the very first line with anything actually on it.
        var documentStart = document.Ast.Root.Span.Start;
        var firstLineSpan = document.LineMap.GetCharacterPosition(documentStart).Line.Span;

        var lens = new CodeLens()
        {
            Range = ToLspRange(firstLineSpan, document),
            Command = new Command()
            {
                Title = "▶ Run",
                Command_ = "noa-lang.run",
                Arguments = []
            }
        };

        return Task.FromResult<IList<CodeLens>?>([lens]);
    }
}
