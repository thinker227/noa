namespace Noa.Compiler.Symbols;

/// <summary>
/// Handles native symbol imports.
/// </summary>
internal static class NativeImports
{
    /// <summary>
    /// Constructs a <see cref="ImportScope"/> containing a set of imported native symbols.
    /// </summary>
    public static ImportScope ConstructImportScope()
    {
        var scope = new ImportScope();

        // Console IO
        Declare(0x0, "print", ["what", "appendNewline"]);
        Declare(0x1, "getInput", []);
        
        // File IO
        Declare(0x80, "readFile", ["path"]);
        Declare(0x81, "writeFile", ["path", "content"]);

        // Strings
        Declare(0x100, "toString", ["x"]);
        
        // Lists
        Declare(0x180, "push", ["list", "value"]);
        Declare(0x181, "pop", ["list"]);
        Declare(0x182, "append", ["source", "value"]);
        Declare(0x183, "concat", ["source", "values"]);
        Declare(0x184, "slice", ["source", "start", "end"]);
        Declare(0x185, "map", ["source", "transform"]);
        Declare(0x186, "flatMap", ["source", "transform"]);
        Declare(0x187, "filter", ["source", "predicate"]);
        Declare(0x188, "reduce", ["source", "seed", "function"]);
        Declare(0x189, "reverse", ["source"]);
        Declare(0x18A, "any", ["source", "predicate"]);
        Declare(0x18B, "all", ["source", "predicate"]);
        Declare(0x18C, "find", ["source", "predicate", "fromEnd"]);
        
        return scope;

        void Declare(uint id, string name, ReadOnlySpan<string> parameters)
        {
            var function = new NativeFunction()
            {
                Name = name,
                Id = id
            };
            foreach (var param in parameters) function.AddParameter(param);
            scope.Declare(function);
        }
    }
}
