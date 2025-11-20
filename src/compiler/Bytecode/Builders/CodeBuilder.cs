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
    /// The address immediately after the end address of the builder within the code section.
    /// </summary>
    public Address EndAddress => new(StartAddress.Value + Length);

    public uint Length => length + 1;

    /// <summary>
    /// The current offset from <see cref="StartAddress"/>.
    /// </summary>
    public uint AddressOffset => length;

    public void Write(Carpenter writer)
    {
        foreach (var i in instructions) writer.Write(i);
        writer.Opcode(Opcode.Boundary);
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

    public void NoOp() => Add(Opcode.NoOp);

    public AddressHole CreateAddressHole()
    {
        // Setting the initial offset to 0xFFFFFFFF ensures that in case the
        // address hole isn't filled then it's immediately obvious that this occurred.
        var data = new AddressOffsetData(this, 0xFFFFFFFF);
        return new(data);
    }

    private AddressHole JumpLike(Opcode opcode)
    {
        var hole = CreateAddressHole();
        Add(opcode, hole.Data);
        return new(hole.Data);
    }
    
    public AddressHole Jump() => JumpLike(Opcode.Jump);

    public void Jump(AddressOffsetData data) => Add(Opcode.Jump, data);

    public void Jump(uint addressOffset) => Jump(new AddressOffsetData(this, addressOffset));
    
    public AddressHole JumpIf() => JumpLike(Opcode.JumpIf);

    public void JumpIf(AddressOffsetData data) => Add(Opcode.JumpIf, data);

    public void JumpIf(uint addressOffset) => JumpIf(new AddressOffsetData(this, addressOffset));

    public void Call(uint argCount)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, argCount);
        Add(Opcode.Call, bytes);
    }
    
    public void Ret() => Add(Opcode.Ret);

    public void EnterTempFrame() => Add(Opcode.EnterTempFrame);

    public void ExitTempFrame() => Add(Opcode.ExitTempFrame);

    public void PushFloat(double value)
    {
        var bytes = new byte[8];
        BinaryPrimitives.WriteDoubleBigEndian(bytes, value);
        Add(Opcode.PushFloat, bytes);
    }

    public void PushBool(bool value)
    {
        var b = value ? (byte)1 : (byte)0;
        Add(Opcode.PushBool, [b]);
    }
    
    public void PushFunc(FunctionId id, IReadOnlyList<VariableIndex> captureIndices)
    {
        var captureCount = captureIndices.Count;

        var captureIndicesBytes = 4 * captureCount;
        var bytes = new byte[4 + 4 + captureIndicesBytes];
        
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(..4), id.Id);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(4..8), (uint)captureCount);
        
        for (var i = 0; i < captureCount; i++)
        {
            var index = 8 + 4 * i;
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(index, 4), captureIndices[i].Index);
        }

        Add(Opcode.PushFunc, bytes);
    }

    public void PushNil() => Add(Opcode.PushNil);

    public void PushString(StringIndex index)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, index.Index);
        Add(Opcode.PushString, bytes);
    }

    public void PushObject(bool dyn)
    {
        var b = dyn ? (byte)1 : (byte)0;
        Add(Opcode.PushObject, [b]);
    }

    public void PushList() => Add(Opcode.PushList);
    
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

    public void Concat() => Add(Opcode.Concat);

    public new void ToString() => Add(Opcode.ToString);

    public void AddField(bool mutable)
    {
        var b = mutable ? (byte)1 : (byte)0;
        Add(Opcode.AddField, [b]);
    }

    public void WriteField() => Add(Opcode.WriteField);

    public void ReadField() => Add(Opcode.ReadField);

    public void AppendElement() => Add(Opcode.AppendElement);

    public void WriteElement() => Add(Opcode.WriteElement);

    public void ReadElement() => Add(Opcode.ReadElement);
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
    /// The writable data for the address hole.
    /// </summary>
    public AddressOffsetData Data => data;
    
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
/// A mutable wrapper around an address offset.
/// </summary>
/// <param name="builder">The code builder for the data.</param>
/// <param name="offset">The initial address offset.</param>
internal sealed class AddressOffsetData(CodeBuilder builder, uint offset) : IWritable
{
    /// <summary>
    /// The address offset.
    /// </summary>
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
