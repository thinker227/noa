namespace Noa.Compiler.Bytecode.Builders;

/// <summary>
/// A builder for a function.
/// </summary>
/// <param name="id">The ID of the function.</param>
/// <param name="nameIndex">The string index of the name of the function.</param>
internal sealed class FunctionBuilder(CodeBuilder code, FunctionId id, StringIndex nameIndex, uint arity) : IWritable
{
    /// <summary>
    /// The ID of the function.
    /// </summary>
    public FunctionId Id { get; } = id;

    /// <summary>
    /// The start address of the function within the code section.
    /// </summary>
    public Address Address => Code.StartAddress;

    /// <summary>
    /// The builder for the code of the function.
    /// </summary>
    public CodeBuilder Code { get; } = code;

    public LocalsInator Locals { get; } = new(arity);
    
    public uint Length => 4 + 4 + 4 + 4 + 4;

    public void Write(Carpenter writer)
    {
        writer.Write(Id);
        writer.Write(nameIndex);
        writer.UInt(Locals.Parameters);
        writer.UInt(Locals.Variables);
        writer.UInt(Address.Value);
    }

    /// <summary>
    /// Implicitly converts a function builder to a function ID.
    /// </summary>
    /// <param name="builder">The builder to convert.</param>
    public static implicit operator FunctionId(FunctionBuilder builder) => builder.Id;
}

/// <summary>
/// A function ID.
/// </summary>
/// <param name="Id">The numeric ID.</param>
internal readonly record struct FunctionId(uint Id) : IWritable
{
    public uint Length => 4;

    public void Write(Carpenter writer) => writer.UInt(Id);

    public override string ToString() => $"func <{Id}>";
}
