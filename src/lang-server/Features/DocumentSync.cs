using Draco.Lsp.Model;
using Draco.Lsp.Server.TextDocument;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer : ITextDocumentSync
{
    // Recompile the entire file for each change.
    public TextDocumentSyncKind SyncKind => TextDocumentSyncKind.Full;
    
    public Task TextDocumentDidOpenAsync(DidOpenTextDocumentParams param)
    {
        workspace.GetOrCreateDocument(param.TextDocument.Uri);
        
        return Task.CompletedTask;
    }

    public Task TextDocumentDidCloseAsync(DidCloseTextDocumentParams param)
    {
        workspace.DeleteDocument(param.TextDocument.Uri);

        return Task.CompletedTask;
    }

    public Task TextDocumentDidChangeAsync(DidChangeTextDocumentParams param)
    {
        workspace.MarkAsUpdated(param.TextDocument.Uri);
        
        return Task.CompletedTask;
    }
}
