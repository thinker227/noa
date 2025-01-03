using Draco.Lsp.Model;
using Draco.Lsp.Server.Language;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;
using Noa.LangServer.Addon;
using Range = Draco.Lsp.Model.Range;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer : IRename, IPrepareRename
{
    public Task<OneOf<Range, PrepareRenameResult>?> PrepareRenameAsync(
        PrepareRenameParams param,
        CancellationToken cancellationToken)
    {
        var documentUri = param.TextDocument.Uri;
        
        logger.Debug(
            "Preparing rename for {documentUri} at position {line}:{character}",
            documentUri,
            param.Position.Line,
            param.Position.Character);
        
        var document = workspace.GetOrCreateDocument(documentUri, cancellationToken);
        var position = ToAbsolutePosition(param.Position, document.LineMap);
        var node = document.Ast.Root.FindNodeAt(position);
        var symbol = GetSymbol(node);

        if (symbol is not IDeclaredSymbol declared) return Task.FromResult<OneOf<Range, PrepareRenameResult>?>(null);

        var range = ToLspRange(declared.DefinitionLocation.Span, document);

        return Task.FromResult(new OneOf<Range, PrepareRenameResult>?(range));
    }
    
    public Task<WorkspaceEdit?> RenameAsync(RenameParams param, CancellationToken cancellationToken)
    {
        var documentUri = param.TextDocument.Uri;
        
        logger.Debug(
            "Fetching hover for {documentUri} at position {line}:{character}",
            documentUri,
            param.Position.Line,
            param.Position.Character);
        
        var document = workspace.GetOrCreateDocument(documentUri, cancellationToken);
        var position = ToAbsolutePosition(param.Position, document.LineMap);
        var node = document.Ast.Root.FindNodeAt(position);
        var symbol = GetSymbol(node);

        if (symbol is not IDeclaredSymbol declared) return Task.FromResult<WorkspaceEdit?>(null);

        var references = document.GetReferences(declared, true);

        var textEdits = references.Select(x => new TextEdit()
        {
            Range = ToLspRange(x.Span, document),
            NewText = param.NewName
        });
        var textDocumentEdit = new TextDocumentEdit()
        {
            TextDocument = new() { Uri = documentUri, Version = null },
            Edits = textEdits
                .Select(x => new OneOf<ITextEdit, AnnotatedTextEdit>(x))
                .ToList()
        };
        var workspaceEdit = new WorkspaceEdit()
        {
            DocumentChanges = [new(textDocumentEdit)]
        };

        return Task.FromResult<WorkspaceEdit?>(workspaceEdit);
    }

    public RenameRegistrationOptions RenameRegistrationOptions => new()
    {
        DocumentSelector = DocumentSelector,
        PrepareProvider = true
    };
}
