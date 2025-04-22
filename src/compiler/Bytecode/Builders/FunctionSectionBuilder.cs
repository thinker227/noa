namespace Noa.Compiler.Bytecode.Builders;

/// <summary>
/// A builder for a function section.
/// </summary>
internal sealed class FunctionSectionBuilder : IWritable
{
    private readonly List<FunctionBuilder> functions = [];
    private uint currentId = 0;

    /// <summary>
    /// The builder for the main function.
    /// </summary>
    public FunctionBuilder Main { get; private set; } = null!;
    
    private uint FunctionsLength => (uint)functions.Sum(f => f.Length); 

    public uint Length => 4 + FunctionsLength;

    private FunctionSectionBuilder() {}

    /// <summary>
    /// Creates a new builder.
    /// </summary>
    /// <param name="mainNameIndex">The name index of the name of the main function.</param>
    /// <returns>
    /// The created <see cref="FunctionSectionBuilder"/>
    /// as well as a <see cref="FunctionBuilder"/> for the main function.
    /// </returns>
    public static (FunctionSectionBuilder builder, FunctionBuilder main) Create(StringIndex mainNameIndex)
    {
        var builder = new FunctionSectionBuilder();
        var main = builder.CreateFunction(mainNameIndex, 0);
        builder.Main = main;

        return (builder, main);
    }

    /// <summary>
    /// Creates a new function.
    /// </summary>
    /// <param name="nameIndex">The string index of the name of the function.</param>
    /// <param name="arity">The arity of the function.</param>
    /// <returns>A builder for the created function.</returns>
    public FunctionBuilder CreateFunction(StringIndex nameIndex, uint arity)
    {
        var previous = functions.Count > 0
            ? functions[^1].Code
            : null;
        var code = new CodeBuilder(previous);

        var functionId = new FunctionId(currentId);
        
        var builder = new FunctionBuilder(code, functionId, nameIndex, arity);
        currentId++;
            
        functions.Add(builder);

        return builder;
    }

    public CodeSectionBuilder CreateCodeSection()
    {
        var codeBuilders = functions.Select(x => x.Code);
        return new(codeBuilders);
    }

    public void Write(Carpenter writer)
    {
        writer.UInt((uint)functions.Count);

        foreach (var function in functions) writer.Write(function);
    }
}
