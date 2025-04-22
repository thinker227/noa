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
    OpenBrace,
    CloseBrace,
    Semicolon,
    Comma,
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
