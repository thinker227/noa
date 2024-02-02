namespace Noa.Compiler.Parsing;

internal static class SyntaxFacts
{
    public static bool IsWhitespace(char c) => char.IsWhiteSpace(c);

    public static bool CanBeginName(char c) =>
        char.IsLetter(c);

    public static bool CanBeInName(char c) =>
        CanBeginName(c) || char.IsNumber(c);

    public static bool IsDigit(char c) =>
        c is >= '0' and <= '9';
}
