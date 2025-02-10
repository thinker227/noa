namespace Noa.Compiler.Syntax;

internal static class WidthExtensions
{
    public static int GetWidth<T>(this IEnumerable<T> xs) where T : Green.SyntaxNode =>
        xs.Sum(x => x.GetWidth());
}
