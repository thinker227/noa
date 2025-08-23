using Noa.Compiler.Syntax;

namespace Noa.Compiler.Parsing;

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
        TokenKind.Dot => ".",
        TokenKind.Colon => ":",
        TokenKind.Semicolon => ";",
        TokenKind.Equals => "=",
        TokenKind.Bang => "!",
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
        TokenKind.BangEquals => "!=",
        TokenKind.PlusEquals => "+=",
        TokenKind.DashEquals => "-=",
        TokenKind.StarEquals => "*=",
        TokenKind.SlashEquals => "/=",
        
        TokenKind.Func => "func",
        TokenKind.Let => "let",
        TokenKind.Mut => "mut",
        TokenKind.Dyn => "dyn",
        TokenKind.If => "if",
        TokenKind.Else => "else",
        TokenKind.Loop => "loop",
        TokenKind.Return => "return",
        TokenKind.Break => "break",
        TokenKind.Continue => "continue",
        TokenKind.True => "true",
        TokenKind.False => "false",
        TokenKind.Not => "not",
        TokenKind.Or => "or",
        TokenKind.And => "and",

        TokenKind.BeginString => "\"",
        TokenKind.EndString => "\"",
        TokenKind.BeginInterpolation => "{",
        TokenKind.EndInterpolation => "}",
        
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

        TokenKind.StringText => "string text",
        
        TokenKind.Name => "name",
        TokenKind.Number => "number",
        
        _ => kind.ConstantString() ?? throw new UnreachableException()
    };

    public static AssignmentKind? ToAssignmentKind(this TokenKind kind) => kind switch
    {
        TokenKind.Equals => AssignmentKind.Assign,
        TokenKind.PlusEquals => AssignmentKind.Plus,
        TokenKind.DashEquals => AssignmentKind.Minus,
        TokenKind.StarEquals => AssignmentKind.Mult,
        TokenKind.SlashEquals => AssignmentKind.Div,
        _ => null
    };
    
    /// <summary>
    /// Tries to convert a <see cref="TokenKind"/> into a <see cref="UnaryKind"/>.
    /// Returns null if the token kind cannot be converted.
    /// </summary>
    /// <param name="kind">The kind to try convert.</param>
    public static UnaryKind? ToUnaryKind(this TokenKind kind) => kind switch
    {
        TokenKind.Plus => UnaryKind.Identity,
        TokenKind.Dash => UnaryKind.Negate,
        TokenKind.Not => UnaryKind.Not,
        _ => null
    };

    /// <summary>
    /// Tries to convert a <see cref="TokenKind"/> into a <see cref="BinaryKind"/>.
    /// Returns null if the token kind cannot be converted.
    /// </summary>
    /// <param name="kind">The kind to try convert.</param>
    public static BinaryKind? ToBinaryKind(this TokenKind kind) => kind switch
    {
        TokenKind.Plus => BinaryKind.Plus,
        TokenKind.Dash => BinaryKind.Minus,
        TokenKind.Star => BinaryKind.Mult,
        TokenKind.Slash => BinaryKind.Div,
        TokenKind.EqualsEquals => BinaryKind.Equal,
        TokenKind.BangEquals => BinaryKind.NotEqual,
        TokenKind.LessThan => BinaryKind.LessThan,
        TokenKind.GreaterThan => BinaryKind.GreaterThan,
        TokenKind.LessThanEquals => BinaryKind.LessThanOrEqual,
        TokenKind.GreaterThanEquals => BinaryKind.GreaterThanOrEqual,
        TokenKind.Or => BinaryKind.Or,
        TokenKind.And => BinaryKind.And,
        _ => null
    };

    /// <summary>
    /// Whether the token is invisible, i.e. does not consist of any text.
    /// This is only the case for <see cref="TokenKind.Error"/> and <see cref="TokenKind.EndOfFile"/>.
    /// </summary>
    public static bool IsInvisible(this TokenKind kind) =>
        kind is TokenKind.Error or TokenKind.EndOfFile;
}
