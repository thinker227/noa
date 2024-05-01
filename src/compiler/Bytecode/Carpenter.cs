using System.Buffers;
using System.Buffers.Binary;

namespace Noa.Compiler.Bytecode;

/// <summary>
/// Assembles ark( file)s.
/// </summary>
/// <param name="stream">The stream to emit bytes to.</param>
public sealed class Carpenter(Stream stream)
{
    /// <summary>
    /// Writes an <see cref="IWritable"/> element.
    /// </summary>
    /// <param name="writable">The element to write.</param>
    public void Write(IWritable writable) =>
        writable.Write(this);

    /// <summary>
    /// Writes a sequence of bytes.
    /// </summary>
    /// <param name="bytes">The bytes to write.</param>
    public void Bytes(ReadOnlySpan<byte> bytes) =>
        stream.Write(bytes);

    private void Write4ByteValue<T>(SpanAction<byte, T> action, T value)
    {
        Span<byte> bytes = stackalloc byte[4];
        action(bytes, value);
        Bytes(bytes);
    }

    /// <summary>
    /// Writes an opcode.
    /// </summary>
    /// <param name="code">The opcode to write.</param>
    public void Opcode(Opcode code) =>
        stream.WriteByte((byte)code);

    /// <summary>
    /// Writes a signed 32-bit integer.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void Int(int value) =>
        Write4ByteValue(BinaryPrimitives.WriteInt32BigEndian, value);

    /// <summary>
    /// Writes an unsigned 32-bit integer.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void UInt(uint value) =>
        Write4ByteValue(BinaryPrimitives.WriteUInt32BigEndian, value);

    /// <summary>
    /// Writes a 1-byte boolean.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void Bool(bool value) =>
        stream.WriteByte(value ? (byte)1 : (byte)0);
}
