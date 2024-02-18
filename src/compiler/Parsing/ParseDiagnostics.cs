using Noa.Compiler.Diagnostics;

namespace Noa.Compiler.Parsing;

internal static class ParseDiagnostics
{
    public static DiagnosticTemplate<string> UnexpectedCharacter { get; } =
        DiagnosticTemplate.Create<string>(
            "NOA-SYN-001",
            c => $"Unexpected character '{c}'",
            Severity.Error);

    public static DiagnosticTemplate<Token> UnexpectedToken { get; } =
        DiagnosticTemplate.Create<Token>(
            "NOA-SYN-002",
            token => $"Unexpected token '{token.Text}'",
            Severity.Error);
    
    public static DiagnosticTemplate<IReadOnlyCollection<TokenKind>> ExpectedKinds { get; } =
        DiagnosticTemplate.Create<IReadOnlyCollection<TokenKind>>(
            "NOA-SYN-003",
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
            "NOA-SYN-004",
            "Only block, call, if, loop, return, break, and continue expressions " +
            "may be used as statements",
            Severity.Error);
    
    public static DiagnosticTemplate<string> LiteralTooLarge { get; } =
        DiagnosticTemplate.Create<string>(
            "NOA-SYN-005",
            text => $"Number literal '{text}' is too large (> {int.MaxValue})",
            Severity.Error);

    public static DiagnosticTemplate InvalidLValue { get; } =
        DiagnosticTemplate.Create(
            "NOA-SYN-006",
            "Only identifier expressions can be used on the left-hand side of an assignment statement",
            Severity.Error);
}
