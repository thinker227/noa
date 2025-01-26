using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;
using TextMappingUtils;

namespace Noa.Compiler.Parsing;

internal sealed partial class Lexer(Source source, CancellationToken cancellationToken)
{
    private readonly ImmutableArray<Token>.Builder tokens = ImmutableArray.CreateBuilder<Token>();
    private readonly List<IDiagnostic> diagnostics = [];
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

    private void ConstructToken(TokenKind kind, int length)
    {
        var span = TextSpan.FromLength(position, length);
        var text = kind.ConstantString() ?? Rest[..length].ToString();
        
        Progress(length);

        AddToken(new(kind, text, span));
    }

    private void AddToken(Token token) => tokens.Add(token);
}
