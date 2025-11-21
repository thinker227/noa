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
        Declare(0x2, "readFile", ["path"]);
        Declare(0x3, "writeFile", ["path", "content"]);

        // Strings
        Declare(0x4, "toString", ["x"]);
        
        // Lists
        Declare(0x5, "push", ["list", "value"]);
        Declare(0x6, "pop", ["list"]);
        Declare(0x7, "append", ["source", "value"]);
        Declare(0x8, "concat", ["source", "values"]);
        Declare(0x9, "slice", ["source", "start", "end"]);
        Declare(0xA, "map", ["source", "transform"]);
        Declare(0xB, "flatMap", ["source", "transform"]);
        Declare(0xC, "filter", ["source", "predicate"]);
        Declare(0xD, "reduce", ["source", "function", "seed"]);
        Declare(0xE, "reverse", ["source"]);
        Declare(0xF, "any", ["source", "predicate"]);
        Declare(0x10, "all", ["source", "predicate"]);
        Declare(0x11, "find", ["source", "predicate", "fromEnd"]);
        
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
