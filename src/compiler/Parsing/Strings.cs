using System.Text;
using Noa.Compiler.Nodes;
using TextMappingUtils;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    private StringExpression ParseString()
    {
        var beginToken = Expect(TokenKind.BeginString);

        var parts = ImmutableArray.CreateBuilder<StringPart>();

        while (!AtEnd && Current.Kind is not TokenKind.EndString)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (Expect(SyntaxFacts.CanOccurWithinString)?.Kind)
            {
            case TokenKind.StringText:
                {
                    var textToken = Advance();
                    var text = ParseStringText(textToken);

                    parts.Add(new TextStringPart()
                    {
                        Ast = Ast,
                        Span = textToken.Span,
                        Text = text
                    });

                    break;
                }
            
            case TokenKind.BeginInterpolation:
                {
                    var beginInterpolationToken = Advance();

                    var expression = ParseExpressionOrError();

                    var endInterpolationToken = Expect(TokenKind.EndInterpolation);

                    parts.Add(new InterpolationStringPart()
                    {
                        Ast = Ast,
                        Span = TextSpan.Between(beginInterpolationToken.Span, endInterpolationToken.Span),
                        Expression = expression
                    });

                    break;
                }
            }
        }

        var endToken = Expect(TokenKind.EndString);

        return new()
        {
            Ast = Ast,
            Span = TextSpan.Between(beginToken.Span, endToken.Span),
            Parts = parts.ToImmutable()
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
            var position = token.Span.Start + 1 + i;
            var span = TextSpan.FromLength(position, 2);
            ReportDiagnostic(ParseDiagnostics.UnknownEscapeSequence, raw[i + 1].ToString(), span);
        }

        return text.ToString();
    }
}
