namespace Noa.Compiler.Syntax;

internal static class ToRedExtensions
{
    public static SyntaxList<TRed> ToRed<TGreen, TRed>(this ImmutableArray<TGreen> green, int position, SyntaxNode parent)
        where TGreen : Green.SyntaxNode
        where TRed : SyntaxNode =>
        new(position, parent, green);

    public static SeparatedSyntaxList<TRed> ToRed<TGreen, TRed>(this Green.SeparatedSyntaxList<TGreen> green, int position, SyntaxNode parent)
        where TGreen : Green.SyntaxNode
        where TRed : SyntaxNode =>
        new(position, parent, green.Nodes, green.Tokens);
}
