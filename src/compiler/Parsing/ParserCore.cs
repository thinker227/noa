using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    private ParseState state;
    
    private Source Source => state.Source;
    
    private Ast Ast => state.Ast;

    private Token Current => state.Current;

    public bool AtEnd => Current.Kind is TokenKind.EndOfFile;
    
    internal Parser(Source source, Ast ast, IEnumerable<Token> tokenSource) =>
        state = new(source, ast, tokenSource);

    private void ReportDiagnostic(IDiagnostic diagnostic) =>
        state.Diagnostics.Add(diagnostic);
    
    private Token Advance() => state.Advance();

    private Token Expect(TokenKind kind)
    {
        if (Current.Kind == kind) return Advance();

        var diagnostic = ParseDiagnostics.ExpectedKinds.Format([kind], Current.Location);
        ReportDiagnostic(diagnostic);
        
        var location = Location.FromLength(Source.Name, Current.Location.Start, 0);
        return new(TokenKind.Error, "", location);
    }

    private Token? Expect(IReadOnlySet<TokenKind> kinds)
    {
        if (kinds.Contains(Current.Kind)) return Current;

        var diagnostic = ParseDiagnostics.ExpectedKinds.Format(kinds, Current.Location);
        ReportDiagnostic(diagnostic);
        
        return null;
    }

    /// <summary>
    /// Parses a token-separated list of nodes.
    /// </summary>
    /// <param name="separatorKind">The token kind of the separator between the nodes.</param>
    /// <param name="allowTrailingSeparator">Whether to allow a trailing separator token at the end of the list.</param>
    /// <param name="parse">The function to parse a node.</param>
    /// <param name="stopKinds">The kinds at which to stop parsing the list.</param>
    /// <typeparam name="T">The type of the nodes to parse.</typeparam>
    private ImmutableArray<T> ParseSeparatedList<T>(
        TokenKind separatorKind,
        bool allowTrailingSeparator,
        Func<T> parse,
        params TokenKind[] stopKinds)
        where T : Node
    {
        var stopKindsSet = stopKinds.ToHashSet();
        if (AtEnd || stopKindsSet.Contains(Current.Kind)) return [];

        var nodes = ImmutableArray.CreateBuilder<T>();

        while (!AtEnd)
        {
            var previousToken = Current;

            var node = parse();
            
            // Check whether the parser parsed anything at all to prevent it from getting stuck.
            if (Current == previousToken)
            {
                var diagnostic = ParseDiagnostics.UnexpectedToken.Format(Current, Current.Location);
                ReportDiagnostic(diagnostic);

                Advance();
            }
            else nodes.Add(node);

            if (Current.Kind != separatorKind && stopKindsSet.Contains(Current.Kind)) break;

            Expect(separatorKind);

            // If we allow a trailing separator then check the stopping condition again.
            if (allowTrailingSeparator && stopKindsSet.Contains(Current.Kind)) break;
        }

        return nodes.ToImmutable();
    }
}
