using System.Text;
using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    private string ParseStringText(Token strToken)
    {
        // A string literal might be unterminated, but we should still parse it properly,
        // so we need to account for the text possibly not ending with a closing quote.
        var endOffset = strToken.Text.EndsWith('"') ? -1 : 0;
        var raw = strToken.Text.AsSpan(1..(strToken.Text.Length + endOffset));

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

            // Accounting for the opening quote in the location.
            var position = strToken.Location.Start + 1 + i;
            var location = Location.FromLength(Source.Name, position, 2);
            ReportDiagnostic(ParseDiagnostics.UnknownEscapeSequence.Format(raw[i + 1].ToString(), location));
        }

        return text.ToString();
    }
}
