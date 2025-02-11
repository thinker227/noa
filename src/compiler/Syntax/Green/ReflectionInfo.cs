using System.Reflection;

namespace Noa.Compiler.Syntax.Green;

internal static class ReflectionInfo<TNode> where TNode : SyntaxNode
{
    // Hack because C# doesn't have associated types.
    // Get a reference to the ConstructorInfo for the (Green.SyntaxNode, int, Syntax.SyntaxNode, IReadOnlyList<Green.SyntaxNode>)
    // constructor of Syntax.SeparatedSyntaxList<TRed> and Syntax.SyntaxList<TRed> so we can instantiate it
    // without statically knowing the type of TRed.

    public static Type RedElementType { get; } = Assembly
        .GetExecutingAssembly()
        .GetType($"Noa.Compiler.Syntax.{typeof(TNode).Name}")!;
    
    public static Type RedSeparatedSyntaxListType { get; } = typeof(Syntax.SeparatedSyntaxList<>).MakeGenericType(RedElementType);

    public static ConstructorInfo RedSeparatedSyntaxListConstructor { get; } = RedSeparatedSyntaxListType.GetConstructor(
        BindingFlags.NonPublic | BindingFlags.Instance,
        [
            typeof(int),
            typeof(Syntax.SyntaxNode),
            typeof(IReadOnlyList<Green.SyntaxNode>)
        ])!;
    
    public static Type RedSyntaxListType { get; } = typeof(Syntax.SyntaxList<>).MakeGenericType(RedElementType);

    public static ConstructorInfo RedSyntaxListConstructor { get; } = RedSyntaxListType.GetConstructor(
        BindingFlags.NonPublic | BindingFlags.Instance,
        [
            typeof(int),
            typeof(Syntax.SyntaxNode),
            typeof(IReadOnlyList<Green.SyntaxNode>)
        ])!;
}
