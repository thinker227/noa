using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    private readonly Source source;
    private readonly Ast ast;
    private readonly IEnumerator<Token> tokens;
    private readonly List<IDiagnostic> diagnostics = [];
    private Token current;

    public bool AtEnd => current.Kind is TokenKind.EndOfFile;
    
    private Parser(Source source, Ast ast, IEnumerable<Token> tokens)
    {
        this.source = source;
        this.ast = ast;
        this.tokens = tokens.GetEnumerator();

        // Advance to the first token.
        Advance();
    }

    private Token Advance()
    {
        var token = current;

        // If we're at the end of the input, the end of file token should be persistent.
        if (!tokens.MoveNext()) return token;
        
        var next = tokens.Current;

        // Loop through and skip any erroneous tokens
        while (next.Kind is TokenKind.Error)
        {
            var diagnostic = ParseDiagnostics.UnexpectedToken.Format(next, next.Location);
            diagnostics.Add(diagnostic);
                
            // This should never occur because there should always be an end of file token
            // at the very end of the input, but just in case.
            if (!tokens.MoveNext()) break;

            next = tokens.Current;
        }
            
        current = next;

        return token;
    }

    private Token Expect(TokenKind kind)
    {
        if (current.Kind == kind) return Advance();

        var diagnostic = ParseDiagnostics.ExpectedKinds.Format([kind], current.Location);
        diagnostics.Add(diagnostic);
        
        var location = Location.FromLength(source.Name, current.Location.Start, 0);
        return new(TokenKind.Error, "", location);
    }

    private Token? Expect(IReadOnlySet<TokenKind> kinds)
    {
        if (kinds.Contains(current.Kind)) return current;

        var diagnostic = ParseDiagnostics.ExpectedKinds.Format(kinds, current.Location);
        diagnostics.Add(diagnostic);
        
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
        if (AtEnd || stopKindsSet.Contains(current.Kind)) return [];

        var nodes = ImmutableArray.CreateBuilder<T>();

        while (!AtEnd)
        {
            var previousToken = current;

            var node = parse();
            
            // Check whether the parser parsed anything at all to prevent it from getting stuck.
            if (current == previousToken)
            {
                var diagnostic = ParseDiagnostics.UnexpectedToken.Format(current, current.Location);
                diagnostics.Add(diagnostic);

                Advance();
            }
            else nodes.Add(node);

            if (current.Kind != separatorKind && stopKindsSet.Contains(current.Kind)) break;

            Expect(separatorKind);

            // If we allow a trailing separator then check the stopping condition again.
            if (allowTrailingSeparator && stopKindsSet.Contains(current.Kind)) break;
        }

        return nodes.ToImmutable();
    }
}
