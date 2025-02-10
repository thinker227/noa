namespace Noa.Compiler.Syntax.Green;

internal static class WidthExtensions
{
    public static int GetWidth<T>(this ImmutableArray<T> xs) where T : SyntaxNode =>
        xs.Sum(x => x.GetWidth());
}
