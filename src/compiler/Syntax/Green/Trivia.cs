namespace Noa.Compiler.Syntax.Green;

/// <inheritdoc cref="Syntax.Trivia"/>
internal abstract class Trivia
{
    public abstract Syntax.Trivia ToRed(int fullPosition, Syntax.Token parent);

    public abstract int GetFullWidth();
}

internal sealed class WhitespaceTrivia(string whitespace) : Trivia
{
    public string WhitespaceText { get; } = whitespace;

    public override Syntax.WhitespaceTrivia ToRed(int fullPosition, Syntax.Token parent) =>
        new(this, fullPosition, parent);

    public override int GetFullWidth() => WhitespaceText.Length;
}

internal sealed class CommentTrivia(string fullText) : Trivia
{
    public string FullText { get; } = fullText;

    public override Syntax.CommentTrivia ToRed(int fullPosition, Syntax.Token parent) =>
        new(this, fullPosition, parent);

    public override int GetFullWidth() => FullText.Length;
}

internal sealed class UnexpectedTokenTrivia(TokenKind kind, string text, int width) : Trivia
{
    public TokenKind Kind { get; } = kind;

    public string Text { get; } = text;

    public override Syntax.UnexpectedTokenTrivia ToRed(int fullPosition, Syntax.Token parent) =>
        new(this, fullPosition, parent);

    public override int GetFullWidth() => width;
}

internal sealed class SkippedTokenTrivia(TokenKind kind, string text, int widthh) : Trivia
{
    public TokenKind Kind { get; } = kind;

    public string Text { get; } = text;

    public override Syntax.SkippedTokenTrivia ToRed(int fullPosition, Syntax.Token parent) =>
        new(this, fullPosition, parent);

    public override int GetFullWidth() => throw new NotImplementedException();
}

internal sealed class UnexpectedCharacterTrivia(string character) : Trivia
{
    public string Character { get; } = character;

    public override Syntax.UnexpectedCharacterTrivia ToRed(int fullPosition, Syntax.Token parent) =>
        new(this, fullPosition, parent);

    public override int GetFullWidth() => Character.Length;
}
