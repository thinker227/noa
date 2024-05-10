using System.Buffers.Binary;

namespace Noa.Compiler.Bytecode.Builders;

/// <summary>
/// A builder for a code section.
/// </summary>
internal sealed class CodeBuilder(CodeBuilder? previous) : IWritable
{
    private readonly List<Instruction> instructions = [];
    private uint length = 0;
    
    /// <summary>
    /// The start address of the builder within the code section.
    /// </summary>
    public Address StartAddress => previous?.EndAddress ?? new(0);

    /// <summary>
    /// The address immediatelt after the end address of the builder within the code section.
    /// </summary>
    public Address EndAddress => new(StartAddress.Value + Length);

    public uint Length => length;

    /// <summary>
    /// The current offset from <see cref="StartAddress"/>.
    /// </summary>
    public uint AddressOffset => length;

    public void Write(Carpenter writer)
    {
        foreach (var i in instructions) writer.Write(i);
    }

    private void Add(Instruction i)
    {
        instructions.Add(i);
        length += i.Length;
    }

    private void Add(Opcode opcode)
    {
        var instruction = new Instruction(opcode);
        Add(instruction);
    }

/// <summary>
/// A wrapper around a byte array containing instruction data.
/// </summary>
/// <param name="Data">The instruction data.</param>
internal readonly record struct PlainData(byte[] Data) : IWritable
{
    public uint Length => (uint)Data.Length;

    public void Write(Carpenter writer) => writer.Bytes(Data);
}

    private void Add(Opcode opcode, byte[] data)
    {
        var writableData = new PlainData(data);
        Add(opcode, writableData);
    }

    private void Add(Opcode opcode, IWritable data)
    {
        var instruction = new Instruction(opcode) { Data = data };
        Add(instruction);
    }

    private AddressHole JumpLike(Opcode opcode)
    {
        // Setting the initial offset to 0xFFFFFFFF ensures that in case the
        // address hole isn't filled then it's immediately obvious that this occurred.
        var data = new AddressOffsetData(this, 0xFFFFFFFF);
        Add(opcode, data);
        return new(data);
    }

    public void NoOp() => Add(Opcode.NoOp);
    
    public AddressHole Jump() => JumpLike(Opcode.Jump);

    public void Jump(uint addressOffset)
    {
        var data = new AddressOffsetData(this, addressOffset);
        Add(Opcode.Jump, data);
    }
    
    public AddressHole JumpIf() => JumpLike(Opcode.JumpIf);

    public void JumpIf(uint addressOffset)
    {
        var data = new AddressOffsetData(this, addressOffset);
        Add(Opcode.JumpIf, data);
    }

    public void Call(uint argCount)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, argCount);
        Add(Opcode.Call, bytes);
    }
    
    public void Ret() => Add(Opcode.Ret);

    public void PushInt(int value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        Add(Opcode.PushInt, bytes);
    }

    public void PushBool(bool value)
    {
        var b = value ? (byte)1 : (byte)0;
        Add(Opcode.PushBool, [b]);
    }
    
    public void PushFunc(FunctionId id)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, id.Id);
        Add(Opcode.PushFunc, bytes);
    }

    public void PushNil() => Add(Opcode.PushNil);
    
    public void Pop() => Add(Opcode.Pop);

    public void Dup() => Add(Opcode.Dup);

    public void Swap() => Add(Opcode.Swap);
    
    public void StoreVar(VariableIndex varIndex)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, varIndex.Index);
        Add(Opcode.StoreVar, bytes);
    }
    
    public void LoadVar(VariableIndex varIndex)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, varIndex.Index);
        Add(Opcode.LoadVar, bytes);
    }
    
    public void Add() => Add(Opcode.Add);
    
    public void Sub() => Add(Opcode.Sub);
    
    public void Mult() => Add(Opcode.Mult);
    
    public void Div() => Add(Opcode.Div);
    
    public void Equal() => Add(Opcode.Equal);
    
    public void LessThan() => Add(Opcode.LessThan);
    
    public void Not() => Add(Opcode.Not);
    
    public void And() => Add(Opcode.And);
    
    public void Or() => Add(Opcode.Or);

    public void GreaterThan() => Add(Opcode.GreaterThan);
}

/// <summary>
/// An address in a function body.
/// </summary>
/// <param name="Value">The numeric value.</param>
internal readonly record struct Address(uint Value)
{
    public override string ToString() => $"@{Value}";
}

/// <summary>
/// A hole into which an address offset can be written.
/// </summary>
/// <param name="data">The data to write to.</param>
internal readonly struct AddressHole(AddressOffsetData data)
{
    /// <summary>
    /// Fills the hole with a specified address offset.
    /// </summary>
    /// <param name="addressOffset">The address offset to write into the hole.</param>
    public void SetAddress(uint addressOffset) =>
        data.Offset = addressOffset;
}

/// <summary>
/// A wrapper around a byte array containing instruction data.
/// </summary>
/// <param name="Data">The instruction data.</param>
internal readonly record struct PlainData(byte[] Data) : IWritable
{
    public uint Length => (uint)Data.Length;

    public void Write(Carpenter writer) => writer.Bytes(Data);
}

/// <summary>
/// An 
/// </summary>
/// <param name="builder"></param>
/// <param name="offset"></param>
internal sealed class AddressOffsetData(CodeBuilder builder, uint offset) : IWritable
{
    public uint Offset { get; set; } = offset;

    public uint Length => 4;

    public void Write(Carpenter writer) => writer.UInt(builder.StartAddress.Value + Offset);
}

/// <summary>
/// A variable index in a function.
/// </summary>
/// <param name="Index">The numeric index.</param>
internal readonly record struct VariableIndex(uint Index)
{
    public override string ToString() => $"var <{Index}>";
}
