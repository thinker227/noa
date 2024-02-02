namespace Noa.Compiler.Parsing;

internal sealed partial class Lexer(Source source)
{
    private readonly Source source = source;
    private readonly string text = source.Text;
    private int position = 0;

    private char Current =>
        position < text.Length
            ? text[position]
            : '\0';

    private ReadOnlySpan<char> Rest => text.AsSpan(position);

    private bool AtEnd => position >= text.Length;
}
