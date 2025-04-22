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
    private readonly List<TriviaToken> triviaTokens;
    
    /// <summary>
    /// The source which is being parsed.
    /// </summary>
    public Source Source { get; }

    /// <summary>
    /// The current token.
    /// </summary>
    public Token Current { get; private set; }

    private ParseState(
        Source source,
        Token current,
        ImmutableArray<Token> tokens,
        int position,
        IEnumerable<TriviaToken> triviaTokens)
    {
        Source = source;
        Current = current;
        
        this.tokens = tokens;
        this.position = position;
        this.triviaTokens = triviaTokens.ToList();
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
        Current = tokens[0];
        
        this.tokens = tokens;
        position = 0;
        triviaTokens = [];
    }

    private Token MoveNext()
    {
        // Ensure that the position does not progress past the end of the tokens.
        if (position < tokens.Length - 1) position += 1;
        
        return tokens[position];
    }

    /// <summary>
    /// Advances the parser by one token and returns the previous token.
    /// </summary>
    public Token Advance()
    {
        var token = Current;

        Current = MoveNext();

        return token;
    }

    /// <summary>
    /// Consumes the current token and adds it as a trivia
    /// which will be appended as trivia to the next token.
    /// </summary>
    public void ConsumeAsTrivia(TriviaTokenKind kind)
    {
        var triviaToken = new TriviaToken(Current, kind);
        var next = MoveNext();

        Current = triviaToken.AttachTo(next);
    }

    /// <summary>
    /// Branches the parse state, creating a new state with the same source and AST,
    /// but which can be advanced and have diagnostics added to it without affecting the original state.
    /// </summary>
    public ParseState Branch() => new(
        Source,
        Current,
        tokens,
        position,
        triviaTokens);
}
