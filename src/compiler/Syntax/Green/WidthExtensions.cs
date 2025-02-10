namespace Noa.Compiler.Syntax.Green;

internal static class WidthExtensions
{
    public static int Width<T>(this ImmutableArray<T> xs) where T : SyntaxNode =>
        xs.Sum(x => x.Width());
}
