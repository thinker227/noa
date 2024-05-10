namespace Noa.Compiler.Bytecode.Builders;

/// <summary>
/// A builder for a code section.
/// </summary>
/// <param name="builders">The code builders in the section.</param>
internal sealed class CodeSectionBuilder(IEnumerable<CodeBuilder> builders) : IWritable
{
    public uint Length => (uint)builders.Sum(x => x.Length);

    public void Write(Carpenter writer)
    {
        writer.UInt(Length);

        foreach (var builder in builders) writer.Write(builder);
    }
}
