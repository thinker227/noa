using System.Text;
using Noa.Compiler.Nodes;
using TextMappingUtils;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    private string ParseStringText(Token token)
    {
        // A string literal might be unterminated, but we should still parse it properly,
        // so we need to account for the text possibly not ending with a closing quote.
        var endOffset = token.Text.EndsWith('"') ? -1 : 0;
        var end = token.Text.Length + endOffset;
        var raw = token.Text.AsSpan(1..end);

        var text = new StringBuilder();

        for (var i = 0; i < raw.Length; i++)
        {
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
