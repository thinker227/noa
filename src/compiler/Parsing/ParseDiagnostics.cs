namespace Noa.Compiler.Parsing;

internal static class ParseDiagnostics
{
    public static DiagnosticTemplate<Token> UnexpectedToken { get; } =
        DiagnosticTemplate.Create<Token>(
            token => $"Unexpected token '{token.Text}'",
            Severity.Error);
    
    public static DiagnosticTemplate<IReadOnlyCollection<TokenKind>> ExpectedKinds { get; } =
        DiagnosticTemplate.Create<IReadOnlyCollection<TokenKind>>(
            kinds =>
            {
                var kindsString = Formatting.JoinOxfordOr(kinds
                    .Select(kind => kind.ToDisplayString()));

                return kinds.Count == 1
                    ? $"Expected {kindsString}"
                    : $"Expected either {kindsString}";
            },
            Severity.Error);

    public static DiagnosticTemplate InvalidExpressionStatement { get; } =
        DiagnosticTemplate.Create(
            "Only block, call, if, loop, return, break, and continue expressions " +
            "may be used as statements",
            Severity.Error);
    
    public static DiagnosticTemplate<string> LiteralTooLarge { get; } =
        DiagnosticTemplate.Create<string>(
            text => $"Number literal '{text}' is too large (> {int.MaxValue})",
            Severity.Error);
}
