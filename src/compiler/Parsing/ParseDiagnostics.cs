using Noa.Compiler.Diagnostics;
using Token = Noa.Compiler.Syntax.Green.Token;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

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

    public static DiagnosticTemplate<Unit> InvalidExpressionStatement { get; } =
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

    public static DiagnosticTemplate<Unit> InvalidLValue { get; } =
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

    public static DiagnosticTemplate<Unit> ElseOmitted { get; } =
        DiagnosticTemplate.Create(
            "NOA-SYN-007",
            page => page
                .Raw("The ")
                .Keyword("else branch")
                .Raw(" of an ")
                .Keyword("if expression")
                .Raw(" can only be omitted when the expression is used as a statement."),
            Severity.Error);
    
    public static DiagnosticTemplate<Unit> UnterminatedString { get; } =
        DiagnosticTemplate.Create(
            "NOA-SYN-008",
            page => page
                .Raw("Unterminated ")
                .Keyword("string literal")
                .Raw("."),
            Severity.Error);
    
    public static DiagnosticTemplate<string> UnknownEscapeSequence { get; } =
        DiagnosticTemplate.Create<string>(
            "NOA-SYN-009",
            (seq, page) => page
                .Raw("Unknown escape sequence ")
                .Source(seq)
                .Raw("."),
            Severity.Error);
}
