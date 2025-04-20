using Noa.Compiler.Diagnostics;
using Noa.Compiler.Syntax.Green;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing;


internal sealed partial class Lexer(Source source, CancellationToken cancellationToken)
{
    private readonly ImmutableArray<Token>.Builder tokens = ImmutableArray.CreateBuilder<Token>();
    private readonly Stack<int> interpolationCurlyDepths = [];
    private readonly List<ILexerDiagnostic> diagnosticsForNextToken = [];
    private readonly string text = source.Text;
    private int position = 0;
    private readonly ImmutableArray<Trivia>.Builder trivia = ImmutableArray.CreateBuilder<Trivia>();

    private char Current =>
        position < text.Length
            ? text[position]
            : '\0';

    private ReadOnlySpan<char> Rest => text.AsSpan(position..);

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
    
    private void ReportDiagnostic(DiagnosticTemplate<Unit> template, int width) =>
        diagnosticsForNextToken.Add(new LexerDiagnostic<Unit>(template, new Unit(), position, width));
    
    private void ReportDiagnostic<T>(DiagnosticTemplate<T> template, T arg, int width) =>
        diagnosticsForNextToken.Add(new LexerDiagnostic<T>(template, arg, position, width));
    
    private ImmutableArray<Trivia> ConsumeLeadingTrivia()
    {
        var built = trivia.ToImmutable();
        trivia.Clear();
        return built;
    }

    private void ConstructToken(TokenKind kind, int length)
    {
        var text = kind.ConstantString() ?? Rest[..length].ToString();
        var leadingTrivia = ConsumeLeadingTrivia();

        var token = new Token(kind, text, leadingTrivia, length);

        if (diagnosticsForNextToken is not [])
        {
            foreach (var diagnostic in diagnosticsForNextToken)
            {                
                token.AddDiagnostic(diagnostic.ToPartial(position));
            }
            
            diagnosticsForNextToken.Clear();
        }

        AddToken(token);

        Progress(length);
    }

    private void AddToken(Token token) => tokens.Add(token);
}
