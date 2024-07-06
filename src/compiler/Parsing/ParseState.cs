using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;
using SuperLinq;

namespace Noa.Compiler.Parsing;

/// <summary>
/// The state of a parser.
/// </summary>
internal sealed class ParseState
{
    private readonly IBuffer<Token> tokenSource;
    private readonly IEnumerator<Token> tokens;
    private Token current;
    
    /// <summary>
    /// The source which is being parsed.
    /// </summary>
    public Source Source { get; }
    
    /// <summary>
    /// The AST which parsed nodes belong to.
    /// </summary>
    public Ast Ast { get; }

    /// <summary>
    /// The current token.
    /// </summary>
    public Token Current => current;
    
    /// <summary>
    /// The diagnostics produced so far by the parser.
    /// </summary>
    public List<IDiagnostic> Diagnostics { get; }

    private ParseState(
        Source source,
        Ast ast,
        IBuffer<Token> tokenSource,
        IEnumerator<Token> tokens,
        Token current,
        IEnumerable<IDiagnostic> diagnostics)
    {
        Source = source;
        Ast = ast;
        Diagnostics = diagnostics.ToList();
        
        this.tokenSource = tokenSource;
        this.tokens = tokens;
        this.current = current;
    }

    /// <summary>
    /// Creates a new <see cref="ParseState"/>.
    /// </summary>
    /// <param name="source">The source which is being parsed.</param>
    /// <param name="ast">The AST which parsed nodes belong to.</param>
    /// <param name="tokens">The tokens to parse.</param>
    public ParseState(Source source, Ast ast, IEnumerable<Token> tokens)
    {
        Source = source;
        Ast = ast;
        Diagnostics = [];
        
        tokenSource = tokens.Publish();
        this.tokens = tokenSource.GetEnumerator();
        
        // Advance to the first token.
        // The input sequence should always contain at least one token, so this should always work.
        Advance();
    }
    
    /// <summary>
    /// Advances the parser by one token and returns the previous token.
    /// Skips over any invalid tokens.
    /// </summary>
    public Token Advance()
    {
        var token = current;

        // If we're at the end of the input, the end of file token should be persistent.
        if (!tokens.MoveNext()) return token;
        
        var next = tokens.Current;

        // Loop through and skip any erroneous tokens
        while (next.Kind is TokenKind.Error)
        {
            var diagnostic = ParseDiagnostics.UnexpectedCharacter.Format(next.Text, next.Location);
            Diagnostics.Add(diagnostic);
                
            // This should never occur because there should always be an end of file token
            // at the very end of the input, but just in case.
            if (!tokens.MoveNext()) break;

            next = tokens.Current;
        }
            
        current = next;

        return token;
    }

    /// <summary>
    /// Branches the parse state, creating a new state with the same source and AST,
    /// but which can be advanced and have diagnostics added to it without affecting the original state.
    /// </summary>
    public ParseState Branch() => new(
        Source,
        Ast,
        tokenSource,
        tokenSource.GetEnumerator(),
        current,
        Diagnostics);
}
