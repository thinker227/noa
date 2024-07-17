using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing;

/// <summary>
/// The state of a parser.
/// </summary>
internal sealed class ParseState
{
    private readonly ImmutableArray<Token> tokens;
    private int position;
    
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
    public Token Current => tokens[position];
    
    /// <summary>
    /// The diagnostics produced so far by the parser.
    /// </summary>
    public List<IDiagnostic> Diagnostics { get; }

    private ParseState(
        Source source,
        Ast ast,
        ImmutableArray<Token> tokens,
        int position,
        IEnumerable<IDiagnostic> diagnostics)
    {
        Source = source;
        Ast = ast;
        Diagnostics = diagnostics.ToList();
        
        this.tokens = tokens;
        this.position = position;
    }

    /// <summary>
    /// Creates a new <see cref="ParseState"/>.
    /// </summary>
    /// <param name="source">The source which is being parsed.</param>
    /// <param name="ast">The AST which parsed nodes belong to.</param>
    /// <param name="tokens">The tokens to parse.</param>
    public ParseState(Source source, Ast ast, ImmutableArray<Token> tokens)
    {
        Source = source;
        Ast = ast;
        Diagnostics = [];
        
        this.tokens = tokens;
        position = 0;

        SkipErroneous();
    }

    private bool Proceed()
    {
        if (position >= tokens.Length - 1) return false;
        
        position += 1;
        return true;
    }

    private void SkipErroneous()
    {
        while (Current.Kind is TokenKind.Error)
        {
            var diagnostic = ParseDiagnostics.UnexpectedCharacter.Format(Current.Text, Current.Location);
            Diagnostics.Add(diagnostic);
                
            // This should never occur because there should always be an end of file token
            // at the very end of the input, but just in case.
            if (!Proceed()) break;
        }
    }
    
    /// <summary>
    /// Advances the parser by one token and returns the previous token.
    /// Skips over any invalid tokens.
    /// </summary>
    public Token Advance()
    {
        var token = Current;
        
        Proceed();
        
        SkipErroneous();

        return token;
    }

    /// <summary>
    /// Branches the parse state, creating a new state with the same source and AST,
    /// but which can be advanced and have diagnostics added to it without affecting the original state.
    /// </summary>
    public ParseState Branch() => new(
        Source,
        Ast,
        tokens,
        position,
        Diagnostics);
}
