namespace Noa.Compiler;

/// <summary>
/// A location in source code.
/// </summary>
/// <param name="SourceName">The name of the source.</param>
/// <param name="Start">The start position in the source.</param>
/// <param name="End">The end position in the source.</param>
public readonly struct Location(string SourceName, int Start, int End)
{
    /// <summary>
    /// The length of the location in the source.
    /// </summary>
    public int Length => End - Start;

    /// <summary>
    /// Creates a new location from a start position and length.
    /// </summary>
    /// <param name="sourceName">The name of the source.</param>
    /// <param name="start">The start position in the source.</param>
    /// <param name="length">The length of the location in source.</param>
    public static Location FromLength(string sourceName, int start, int length) =>
        new(sourceName, start, start + length);

    public override string ToString() => $"{Start} to {End} in '{SourceName}'";
}
