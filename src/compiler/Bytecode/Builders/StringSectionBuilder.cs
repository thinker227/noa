using System.Text;

namespace Noa.Compiler.Bytecode.Builders;

/// <summary>
/// A builder for a string section.
/// </summary>
public sealed class StringSectionBuilder : IWritable
{
    private readonly List<ArkString> strings = [];
    private readonly Dictionary<string, StringIndex> indices = [];
    private uint stringsByteLength;

    public uint Length => 4 + stringsByteLength;

    /// <summary>
    /// Gets a string from a string index.
    /// </summary>
    /// <param name="index">The index into the section.</param>
    public ArkString this[StringIndex index] => strings[(int)index.Index];

    /// <summary>
    /// Gets or adds a string to the section.
    /// </summary>
    /// <param name="str">The string to get or add.</param>
    /// <returns>The index of the existing or newly added string.</returns>
    public StringIndex GetOrAdd(string str)
    {
        if (indices.TryGetValue(str, out var index)) return index;
        
        var arkString = new ArkString(str);

        stringsByteLength += arkString.Length;
        
        index = new((uint)strings.Count);
        strings.Add(arkString);
        indices.Add(str, index);

        return index;
    }

    public void Write(Carpenter writer)
    {
        writer.UInt(stringsByteLength);

        foreach (var str in strings) writer.Write(str);
    }
}

public readonly record struct ArkString(string String) : IWritable
{
    private readonly uint stringByteLength = (uint)Encoding.UTF8.GetByteCount(String);
    
    public uint Length => 4 + stringByteLength;

    public void Write(Carpenter writer)
    {
        Span<byte> bytes = new byte[stringByteLength];
        Encoding.UTF8.GetBytes(String, bytes);
            
        writer.UInt(stringByteLength);
        writer.Bytes(bytes);
    }

    public override string ToString() => String;
}

/// <summary>
/// An index into a string section.
/// </summary>
/// <param name="Index">The numeric index.</param>
public readonly record struct StringIndex(uint Index) : IWritable
{
    public uint Length => 4;

    public void Write(Carpenter writer) => writer.UInt(Index);

    public override string ToString() => $"string <{Index}>";
}
