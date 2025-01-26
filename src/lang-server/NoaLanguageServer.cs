using System.Diagnostics;
using Draco.Lsp.Model;
using Draco.Lsp.Server;
using Noa.Compiler;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;
using TextMappingUtils;
using Noa.Compiler.Workspaces;
using Serilog;
using Location = Draco.Lsp.Model.Location;
using Range = Draco.Lsp.Model.Range;

namespace Noa.LangServer;

/// <summary>
/// The Noa language server.
/// </summary>
/// <param name="client">The language client for the server.</param>
/// <param name="logger">The logger to log messages to.</param>
/// <param name="sourceProvider">The source provider for the workspace.</param>
public sealed partial class NoaLanguageServer(
    ILanguageClient client,
    ILogger logger,
    ISourceProvider<DocumentUri> sourceProvider)
    : ILanguageServer
{
    private readonly Workspace<DocumentUri> workspace = new(sourceProvider);

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
    private static int ToAbsolutePosition(Position position, LineMap lineMap) =>
        lineMap.GetLine((int)position.Line + 1).Span.Start + (int)position.Character;

    private static Location ToLspLocation(Noa.Compiler.Location location, NoaDocument<DocumentUri> document) =>
        new()
        {
            Range = ToLspRange(location.Span, document),
            Uri = document.Uri
        };

    /// <summary>
    /// Converts a <see cref="TextSpan"/> into a <see cref="Range"/>.
    /// </summary>
    /// <param name="span">The span to convert.</param>
    /// <param name="document">The document the location is from.</param>
    private static Range ToLspRange(TextSpan span, NoaDocument<DocumentUri> document)
    {
        var start = document.LineMap.GetCharacterPosition(span.Start);
        var end = document.LineMap.GetCharacterPosition(span.End);

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
