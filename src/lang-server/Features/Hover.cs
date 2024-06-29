using System.Diagnostics;
using Draco.Lsp.Model;
using Draco.Lsp.Server.Language;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer : IHover
{
    public Task<Hover?> HoverAsync(HoverParams param, CancellationToken cancellationToken)
    {
        var documentUri = param.TextDocument.Uri;
        
        logger.Debug(
            "Fetching hover for {documentUri} at position {line}:{character}",
            documentUri,
            param.Position.Line,
            param.Position.Character);
        
        var document = GetOrCreateDocument(documentUri, cancellationToken);
        var position = ToAbsolutePosition(param.Position, document.LineMap);
        var node = document.Ast.Root.FindNodeAt(position);

        var hover = node switch
        {
            Identifier { Parent.Value: FunctionDeclaration x } => GetHoverForSymbol(x.Symbol.Value),
            Identifier { Parent.Value: LetDeclaration x } => GetHoverForSymbol(x.Symbol.Value),
            IdentifierExpression x => GetHoverForSymbol(x.ReferencedSymbol.Value),
            _ => null
        };

        return Task.FromResult(hover);
    }

    private static Hover? GetHoverForSymbol(ISymbol symbol)
    {
        var markup = symbol switch
        {
            NomialFunction or ParameterSymbol or VariableSymbol =>
                $"""
                 ```noa
                 {GetDisplayCode(symbol)}
                 ```                                                   
                 """,
            _ => null
        };
        
        if (markup is null) return null;
        
        var content = new MarkupContent()
        {
            Kind = MarkupKind.Markdown,
            Value = markup
        };
        
        return new()
        {
            Contents = new(content)
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

    public HoverRegistrationOptions HoverRegistrationOptions => new()
    {
        DocumentSelector = DocumentSelector
    };
}
