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
        
        return new NoaDocument(ast, lineMap, documentUri);
    }

    private static int ToAbsolutePosition(Position position, LineMap lineMap) =>
        lineMap.GetLine((int)position.Line + 1).Start + (int)position.Character;

    private static Location ToLspLocation(Noa.Compiler.Location location, NoaDocument document) =>
        new()
        {
            Range = ToRange(location, document),
            Uri = document.Uri
        };

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
    
    private static MarkupContent? GetMarkupForSymbol(ISymbol symbol)
    {
        var markup = symbol switch
        {
            NomialFunction or ParameterSymbol or VariableSymbol =>
                $"""
                 ```noa
                 {GetDisplayCode(symbol)}
                 ```
                 ({GetSymbolKind(symbol)})                                                   
                 """,
            _ => null
        };
        
        if (markup is null) return null;
        
        return new()
        {
            Kind = MarkupKind.Markdown,
            Value = markup
        };

        static string GetSymbolKind(ISymbol symbol) => symbol switch
        {
            NomialFunction => "function",
            ParameterSymbol => "parameter",
            VariableSymbol => "variable",
            _ => throw new UnreachableException()
        };
            
        static string GetDisplayCode(ISymbol symbol) => symbol switch
        {
            NomialFunction x => $"func {x.Name}(...)",
            ParameterSymbol { Function: NomialFunction f } x => $"func {f.Name}({x.Name})",
            ParameterSymbol { Function: LambdaFunction } x => $"({x.Name}) => ...",
            VariableSymbol { IsMutable: false } x => $"let {x.Name}",
            VariableSymbol { IsMutable: true } x => $"let mut {x.Name}",
            _ => throw new UnreachableException()
        };
    }
    
    private static ISymbol? GetSymbol(Node? node) => node switch
    {
        Identifier { Parent.Value: FunctionDeclaration x } => x.Symbol.Value,
        Identifier { Parent.Value: LetDeclaration x } => x.Symbol.Value,
        Identifier { Parent.Value: Parameter x } => x.Symbol.Value,
        IdentifierExpression x => x.ReferencedSymbol.Value,
        _ => null
    };
}
