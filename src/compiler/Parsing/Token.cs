namespace Noa.Compiler.Parsing;

internal readonly record struct Token(TokenKind Kind, string? Text, Location Location)
{
    public string Text { get; } =
        Text ?? Kind.ConstantString() ?? throw new InvalidOperationException(
            $"Cannot create a token with kind '{Kind}' without explicitly " +
            $"specifying its text because the kind does not have a constant string");

    public override string ToString() => $"{Kind} '{Text}' at {Location}";
}

internal enum TokenKind
{
    // Meta
    Error = 0,
    EndOfFile,
    
    // Symbols
    OpenParen,
    CloseParen,
    OpenBrace,
    CloseBrace,
    Semicolon,
    Comma,
    Equals,
    Plus,
    Dash,
    Star,
    Slash,
    LessThan,
    GreaterThan,
    LessThanEquals,
    GreaterThanEquals,
    EqualsGreaterThan,
    EqualsEquals,
    
    // Keywords
    Let,
    Mut,
    If,
    Else,
    Loop,
    Return,
    Break,
    Continue,
    True,
    False,
    
    // Special
    Name,
    String,
    Number,
}

internal static class TokenKindExtensions
{
    /// <summary>
    /// Returns the constant string representation of a <see cref="TokenKind"/>, or null if it does not have one.
    /// Eg., a <see cref="TokenKind.OpenParen"/> token will always have the text <c>(</c>,
    /// but a <see cref="TokenKind.Name"/> will always have a different text.
    /// </summary>
    /// <param name="kind">The kind to get the constant string of.</param>
    public static string? ConstantString(this TokenKind kind) => kind switch
    {
        TokenKind.EndOfFile => "",
        
        TokenKind.OpenParen => "(",
        TokenKind.CloseParen => ")",
        TokenKind.OpenBrace => "{",
        TokenKind.CloseBrace => "}",
        TokenKind.Comma => ",",
        TokenKind.Semicolon => ";",
        TokenKind.Equals => "=",
        TokenKind.Plus => "+",
        TokenKind.Dash => "-",
        TokenKind.Star => "*",
        TokenKind.Slash => "/",
        TokenKind.LessThan => "<",
        TokenKind.GreaterThan => ">",
        TokenKind.LessThanEquals => "<=",
        TokenKind.GreaterThanEquals => ">=",
        TokenKind.EqualsGreaterThan => "=>",
        TokenKind.EqualsEquals => "==",
        
        TokenKind.Let => "let",
        TokenKind.Mut => "mut",
        TokenKind.If => "if",
        TokenKind.Else => "else",
        TokenKind.Loop => "loop",
        TokenKind.Return => "return",
        TokenKind.Break => "break",
        TokenKind.Continue => "continue",
        TokenKind.True => "true",
        TokenKind.False => "false",
        
        _ => null
    };

    /// <summary>
    /// Gets a human-readable display string for a <see cref="TokenKind"/>.
    /// </summary>
    /// <param name="kind">The kind to get the display string for.</param>
    public static string ToDisplayString(this TokenKind kind) => kind switch
    {
        TokenKind.EndOfFile => "<end of file>",
        TokenKind.Error => "<error>",
        
        TokenKind.Name => "name",
        TokenKind.String => "string",
        TokenKind.Number => "number",
        
        _ => kind.ConstantString() ?? throw new UnreachableException()
    };
}
