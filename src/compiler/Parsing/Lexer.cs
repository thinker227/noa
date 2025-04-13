using Noa.Compiler.Diagnostics;
using Noa.Compiler.Syntax.Green;
using TextMappingUtils;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing;

internal sealed partial class Lexer
{
    public static ImmutableArray<Token> Lex(
        Source source,
        CancellationToken cancellationToken)
    {
        var lexer = new Lexer(source, cancellationToken);
        lexer.Lex();
        return lexer.tokens.ToImmutable();
    }

    private void Lex()
    {
        while (!AtEnd)
        {
            // Whitespace
            if (SyntaxFacts.IsWhitespace(Current))
            {
                var whitespaceLength = 0;

                while (whitespaceLength < Rest.Length && SyntaxFacts.IsWhitespace(Rest[whitespaceLength]))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    whitespaceLength += 1;
                }

                var whitespace = Rest[..whitespaceLength].ToString();
                trivia.Add(new WhitespaceTrivia(whitespace));

                Progress(whitespaceLength);

                continue;
            }

            // Comments
            if (Get(2) is "//")
            {
                var commentLength = 2;

                while (commentLength < Rest.Length && Rest[commentLength] is not '\n')
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    commentLength += 1;
                }

                var comment = Rest[..commentLength].ToString();
                trivia.Add(new CommentTrivia(comment));

                Progress(commentLength);

                continue;
            }

            // Symbol clusters
            if (TrySymbol() is var (kind, tokenLength))
            {
                int depth;

                // Count curly depths if inside interpolation.

                if (kind is TokenKind.OpenBrace && interpolationCurlyDepths.TryPop(out depth))
                    interpolationCurlyDepths.Push(depth + 1);
                
                if (kind is TokenKind.CloseBrace && interpolationCurlyDepths.TryPop(out depth))
                {
                    interpolationCurlyDepths.Push(depth - 1);
                    
                    // Return from lexing the current string interpolation.
                    if (depth == 0) return;
                }

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

            // Strings
            if (TryString()) continue;

            // Unknown
            var unexpectedText = Rest[..1].ToString();
            
            ReportDiagnostic(
                ParseDiagnostics.UnexpectedCharacter,
                unexpectedText,
                width: 1);
            
            trivia.Add(new UnexpectedCharacterTrivia(unexpectedText));
            
            Progress(1);
        }
        
        ConstructToken(TokenKind.EndOfFile, 0);
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
            "+=" => TokenKind.PlusEquals,
            "-=" => TokenKind.DashEquals,
            "*=" => TokenKind.StarEquals,
            "/=" => TokenKind.SlashEquals,
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

        var afterDecimalPoint = false;

        var i = 0;
        for (; i < Rest.Length; i++)
        {
            // Only try to lex a decimal point if we've lexed at least one digit
            // and haven't already lexed a decimal point.
            if (i >= 1 && !afterDecimalPoint && Rest[i] is '.')
            {
                // Exit if there's no digit after the decimal point.
                if (Rest[(i + 1)..] is not [var next, ..] || !SyntaxFacts.IsDigit(next)) break;

                afterDecimalPoint = true;
                continue;
            }

            if (!SyntaxFacts.IsDigit(Rest[i])) break;
        }

        return Rest[..i];
    }

    private bool TryString()
    {
        bool isOptOut;
        switch (Rest)
        {
        case ['\\', '"', ..]:
            isOptOut = true;
            break;
        case ['"', ..]:
            isOptOut = false;
            break;
        default:
            return false;
        }

        var startQuoteLength = isOptOut ? 2 : 1;
        var interpolationStartLength = isOptOut ? 1 : 2;

        ConstructToken(TokenKind.BeginString, startQuoteLength);

        var i = 0;
        for (; i < Rest.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var current = Rest[i];

            // Begin interpolation.
            if (!isOptOut && Rest[i..] is ['\\', '{', ..] ||
                 isOptOut && Rest[i..] is ['{', ..])
            {
                // Only construct string text if there are any characters within.
                if (i > 0) ConstructToken(TokenKind.StringText, i);

                ConstructToken(TokenKind.BeginInterpolation, interpolationStartLength);

                // Push an interpolation curly depth of 0 to keep track of how deep within curly pairs the lexer is
                // and when to break out of the interpolation.
                interpolationCurlyDepths.Push(0);

                // Recursively invoke the lexer to lex the tokens within the interpolation.
                Lex();

                interpolationCurlyDepths.Pop();

                // Once the lexer has lexed the tokens within the interpolation and returned here,
                // it might be the case that the string was unterminated and that there is not closing }.
                if (!AtEnd) ConstructToken(TokenKind.EndInterpolation, 1);

                // Reset i to -1 so that the next iteration will start back at index 0 of the new rest.
                i = -1;
                continue;
            }

            // If we encounter a quote which is not preceded by a \, then we've reached the end of the string.
            if (current is '"')
            {
                // Only construct string text if there are any characters within.
                if (i > 0) ConstructToken(TokenKind.StringText, i);
                
                ConstructToken(TokenKind.EndString, 1);

                return true;
            }

            // Encountered an unterminated string, either because of a newline or end of input.
            if (current is '\n' || i == Rest.Length - 1) break;

            // Skip escape sequences.
            // An escape sequence here is considered a \ followed by any character except a newline.
            if (Rest[i..] is ['\\', not '\n', ..]) i++;
        }

        // If we got here then the string is unterminated.
        // Report a diagnostic at the very end of the string.
        ReportDiagnostic(
            ParseDiagnostics.UnterminatedString,
            width: 1);

        return true;
    }
}
