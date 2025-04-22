using TextMappingUtils;

namespace Noa.Compiler;

/// <summary>
/// A location in source code.
/// </summary>
/// <param name="SourceName">The name of the source.</param>
/// <param name="Span">The text span of the location within the source.</param>
public readonly record struct Location(string SourceName, TextSpan Span)
{
    /// <summary>
    /// Creates a new location.
    /// </summary>
    /// <param name="sourceName">The name of the source.</param>
    /// <param name="start">The start of the location from the start of the text.</param>
    /// <param name="end">The end of the location from the start of the text.</param>
    public Location(string sourceName, int start, int end)
        : this(sourceName, new(start, end)) {}
    
    public override string ToString() => $"{Span} in '{SourceName}'";
}
