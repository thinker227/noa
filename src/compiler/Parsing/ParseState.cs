using Noa.Compiler.Diagnostics;
using Noa.Compiler.Syntax.Green;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

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
    /// The current token.
    /// </summary>
    public Token Current => tokens[position];

    private ParseState(
        Source source,
        ImmutableArray<Token> tokens,
        int position)
    {
        Source = source;
        
        this.tokens = tokens;
        this.position = position;
    }

    /// <summary>
    /// Creates a new <see cref="ParseState"/>.
    /// </summary>
    /// <param name="source">The source which is being parsed.</param>
    /// <param name="ast">The AST which parsed nodes belong to.</param>
    /// <param name="tokens">The tokens to parse.</param>
    public ParseState(Source source, ImmutableArray<Token> tokens)
    {
        Source = source;
        
        this.tokens = tokens;
        position = 0;
    }
    
    /// <summary>
    /// Advances the parser by one token and returns the previous token.
    /// Skips over any invalid tokens.
    /// </summary>
    public Token Advance()
    {
        var token = Current;

        // Ensure that the position does not progress past the end of the tokens.
        if (position < tokens.Length - 1) position += 1;

        return token;
    }

    /// <summary>
    /// Branches the parse state, creating a new state with the same source and AST,
    /// but which can be advanced and have diagnostics added to it without affecting the original state.
    /// </summary>
    public ParseState Branch() => new(
        Source,
        tokens,
        position);
}
