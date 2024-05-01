namespace Noa.Compiler.Bytecode.Builders;

/// <summary>
/// A builder for a function section.
/// </summary>
public sealed class FunctionSectionBuilder : IWritable
{
    private readonly List<FunctionBuilder> functions = [];
    private uint currentId = 0;

    /// <summary>
    /// The ID of the main function.
    /// </summary>
    public FunctionId MainId { get; private set; }
    
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
        var main = builder.CreateFunction(mainNameIndex);
        builder.MainId = main.Id;

        return (builder, main);
    }

    /// <summary>
    /// Creates a new function.
    /// </summary>
    /// <param name="nameIndex">The string index of the name of the function.</param>
    /// <returns>A builder for the created function.</returns>
    public FunctionBuilder CreateFunction(StringIndex nameIndex)
    {
        var functionId = new FunctionId(currentId);
        var builder = new FunctionBuilder(functionId, nameIndex);
        currentId++;
            
        functions.Add(builder);

        return builder;
    }

    public void Write(Carpenter writer)
    {
        writer.UInt(FunctionsLength);

        foreach (var function in functions) writer.Write(function);
    }
}
