namespace Noa.Compiler;

/// <summary>
/// Represents a named piece of source text, eg. a source file.
/// </summary>
/// <param name="Text">The text of the source.</param>
/// <param name="Name">The name of the source.</param>
public readonly record struct Source(string Text, string Name);
