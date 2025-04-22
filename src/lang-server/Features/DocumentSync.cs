using Draco.Lsp.Model;
using Draco.Lsp.Server.TextDocument;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer : ITextDocumentSync
{
    // Recompile the entire file for each change.
    public TextDocumentSyncKind SyncKind => TextDocumentSyncKind.Full;
    
    public Task TextDocumentDidOpenAsync(DidOpenTextDocumentParams param)
    {
        logger.Verbose("Opening document {documentUri}", param.TextDocument.Uri);
        workspace.GetOrCreateDocument(param.TextDocument.Uri);
        
        return Task.CompletedTask;
    }

    public Task TextDocumentDidCloseAsync(DidCloseTextDocumentParams param)
    {
        logger.Verbose("Closing document {documentUri}", param.TextDocument.Uri);
        workspace.DeleteDocument(param.TextDocument.Uri);

        return Task.CompletedTask;
    }

    public Task TextDocumentDidChangeAsync(DidChangeTextDocumentParams param)
    {
        logger.Verbose("Updating document {documentUri}", param.TextDocument.Uri);
        var newText = param.ContentChanges[0].Text;
        workspace.UpdateOrCreateDocument(param.TextDocument.Uri, newText);
        
        return Task.CompletedTask;
    }
}
