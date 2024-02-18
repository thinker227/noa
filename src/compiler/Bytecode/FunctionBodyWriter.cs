namespace Noa.Compiler.Bytecode;

/// <summary>
/// Writes function bodies.
/// </summary>
/// <param name="writer">The base binary writer to write to.</param>
internal sealed class FunctionBodyWriter(BinaryWriter writer)
{
    private readonly int streamBasePosition = (int)writer.BaseStream.Position;

    /// <summary>
    /// The address of the current instruction.
    /// </summary>
    public Address Address => new((int)writer.BaseStream.Position - streamBasePosition);

    private void WriteAt(int position, Action write)
    {
        var original = (int)writer.BaseStream.Position;
        
        writer.BaseStream.Position = position;
        write();
        writer.BaseStream.Position = original;
    }
    
    private void Code(OpCode code) => writer.Write((byte)code);

    public void NoOp() => Code(OpCode.NoOp);

    private AddressHole JumpLike(OpCode opCode)
    {
        Code(opCode);
        writer.Write(0);

        var writePosition = (int)writer.BaseStream.Position;

        return new(address =>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(address.Value);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(address.Value, writer.BaseStream.Position);
            
            WriteAt(writePosition, () =>
            {
                writer.Write(address.Value);
            });
        });
    }

    public AddressHole Jump() => JumpLike(OpCode.Jump);

    public AddressHole JumpIf() => JumpLike(OpCode.JumpIf);

    public void Call(int argCount)
    {
        Code(OpCode.Call);
        writer.Write(argCount);
    }
    
    public void Ret() => Code(OpCode.Ret);

    public void PushInt(int int32)
    {
        Code(OpCode.PushInt);
        writer.Write(int32);
    }

    public void PushBool(bool @bool)
    {
        Code(OpCode.PushBool);
        writer.Write(@bool);
    }
    
    public void Pop() => Code(OpCode.Pop);
    
    public void Dup() => Code(OpCode.Dup);

    public void StoreVar(VariableIndex varIndex)
    {
        Code(OpCode.StoreVar);
        writer.Write(varIndex.Index);
    }
    
    public void LoadVar(VariableIndex varIndex)
    {
        Code(OpCode.LoadVar);
        writer.Write(varIndex.Index);
    }
    
    public void Add() => Code(OpCode.Add);
    
    public void Sub() => Code(OpCode.Sub);
    
    public void Mult() => Code(OpCode.Mult);
    
    public void Div() => Code(OpCode.Div);
    
    public void Equal() => Code(OpCode.Equal);
    
    public void LessThan() => Code(OpCode.LessThan);
    
    public void Not() => Code(OpCode.Not);
    
    public void And() => Code(OpCode.And);
    
    public void Or() => Code(OpCode.Or);
}

/// <summary>
/// A hole which can be filled with an address value.
/// </summary>
/// <param name="onFinal">An action which is invoked when the hole is finished.</param>
internal struct AddressHole(Action<Address> onFinal) : IDisposable
{
    /// <summary>
    /// The address the hole should be filled with.
    /// </summary>
    public Address? Address { get; set; } = null;

    public void Dispose()
    {
        if (Address is not { } address)
            throw new InvalidOperationException("Disposing address hole without address being set.");

        onFinal(address);
    }
}

/// <summary>
/// The index of a variable or parameter.
/// </summary>
internal readonly record struct VariableIndex(int Index);

/// <summary>
/// The address of an instruction in a function body.
/// </summary>
internal readonly record struct Address(int Value);
