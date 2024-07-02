using System.Diagnostics;
using Draco.Lsp.Model;
using Draco.Lsp.Server;
using Noa.Compiler;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;
using Serilog;
using Location = Draco.Lsp.Model.Location;
using Range = Draco.Lsp.Model.Range;

namespace Noa.LangServer;

/// <summary>
/// The Noa language server.
/// </summary>
/// <param name="client">The language client for the server.</param>
/// <param name="logger">The logger to log messages to.</param>
public sealed partial class NoaLanguageServer(
    ILanguageClient client,
    ILogger logger)
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

    /// <summary>
    /// Gets an existing document, or creates a new one and saves it if one doesn't already exist.
    /// </summary>
    /// <param name="documentUri">The URI of the document to get or create.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The existing or the newly created document.</returns>
    private NoaDocument GetOrCreateDocument(
        DocumentUri documentUri,
        CancellationToken cancellationToken = default) =>
        documents.TryGetValue(documentUri, out var document)
            ? document
            : UpdateOrCreateDocument(documentUri, cancellationToken: cancellationToken);

    /// <summary>
    /// Updates an existing document, or creates a new one if one doesn't already exist.
    /// The updated or newly created document is subsequently saved.
    /// </summary>
    /// <param name="documentUri">The URI of the document to update or create.</param>
    /// <param name="text">
    /// The text to update or create the document from.
    /// If not specified, reads the text from the document on disk.
    /// </param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The updated or newly created document.</returns>
    private NoaDocument UpdateOrCreateDocument(
        DocumentUri documentUri,
        string? text = null,
        CancellationToken cancellationToken = default)
    {
        var document = CreateDocument(documentUri, text, cancellationToken);
        documents[documentUri] = document;
        return document;
    }

    /// <summary>
    /// Creates a new document. The created document is <b>not</b> saved after being created.
    /// </summary>
    /// <param name="documentUri">The URI of the document to create.</param>
    /// <param name="text">
    /// The text to create the document from.
    /// If not specified, reads the text from the document on disk.
    /// </param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The newly created document.</returns>
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
        
        return new NoaDocument(ast, lineMap, documentUri);
    }

    private void DeleteDocument(DocumentUri documentUri) => documents.Remove(documentUri);

    private static int ToAbsolutePosition(Position position, LineMap lineMap) =>
        lineMap.GetLine((int)position.Line + 1).Start + (int)position.Character;

    private static Location ToLspLocation(Noa.Compiler.Location location, NoaDocument document) =>
        new()
        {
            Range = ToRange(location, document),
            Uri = document.Uri
        };

    /// <summary>
    /// Converts a <see cref="Noa.Compiler.Location"/> into a <see cref="Range"/>.
    /// </summary>
    /// <param name="location">The location to convert.</param>
    /// <param name="document">The document the location is from.</param>
    private static Range ToRange(Noa.Compiler.Location location, NoaDocument document)
    {
        var start = document.LineMap.GetCharacterPosition(location.Start);
        var end = document.LineMap.GetCharacterPosition(location.End);

        return new()
        {
            Start = new() { Line = (uint)start.Line.LineNumber - 1, Character = (uint)start.Offset },
            End = new() { Line = (uint)end.Line.LineNumber - 1, Character = (uint)end.Offset }
        };
    }
    
    /// <summary>
    /// Gets the markup text for displaying a symbol in a tooltip.
    /// </summary>
    /// <param name="symbol">The symbol to get the markup text for.</param>
    private static MarkupContent? GetMarkupForSymbol(ISymbol symbol)
    {
        var markup = symbol switch
        {
            NomialFunction or ParameterSymbol or VariableSymbol =>
                $"""
                 ```noa
                 {GetDisplayCode(symbol)}
                 ```
                 ({GetSymbolDescription(symbol)})                                                   
                 """,
            _ => null
        };
        
        if (markup is null) return null;
        
        return new()
        {
            Kind = MarkupKind.Markdown,
            Value = markup
        };

        static string GetSymbolDescription(ISymbol symbol) => symbol switch
        {
            NomialFunction => "function",
            ParameterSymbol => "parameter",
            VariableSymbol => "variable",
            _ => throw new UnreachableException()
        };
            
        static string GetDisplayCode(ISymbol symbol) => symbol switch
        {
            NomialFunction x => $"func {x.Name}({string.Join(", ", x.Parameters.Select(GetDisplayCode))})",
            ParameterSymbol { IsMutable: false } x => x.Name,
            ParameterSymbol { IsMutable: true } x => $"mut {x.Name}",
            VariableSymbol { IsMutable: false } x => $"let {x.Name}",
            VariableSymbol { IsMutable: true } x => $"let mut {x.Name}",
            _ => throw new UnreachableException()
        };
    }
    
    /// <summary>
    /// Gets the declared or referenced symbol for a node.
    /// </summary>
    /// <param name="node">The node to get the symbol for.</param>
    /// <returns>
    /// The symbol which <paramref name="node"/> declares or references,
    /// or null if the node is null or doesn't declare or reference a symbol.
    /// </returns>
    private static ISymbol? GetSymbol(Node? node) => node switch
    {
        IdentifierExpression x => x.ReferencedSymbol.Value,
        _ => GetDeclaredSymbol(node)
    };

    /// <summary>
    /// Gets the declared symbol for a node.
    /// </summary>
    /// <param name="node">The node to get the symbol for.</param>
    /// <returns>
    /// The symbol which <paramref name="node"/> declares,
    /// or null if the node is null or doesn't declare a symbol.
    /// </returns>
    private static IDeclaredSymbol? GetDeclaredSymbol(Node? node) => node switch
    {
        Identifier { Parent.Value: FunctionDeclaration x } => x.Symbol.Value,
        Identifier { Parent.Value: LetDeclaration x } => x.Symbol.Value,
        Identifier { Parent.Value: Parameter x } => x.Symbol.Value,
        _ => null
    };
}
