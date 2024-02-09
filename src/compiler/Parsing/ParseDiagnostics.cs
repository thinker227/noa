using Noa.Compiler.Diagnostics;

namespace Noa.Compiler.Parsing;

internal static class ParseDiagnostics
{
    public static DiagnosticTemplate<Token> UnexpectedToken { get; } =
        DiagnosticTemplate.Create<Token>(
            "NOA-SYN-001",
            token => $"Unexpected token '{token.Text}'",
            Severity.Error);
    
    public static DiagnosticTemplate<IReadOnlyCollection<TokenKind>> ExpectedKinds { get; } =
        DiagnosticTemplate.Create<IReadOnlyCollection<TokenKind>>(
            "NOA-SYN-002",
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
            "NOA-SYN-003",
            "Only block, call, if, loop, return, break, and continue expressions " +
            "may be used as statements",
            Severity.Error);
    
    public static DiagnosticTemplate<string> LiteralTooLarge { get; } =
        DiagnosticTemplate.Create<string>(
            "NOA-SYN-004",
            text => $"Number literal '{text}' is too large (> {int.MaxValue})",
            Severity.Error);
}
