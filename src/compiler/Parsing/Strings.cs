using System.Text;
using Noa.Compiler.Syntax.Green;
using TextMappingUtils;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    private StringExpressionSyntax ParseString()
    {
        var beginToken = Expect(TokenKind.BeginString);

        var parts = ImmutableArray.CreateBuilder<StringPartSyntax>();

        while (!AtEnd && Current.Kind is not TokenKind.EndString)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (Expect(SyntaxFacts.CanOccurWithinString)?.Kind)
            {
            case TokenKind.StringText:
                {
                    var text = Advance();

                    parts.Add(new TextStringPartSyntax()
                    {
                        Text = text
                    });

                    break;
                }
            
            case TokenKind.BeginInterpolation:
                {
                    var beginInterpolationToken = Advance();

                    var expression = ParseExpressionOrError();

                    var endInterpolationToken = Expect(TokenKind.EndInterpolation);

                    parts.Add(new InterpolationStringPartSyntax()
                    {
                        OpenDelimiter = beginInterpolationToken,
                        Expression = expression,
                        CloseDelimiter = endInterpolationToken
                    });

                    break;
                }

            default:
                // If no token matched, then we've encountered an error.
                // However, the lexer should've already reported this, so we don't need to do anything here.
                // Yes I'm using goto here, but it's a valid use-case.
                goto end;
            }
        }
        end:

        var endToken = Expect(TokenKind.EndString);

        return new()
        {
            OpenQuote = beginToken,
            Parts = parts.ToImmutable(),
            CloseQuote = endToken
        };
    }

    private string ParseStringText(Token token)
    {
        var raw = token.Text;
        var text = new StringBuilder();

        for (var i = 0; i < raw.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var current = raw[i];

            if (current is not '\\')
            {
                text.Append(current);
                continue;
            }

            // Rationale here is that if the last character in a string literal is a \ then the string
            // *has* to be unterminated, so the user is probably about to write an escape sequence,
            // so let's just ignore it.
            if (i == raw.Length - 1) continue;

            var simple = raw[i + 1] switch
            {
                '\\' => '\\',
                '"' => '"',
                '{' => '{',
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                '0' => '\0',
                _ => null as char?
            };
            if (simple is not null)
            {
                text.Append(simple.Value);
                i++;
                continue;
            }

            // Accounting for the opening quote in the span.
            throw new NotImplementedException();
            // var position = token.Span.Start + 1 + i;
            // var span = TextSpan.FromLength(position, 2);
            // ReportDiagnostic(ParseDiagnostics.UnknownEscapeSequence, raw[i + 1].ToString(), span);
        }

        return text.ToString();
    }
}
