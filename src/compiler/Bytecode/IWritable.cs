namespace Noa.Compiler.Bytecode;

/// <summary>
/// An element which can be written to a <see cref="Carpenter"/>.
/// </summary>
internal interface IWritable
{
    /// <summary>
    /// The byte length of the element.
    /// </summary>
    uint Length { get; }
    
    /// <summary>
    /// Writes the element to a <see cref="Carpenter"/>.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    void Write(Carpenter writer);
}

internal static class WritableExtensions
{
    /// <summary>
    /// Writes a writable element to a stream.
    /// </summary>
    /// <param name="writable">The writable element.</param>
    /// <param name="stream">The stream to write to.</param>
    public static void Write(this IWritable writable, Stream stream)
    {
        var carpenter = new Carpenter(stream);
        writable.Write(carpenter);
    }
}
