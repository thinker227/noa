using Noa.Compiler.Bytecode.Builders;

namespace Noa.Compiler.Bytecode;

/// <summary>
/// A writable Ark file.
/// </summary>
/// <param name="functionSection">The builder for the function section.</param>
/// <param name="stringSection">The builder for the string section.</param>
internal sealed class Ark(
    FunctionSectionBuilder functionSection,
    StringSectionBuilder stringSection)
    : IWritable
{
    public uint Length => Header.Length + functionSection.Length + stringSection.Length;

    public void Write(Carpenter writer)
    {
        var header = new Header(functionSection.MainId);
        writer.Write(header);
        writer.Write(functionSection);
        writer.Write(stringSection);
    }
}

/// <summary>
/// An Ark header.
/// </summary>
/// <param name="main">The ID of the main function.</param>
internal sealed class Header(FunctionId main) : IWritable
{
    /// <summary>
    /// The constant length of the header.
    /// </summary>
    public static uint Length => 12;
    
    /// <summary>
    /// The constant identifier.
    /// </summary>
    public static ReadOnlySpan<byte> Identifier => "totheark"u8;
    
    uint IWritable.Length => (uint)Identifier.Length + 4;

    public void Write(Carpenter writer)
    {
        writer.Bytes(Identifier);
        writer.Write(main);
    }
}
