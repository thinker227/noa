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
        Declare("print", ["what", "appendNewline"]);
        Declare("getInput", []);
        
        // File IO
        Declare("readFile", ["path"]);
        Declare("writeFile", ["path", "content"]);
        
        // // Lists
        // Declare("push", ["list", "value"]);
        // Declare("pop", ["list"]);
        //
        // // Functional
        // Declare("append", ["list", "value"]);
        // Declare("concat", ["list", "values"]);
        // Declare("slice", ["list", "start", "end"]);
        // Declare("map", ["list", "transform"]);
        // Declare("flatMap", ["list", "transform"]);
        // Declare("filter", ["list", "predicate"]);
        // Declare("reduce", ["list", "seed", "function"]);
        // Declare("reverse", ["list"]);
        // Declare("any", ["list", "predicate"]);
        // Declare("all", ["list", "predicate"]);
        // Declare("find", ["list", "predicate", "fromEnd"]);
        
        return scope;

        void Declare(string name, string[] parameters)
        {
            var function = new NativeFunction() { Name = name };
            foreach (var param in parameters) function.AddParameter(param);
            scope.Declare(function);
        }
    }
}
