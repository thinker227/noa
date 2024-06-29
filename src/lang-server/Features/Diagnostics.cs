using Noa.Compiler.Diagnostics;
using Draco.Lsp.Model;
using Draco.Lsp.Server.Language;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer : IDiagnostics
{
    public DiagnosticRegistrationOptions DiagnosticRegistrationOptions => new()
    {
        DocumentSelector = DocumentSelector,
        Identifier = "noa",
        InterFileDependencies = false,
        WorkspaceDiagnostics = false
    };

    public async Task<OneOf<RelatedFullDocumentDiagnosticReport, RelatedUnchangedDocumentDiagnosticReport>>
        DocumentDiagnosticsAsync(DocumentDiagnosticParams param, CancellationToken cancellationToken)
    {
        var documentUri = param.TextDocument.Uri;
        logger.Debug("Fetching diagnostics for {documentUri}", documentUri);
        
        var document = GetOrCreateDocument(documentUri, cancellationToken);
        var report = new RelatedFullDocumentDiagnosticReport()
        {
            Items = document.Ast.Diagnostics
                .Select(x => ConvertDiagnostic(x, document))
                .ToList()
        };

        await client.PublishDiagnosticsAsync(new()
        {
            Diagnostics = [],
            Uri = documentUri
        });

        return new(report);
    }

    private Draco.Lsp.Model.Diagnostic ConvertDiagnostic(IDiagnostic diagnostic, NoaDocument document)
    {
        var location = diagnostic.Location;
        var start = document.LineMap.GetCharacterPosition(location.Start);
        var end = document.LineMap.GetCharacterPosition(location.End);

        var message = diagnostic.WriteMessage(StringDiagnosticWriter.Writer);

        return new()
        {
            Message = message,
            Range = new()
            {
                Start = new() { Character = (uint)start.Offset, Line = (uint)start.Line.LineNumber - 1 },
                End = new() { Character = (uint)end.Offset, Line = (uint)end.Line.LineNumber - 1 }
            },
            Code = new OneOf<int, string>(diagnostic.Id.ToString()),
            Severity = diagnostic.Severity switch
            {
                Severity.Warning => DiagnosticSeverity.Warning,
                Severity.Error => DiagnosticSeverity.Error,
                _ => DiagnosticSeverity.Information
            }
        };
    }

    public Task<WorkspaceDiagnosticReport> WorkspaceDiagnosticsAsync(
        WorkspaceDiagnosticParams param,
        CancellationToken cancellationToken) =>
        Task.FromResult(new WorkspaceDiagnosticReport { Items = [] });
}
