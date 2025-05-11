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
        Declare("print", ["what", "appendNewline"], 0x0);
        Declare("getInput", [], 0x1);
        
        // File IO
        Declare("readFile", ["path"], 0x2);
        Declare("writeFile", ["path", "content"], 0x3);

        // Strings
        Declare("toString", ["x"], 0x4);
        
        // Lists
        Declare("push", ["list", "value"], 0x5);
        Declare("pop", ["list"], 0x6);
        Declare("append", ["source", "value"], 0x7);
        Declare("concat", ["source", "values"], 0x8);
        Declare("slice", ["source", "start", "end"], 0x9);
        Declare("map", ["source", "transform"], 0xA);
        Declare("flatMap", ["source", "transform"], 0xB);
        Declare("filter", ["source", "predicate"], 0xC);
        Declare("reduce", ["source", "function", "seed"], 0xD);
        Declare("reverse", ["source"], 0xE);
        Declare("any", ["source", "predicate"], 0xF);
        Declare("all", ["source", "predicate"], 0x10);
        Declare("find", ["source", "predicate", "fromEnd"], 0x11);
        
        return scope;

        void Declare(string name, ReadOnlySpan<string> parameters, uint id)
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
