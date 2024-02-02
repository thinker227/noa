namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    private readonly Source source;
    private readonly Ast ast;
    private readonly IEnumerator<Token> tokens;
    private readonly List<Diagnostic> diagnostics = [];
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
            diagnostics.Add(new(
                $"Unexpected token '{next.Text}'",
                Severity.Error,
                next.Location));
                
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

        diagnostics.Add(new(
            $"Expected {kind.ToDisplayString()}",
            Severity.Error,
            current.Location));

        var location = Location.FromLength(source.Name, current.Location.Start, 0);
        return new(TokenKind.Error, "", location);
    }

    private Token? Expect(IReadOnlySet<TokenKind> kinds)
    {
        if (kinds.Contains(current.Kind)) return current;

        var kindsString = Formatting.JoinOxfordOr(kinds
            .Select(kind => kind.ToDisplayString()));

        var message = kinds.Count == 1
            ? $"Expected {kindsString}"
            : $"Expected either {kindsString}";

        diagnostics.Add(new(message, Severity.Error, current.Location));

        return null;
    }
}
