namespace Noa.Compiler.Bytecode.Builders;

/// <summary>
/// A builder for a function.
/// </summary>
/// <param name="id">The ID of the function.</param>
/// <param name="nameIndex">The string index of the name of the function.</param>
public sealed class FunctionBuilder(FunctionId id, StringIndex nameIndex) : IWritable
{
    /// <summary>
    /// The ID of the function.
    /// </summary>
    public FunctionId Id { get; } = id;

    /// <summary>
    /// The builder for the code of the function.
    /// </summary>
    public CodeBuilder Code { get; } = new();
    
    public uint Length => Id.Length + nameIndex.Length + 4 + Code.Length;

    public void Write(Carpenter writer)
    {
        writer.Write(Id);
        writer.Write(nameIndex);
        writer.UInt(Code.Length);
        writer.Write(Code);
    }
}

/// <summary>
/// A function ID.
/// </summary>
/// <param name="Id">The numeric ID.</param>
public readonly record struct FunctionId(uint Id) : IWritable
{
    public uint Length => 4;

    public void Write(Carpenter writer) => writer.UInt(Id);

    public override string ToString() => $"func <{Id}>";
}
