using Draco.Lsp.Model;
using Draco.Lsp.Server;
using Noa.Compiler;
using Serilog;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer(
    ILanguageClient client,
    ILogger logger,
    CancellationToken cancellationToken)
    : ILanguageServer
{
    private readonly Dictionary<DocumentUri, NoaDocument> documents = [];
    
    public InitializeResult.ServerInfoResult? Info { get; } = new()
    {
        Name = "Noa Language Server",
        Version = "0.1.0"
    };

    public IList<DocumentFilter> DocumentSelector =>
    [
        new()
        {
            Language = "noa",
            Pattern = "**/*.noa"
        }
    ];

    private NoaDocument GetOrCreateDocument(
        DocumentUri documentUri,
        CancellationToken cancellationToken = default) =>
        documents.TryGetValue(documentUri, out var document)
            ? document
            : UpdateOrCreateDocument(documentUri, cancellationToken: cancellationToken);

    private NoaDocument UpdateOrCreateDocument(
        DocumentUri documentUri,
        string? text = null,
        CancellationToken cancellationToken = default)
    {
        var document = CreateDocument(documentUri, text, cancellationToken);
        documents[documentUri] = document;
        return document;
    }

    private NoaDocument CreateDocument(
        DocumentUri documentUri,
        string? text,
        CancellationToken cancellationToken = default)
    {
        var path = documentUri.ToUri().LocalPath;
        text ??= File.ReadAllText(path);
        var source = new Source(text, path);
        
        // Todo: compilation safety measures
        logger.Verbose("Compiling {documentUri}", documentUri);
        var ast = Ast.Create(source, cancellationToken);
        var lineMap = LineMap.Create(text);
        
        return new NoaDocument(ast, lineMap);
    }
}
