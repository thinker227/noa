using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;
using Noa.Compiler.Text;

namespace Noa.Compiler.Parsing;

internal sealed partial class Lexer
{
    public static (ImmutableArray<Token>, IReadOnlyCollection<IDiagnostic>) Lex(
        Source source,
        CancellationToken cancellationToken)
    {
        var lexer = new Lexer(source, cancellationToken);
        lexer.Lex();
        return (lexer.tokens.ToImmutable(), lexer.diagnostics);
    }

    private void Lex()
    {
        while (!AtEnd)
        {
            // Whitespace
            while (SyntaxFacts.IsWhitespace(Current))
            {
                cancellationToken.ThrowIfCancellationRequested();

                Progress(1);
            }

            // Comments
            if (Get(2) is "//")
            {
                while (!AtEnd && Current is not '\n')
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Progress(1);
                }

                continue;
            }

            // If there is trailing whitespace before the end of the source,
            // the previous step has eaten all the whitespace, and we need to break
            // to avoid choking on the end.
            if (AtEnd) break;

            // Symbol clusters
            if (TrySymbol() is var (kind, tokenLength))
            {
                ConstructToken(kind, tokenLength);
                continue;
            }

            // Identifiers and keywords
            if (TryName() is { IsEmpty: false } name)
            {
                ConstructToken(KeywordKind(name) ?? TokenKind.Name, name.Length);
                continue;
            }

            // Numbers
            if (TryNumber() is { IsEmpty: false } number)
            {
                ConstructToken(TokenKind.Number, number.Length);
                continue;
            }

            // Unknown
            var unexpectedSpan = TextSpan.FromLength(position, 1);
            var unexpectedToken = new Token(TokenKind.Error, Rest[..1].ToString(), unexpectedSpan);
            diagnostics.Add(ParseDiagnostics.UnexpectedToken.Format(unexpectedToken, new(source.Name, unexpectedSpan)));
            Progress(1);
        }
        
        var endSpan = TextSpan.FromLength(source.Text.Length, 0);
        AddToken(new(TokenKind.EndOfFile, null, endSpan));
    }

    private (TokenKind, int)? TrySymbol()
    {
        var dual = Get(2) switch
        {
            "<=" => TokenKind.LessThanEquals,
            ">=" => TokenKind.GreaterThanEquals,
            "=>" => TokenKind.EqualsGreaterThan,
            "==" => TokenKind.EqualsEquals,
            "!=" => TokenKind.BangEquals,
            _ => null as TokenKind?
        };

        if (dual is not null) return (dual.Value, 2);

        var single = Get(1) switch
        {
            "(" => TokenKind.OpenParen,
            ")" => TokenKind.CloseParen,
            "{" => TokenKind.OpenBrace,
            "}" => TokenKind.CloseBrace,
            "," => TokenKind.Comma,
            ";" => TokenKind.Semicolon,
            "=" => TokenKind.Equals,
            "!" => TokenKind.Bang,
            "+" => TokenKind.Plus,
            "-" => TokenKind.Dash,
            "*" => TokenKind.Star,
            "/" => TokenKind.Slash,
            "<" => TokenKind.LessThan,
            ">" => TokenKind.GreaterThan,
            _ => null as TokenKind?
        };

        if (single is not null) return (single.Value, 1);

        return null;
    }

    private ReadOnlySpan<char> TryName()
    {
        if (Rest.IsEmpty || !SyntaxFacts.CanBeginName(Rest[0])) return ReadOnlySpan<char>.Empty;

        var i = 1;
        for (; i < Rest.Length; i++)
        {
            if (!SyntaxFacts.CanBeInName(Rest[i])) break;
        }

        return Rest[..i];
    }

    private static TokenKind? KeywordKind(ReadOnlySpan<char> name) => name switch
    {
        "func" => TokenKind.Func,
        "let" => TokenKind.Let,
        "mut" => TokenKind.Mut,
        "if" => TokenKind.If,
        "else" => TokenKind.Else,
        "loop" => TokenKind.Loop,
        "return" => TokenKind.Return,
        "break" => TokenKind.Break,
        "continue" => TokenKind.Continue,
        "true" => TokenKind.True,
        "false" => TokenKind.False,
        _ => null
    };

    private ReadOnlySpan<char> TryNumber()
    {
        if (Rest.IsEmpty) return ReadOnlySpan<char>.Empty;

        var i = 0;
        for (; i < Rest.Length; i++)
        {
            if (!SyntaxFacts.IsDigit(Rest[i])) break;
        }

        return Rest[..i];
    }
}
