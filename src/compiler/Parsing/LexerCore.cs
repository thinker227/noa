namespace Noa.Compiler.Parsing;

internal sealed partial class Lexer(Source source, CancellationToken cancellationToken)
{
    private readonly string text = source.Text;
    private int position = 0;

    private char Current =>
        position < text.Length
            ? text[position]
            : '\0';

    private ReadOnlySpan<char> Rest => text.AsSpan(position);

    private bool AtEnd => position >= text.Length;
    
    private ReadOnlySpan<char> Progress(int length)
    {
        var span = Get(length);
        position += length;
        return span;
    }

    private ReadOnlySpan<char> Get(int length, int from = 0) =>
        from + length <= Rest.Length
            ? Rest.Slice(from, length)
            : [];

    private Token ConstructToken(TokenKind kind, int length)
    {
        var location = Location.FromLength(source.Name, position, length);
        var text = kind.ConstantString() ?? Rest[..length].ToString();
        
        Progress(length);

        return new(kind, text, location);
    }
}
