using System.Reflection;

namespace Noa.Compiler.Syntax;

internal static class ReflectionInfo<TNode> where TNode : SyntaxNode
{
    public static Type GreenElementType { get; } = Assembly
        .GetExecutingAssembly()
        .GetType($"Noa.Compiler.Syntax.Green.{typeof(TNode).Name}")!;
    
    public static Type GreenSeparatedSyntaxListType { get; } = typeof(Green.SeparatedSyntaxList<>).MakeGenericType(GreenElementType);

    public static MethodInfo GreenSeparatedSyntaxListCreate { get; } = GreenSeparatedSyntaxListType.GetMethod(
        "Create",
        BindingFlags.Public | BindingFlags.Static,
        [
            typeof(IEnumerable<Green.SyntaxNode>)
        ])!;
    
    public static Type GreenSyntaxListType { get; } = typeof(Green.SyntaxList<>).MakeGenericType(GreenElementType);

    public static Type ImmutableArrayGreenType { get; } = typeof(ImmutableArray<>).MakeGenericType(GreenElementType);

    public static MethodInfo GreenSyntaxListCreate { get; } = GreenSyntaxListType.GetMethod(
        "Create",
        BindingFlags.Public | BindingFlags.Static,
        [
            typeof(IEnumerable<Green.SyntaxNode>),
        ])!;
}
