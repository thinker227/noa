namespace Noa.Compiler.Parsing;

internal sealed partial class Lexer
{
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
