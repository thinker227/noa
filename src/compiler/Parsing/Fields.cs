using Noa.Compiler.Syntax.Green;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    internal FieldNameSyntax ParseFieldNameOrError()
    {
        if (ParseFieldNameOrNull() is { } name) return name;

        ReportDiagnostic(ParseDiagnostics.ExpectedKinds, SyntaxFacts.CanBeginFieldName, Current);

        return new ErrorFieldNameSyntax();
    }

    internal FieldNameSyntax? ParseFieldNameOrNull()
    {
        switch (Current.Kind)
        {
        case TokenKind.Name:
            {
                var name = Advance();

                return new SimpleFieldNameSyntax()
                {
                    NameToken = name
                };
            }
        
        case TokenKind.BeginString:
            {
                var @string = ParseString();

                return new StringFieldNameSyntax()
                {
                    String = @string
                };
            }
        
        case TokenKind.OpenParen:
            {
                var openParen = Advance();

                var expression = ParseExpressionOrError();

                var closeParen = Expect(TokenKind.CloseParen);

                return new ExpressionFieldNameSyntax()
                {
                    OpenParenToken = openParen,
                    Expression = expression,
                    CloseParenToken = closeParen
                };
            }
        
        default:
            return null;
        }
    }
}
