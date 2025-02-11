using Noa.Compiler.Diagnostics;
using Noa.Compiler.Syntax.Green;
using TextMappingUtils;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    private ParseState state;
    private readonly CancellationToken cancellationToken;

    internal IReadOnlyCollection<IDiagnostic> Diagnostics => state.Diagnostics;
    
    private Ast Ast => state.Ast;

    private Token Current => state.Current;

    private bool AtEnd => Current.Kind is TokenKind.EndOfFile;

    internal Parser(
        Source source,
        Ast ast,
        ImmutableArray<Token> tokenSource,
        CancellationToken cancellationToken)
    {
        state = new(source, ast, tokenSource);
        this.cancellationToken = cancellationToken;
    }

    private void ReportDiagnostic(DiagnosticTemplate template, SyntaxNode node) =>
        throw new NotImplementedException();

    private void ReportDiagnostic<T>(DiagnosticTemplate<T> template, T arg, SyntaxNode node) =>
        throw new NotImplementedException();
    
    private Token Advance() => state.Advance();

    private Token Expect(TokenKind kind)
    {
        if (Current.Kind == kind) return Advance();

        throw new NotImplementedException();

        // ReportDiagnostic(ParseDiagnostics.ExpectedKinds, [kind], Current.Span);
        
        // var span = TextSpan.FromLength(Current.Span.Start, 0);
        // return new(TokenKind.Error, "", span);
    }

    private Token? Expect(IReadOnlySet<TokenKind> kinds)
    {
        if (kinds.Contains(Current.Kind)) return Current;

        throw new NotImplementedException();

        // ReportDiagnostic(ParseDiagnostics.ExpectedKinds, kinds, Current.Span);
        
        // return null;
    }

    /// <summary>
    /// Synchronizes the parser with a set of token kinds.
    /// </summary>
    /// <param name="synchronizationTokens">The token kinds to synchronize with.</param>
    private void Synchronize(IReadOnlySet<TokenKind> synchronizationTokens)
    {
        while (!AtEnd && !synchronizationTokens.Contains(Current.Kind))
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            Advance();
        }
    }

    /// <summary>
    /// Parses a token-separated list of nodes.
    /// </summary>
    /// <param name="separatorKind">The token kind of the separator between the nodes.</param>
    /// <param name="allowTrailingSeparator">Whether to allow a trailing separator token at the end of the list.</param>
    /// <param name="parse">The function to parse a node.</param>
    /// <param name="stopKinds">The kinds at which to stop parsing the list.</param>
    /// <typeparam name="T">The type of the nodes to parse.</typeparam>
    private SeparatedSyntaxList<T> ParseSeparatedList<T>(
        TokenKind separatorKind,
        bool allowTrailingSeparator,
        Func<T> parse,
        params TokenKind[] stopKinds)
        where T : SyntaxNode
    {
        var stopKindsSet = stopKinds.ToHashSet();
        if (AtEnd || stopKindsSet.Contains(Current.Kind)) return SeparatedSyntaxList<T>.Empty;

        var nodes = new List<T>();
        var separators = new List<Token>();

        while (!AtEnd)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var previousToken = Current;

            var node = parse();
            
            // Check whether the parser parsed anything at all to prevent it from getting stuck.
            if (Current == previousToken)
            {
                throw new NotImplementedException();
                // ReportDiagnostic(ParseDiagnostics.UnexpectedToken, Current);

                // Advance();
            }
            else nodes.Add(node);

            if (Current.Kind != separatorKind && stopKindsSet.Contains(Current.Kind)) break;

            var separator = Expect(separatorKind);
            separators.Add(separator);

            // If we allow a trailing separator then check the stopping condition again.
            if (allowTrailingSeparator && stopKindsSet.Contains(Current.Kind)) break;
        }

        return SeparatedSyntaxList<T>.Create(nodes, separators);
    }
}
