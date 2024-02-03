namespace Noa.Compiler.Parsing;

internal sealed partial class Lexer
{
    public static IEnumerable<Token> Lex(Source source)
    {
        var lexer = new Lexer(source);

        while (!lexer.AtEnd)
        {
            var token = lexer.NextToken();
            if (token is not null) yield return token.Value;
        }

        var endLocation = Location.FromLength(source.Name, source.Text.Length, 0);
        yield return new(TokenKind.EndOfFile, null, endLocation);
    }

    private Token? NextToken()
    {
        // Whitespace
        while (SyntaxFacts.IsWhitespace(Current)) Progress(1);

        // If there is trailing whitespace before the end of the source,
        // the previous step has eaten all the whitespace, and we need to return null
        // to avoid choking on the end.
        if (AtEnd) return null;

        // Symbol clusters
        if (TrySymbol() is var (kind, tokenLength)) return ConstructToken(kind, tokenLength);
        
        // Identifiers and keywords
        if (TryName() is { IsEmpty: false } name)
            return ConstructToken(KeywordKind(name) ?? TokenKind.Name, name.Length);

        // Numbers
        if (TryNumber() is { IsEmpty: false } number)
            return ConstructToken(TokenKind.Number, number.Length);
        
        // Unknown
        return ConstructToken(TokenKind.Error, 1);
    }

    private (TokenKind, int)? TrySymbol()
    {
        var dual = Get(2) switch
        {
            "<=" => TokenKind.LessThanEquals,
            ">=" => TokenKind.GreaterThanEquals,
            "=>" => TokenKind.EqualsGreaterThan,
            "==" => TokenKind.EqualsEquals,
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
