using Draco.Lsp.Model;
using Draco.Lsp.Server.Language;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer : IGotoDefinition
{
    public Task<IList<Location>> GotoDefinitionAsync(DefinitionParams param, CancellationToken cancellationToken)
    {
        var documentUri = param.TextDocument.Uri;
        
        logger.Debug(
            "Fetching goto definition info for {documentUri} at position {line}:{character}",
            documentUri,
            param.Position.Line,
            param.Position.Character);
        
        var document = GetOrCreateDocument(documentUri, cancellationToken);
        var position = ToAbsolute(param.Position, document.LineMap);
        var node = document.Ast.Root.FindNodeAt(position);

        if (GetDefinitionLocation(node) is not {} location) return Task.FromResult<IList<Location>>([]);

        var start = document.LineMap.GetCharacterPosition(location.Start);
        var end = document.LineMap.GetCharacterPosition(location.End);

        var lspLocation = new Location()
        {
            Range = new()
            {
                Start = new() { Line = (uint)start.Line.LineNumber - 1, Character = (uint)start.Offset },
                End = new() { Line = (uint)end.Line.LineNumber - 1, Character = (uint)end.Offset }
            },
            Uri = documentUri
        };

        return Task.FromResult<IList<Location>>([lspLocation]);

        static Noa.Compiler.Location? GetDefinitionLocation(Node? node) => node switch
        {
            Identifier { Parent.Value: FunctionDeclaration x } => x.Location,
            Identifier { Parent.Value: LetDeclaration x } => x.Location,
            IdentifierExpression { ReferencedSymbol.Value: IDeclaredSymbol symbol } => symbol.Declaration.Location,
            _ => null
        };
    }

    public DefinitionRegistrationOptions GotoDefinitionRegistrationOptions => new()
    {
        DocumentSelector = DocumentSelector
    };
}
