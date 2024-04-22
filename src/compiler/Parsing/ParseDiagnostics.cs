using Noa.Compiler.Diagnostics;

namespace Noa.Compiler.Parsing;

internal static class ParseDiagnostics
{
    public static DiagnosticTemplate<string> UnexpectedCharacter { get; } =
        DiagnosticTemplate.Create<string>(
            "NOA-SYN-001",
            (c, page) => page
                .Raw("Unexpected character ")
                .Source(c)
                .Raw("."),
            Severity.Error);

    public static DiagnosticTemplate<Token> UnexpectedToken { get; } =
        DiagnosticTemplate.Create<Token>(
            "NOA-SYN-002",
            (token, page) => page
                .Raw("Unexpected token ")
                .Source(token.Text)
                .Raw("."),
            Severity.Error);
    
    public static DiagnosticTemplate<IReadOnlyCollection<TokenKind>> ExpectedKinds { get; } =
        DiagnosticTemplate.Create<IReadOnlyCollection<TokenKind>>(
            "NOA-SYN-003",
            (kinds, page) =>
            {
                var ks = DiagnosticPageUtility.ToPageActions(
                    kinds,
                    (kind, p) => p.Keyword(kind.ToDisplayString()));
                
                if (kinds.Count == 1)
                {
                    page.Raw("Expected ")
                        .Many(ks, ManyTerminator.None);
                }
                else
                {
                    page.Raw("Expected either ")
                        .Many(ks, ManyTerminator.Or);
                }

                page.Raw(".");
            },
            Severity.Error);

    public static DiagnosticTemplate InvalidExpressionStatement { get; } =
        DiagnosticTemplate.Create(
            "NOA-SYN-004",
            page => page
                .Raw("Only ")
                .Many<string>(
                    ["block", "call", "if", "loop", "return", "break", "continue"],
                    (s, p) => p.Keyword(s),
                    ManyTerminator.And)
                .Raw(" expressions may be used as ")
                .Emphasized("statements")
                .Raw("."),
            Severity.Error);

    public static DiagnosticTemplate<string> LiteralTooLarge { get; } =
        DiagnosticTemplate.Create<string>(
            "NOA-SYN-005",
            (text, page) => page
                .Raw("Number literal ")
                .Source(text)
                .Raw(" is too large (")
                .Emphasized($"> {int.MaxValue}")
                .Raw(")."),
            Severity.Error);

    public static DiagnosticTemplate InvalidLValue { get; } =
        DiagnosticTemplate.Create(
            "NOA-SYN-006",
            // "Only identifier expressions can be used on the left-hand side of an assignment statement",
            page => page
                .Raw("Only ")
                .Keyword("identifier expressions")
                .Raw(" can be used on the ")
                .Emphasized("left-hand side of an assignment statement")
                .Raw("."),
            Severity.Error);
}
