using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;

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

    private void ReportDiagnostic(DiagnosticTemplate template, TextSpan span)
    {
        var location = new Location(state.Source.Name, span);
        var diagnostic = template.Format(location);
        state.Diagnostics.Add(diagnostic);
    }

    private void ReportDiagnostic(DiagnosticTemplate<Token> template, Token token) =>
        ReportDiagnostic(template, token, token.Span);

    private void ReportDiagnostic<T>(DiagnosticTemplate<T> template, T arg, TextSpan span)
    {
        var location = new Location(state.Source.Name, span);
        var diagnostic = template.Format(arg, location);
        state.Diagnostics.Add(diagnostic);
    }
    
    private Token Advance() => state.Advance();

    private Token Expect(TokenKind kind)
    {
        if (Current.Kind == kind) return Advance();

        ReportDiagnostic(ParseDiagnostics.ExpectedKinds, [kind], Current.Span);
        
        var span = TextSpan.FromLength(Current.Span.Start, 0);
        return new(TokenKind.Error, "", span);
    }

    private Token? Expect(IReadOnlySet<TokenKind> kinds)
    {
        if (kinds.Contains(Current.Kind)) return Current;

        ReportDiagnostic(ParseDiagnostics.ExpectedKinds, kinds, Current.Span);
        
        return null;
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
            cancellationToken.ThrowIfCancellationRequested();

            var previousToken = Current;

            var node = parse();
            
            // Check whether the parser parsed anything at all to prevent it from getting stuck.
            if (Current == previousToken)
            {
                ReportDiagnostic(ParseDiagnostics.UnexpectedToken, Current);

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
