namespace Noa.Compiler.Syntax;

/// <summary>
/// The kind of a syntax token.
/// </summary>
public enum TokenKind
{
    // Meta
    Error = 0,
    EndOfFile,
    
    // Symbols
    OpenParen,
    CloseParen,
    OpenBracket,
    CloseBracket,
    OpenBrace,
    CloseBrace,
    Comma,
    Dot,
    Colon,
    Semicolon,
    Equals,
    Bang,
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
    BangEquals,
    PlusEquals,
    DashEquals,
    StarEquals,
    SlashEquals,
    
    // Keywords
    Func,
    Let,
    Mut,
    Dyn,
    If,
    Else,
    Loop,
    Return,
    Break,
    Continue,
    True,
    False,
    Not,
    Or,
    And,

    // Strings
    BeginString,
    EndString,
    BeginInterpolation,
    EndInterpolation,
    StringText,
    
    // Special
    Name,
    Number,
}
