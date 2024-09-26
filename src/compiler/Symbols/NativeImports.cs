namespace Noa.Compiler.Symbols;

/// <summary>
/// Handles native symbol imports.
/// </summary>
internal static class NativeImports
{
    /// <summary>
    /// Constructs a <see cref="ImportScope"/> containing a set of imported native symbols.
    /// </summary>
    /// <returns></returns>
    public static ImportScope GetImports()
    {
        var scope = new ImportScope();

        var print = new NativeFunction() { Name = "print" }
            .AddParameter("what")
            .AddParameter("appendNewline");
        scope.Declare(print);

        var input = new NativeFunction() { Name = "input" };
        scope.Declare(input);
        
        return scope;
    }
}
