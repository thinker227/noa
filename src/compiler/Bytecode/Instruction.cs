namespace Noa.Compiler.Bytecode;

/// <summary>
/// An instruction in a function body.
/// </summary>
/// <param name="Opcode">The opcode of the instruction.</param>
internal readonly record struct Instruction(Opcode Opcode) : IWritable
{
    /// <summary>
    /// The additional byte data of the instruction.
    /// </summary>
    public IWritable? Data { get; init; }

    private uint DataLength => (uint)(Data?.Length ?? 0); 
    
    public uint Length => 1 + DataLength;

    public void Write(Carpenter writer)
    {
        writer.Opcode(Opcode);
        
        if (Data is not null) writer.Write(Data);
    }

    public override string ToString()
    {
        if (Data is null) return Opcode.ToString();

        var dataStr = string.Join(", ", Data);
        return $"{Opcode} [{dataStr}]";
    }
}
