using System.Buffers.Binary;

namespace Noa.Compiler.Bytecode.Builders;

/// <summary>
/// A builder for the code of a function.
/// </summary>
internal sealed class CodeBuilder : IWritable
{
    private readonly List<Instruction> instructions = [];
    private uint currentAddress = 0;
    private uint length = 0;

    public uint Length => length;

    /// <summary>
    /// The current address of the builder.
    /// </summary>
    public Address CurrentAddress => new(currentAddress);

    public void Write(Carpenter writer)
    {
        foreach (var i in instructions) writer.Write(i);
    }

    private void Add(Instruction i)
    {
        instructions.Add(i);
        currentAddress++;
        length += i.Length;
    }

    private void Add(Opcode opcode)
    {
        var instruction = new Instruction(opcode);
        Add(instruction);
    }

    private void Add(Opcode opcode, byte[] data)
    {
        var instruction = new Instruction(opcode) { Data = data };
        Add(instruction);
    }

    private AddressHole JumpLike(Opcode opcode)
    {
        // Setting the initial data to 0xFFFFFFFF ensures that in case the
        // address hole isn't filled then it's immediately obvious that this occurred.
        var data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
        
        Add(opcode, data);

        return new(data);
    }

    public void NoOp() => Add(Opcode.NoOp);
    
    public AddressHole Jump() => JumpLike(Opcode.Jump);

    public void Jump(Address address)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, address.Value);
        Add(Opcode.Jump, bytes);
    }
    
    public AddressHole JumpIf() => JumpLike(Opcode.JumpIf);

    public void JumpIf(Address address)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, address.Value);
        Add(Opcode.JumpIf, bytes);
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
/// A hole into which an address can be written.
/// </summary>
/// <param name="bytes">The bytes to write the address to.</param>
internal readonly struct AddressHole(byte[] bytes)
{
    /// <summary>
    /// Fills the hole with a specified address.
    /// </summary>
    /// <param name="address"></param>
    public void SetAddress(Address address) =>
        BinaryPrimitives.WriteUInt32BigEndian(bytes, address.Value);
}

/// <summary>
/// A variable index in a function.
/// </summary>
/// <param name="Index">The numeric index.</param>
internal readonly record struct VariableIndex(uint Index)
{
    public override string ToString() => $"var <{Index}>";
}
