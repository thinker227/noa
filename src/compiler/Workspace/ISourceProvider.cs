namespace Noa.Compiler.Workspace;

/// <summary>
/// A provider for source code.
/// </summary>
public interface ISourceProvider : IEquatable<ISourceProvider>
{
    /// <summary>
    /// Gets the source code from the provider.
    /// </summary>
    Source GetSource();
}

/// <summary>
/// A source code provider which reads its source from a file.
/// </summary>
/// <param name="SourceFile">The source file to read the source code from.</param>
/// <param name="DisplayPath">
/// An optional display path for the file, separate from the file path itself.
/// The display path is not included in equality comparisons.
/// </param>
public readonly record struct FileSourceProvider(FileInfo SourceFile, string? DisplayPath = null) : ISourceProvider
{
    public Source GetSource()
    {
        var text = File.ReadAllText(SourceFile.FullName);

        return new(text, DisplayPath ?? SourceFile.Name);
    }

    public bool Equals(FileSourceProvider other) =>
        other.SourceFile.FullName == SourceFile.FullName;

    public bool Equals(ISourceProvider? other) =>
        other is FileSourceProvider fileSourceProvider &&
        Equals(fileSourceProvider);

    public override int GetHashCode() =>
        SourceFile.FullName.GetHashCode();
}

/// <summary>
/// An ad-hoc source provider which provides arbitrary source texts.
/// </summary>
/// <param name="Text">The source code as text.</param>
/// <param name="Name">The name of the source.</param>
public readonly record struct AdhocSourceProvider(string Text, string Name) : ISourceProvider
{
    public Source GetSource() =>
        new(Text, Name);

    public bool Equals(AdhocSourceProvider other) =>
        other.Text == Text && other.Name == Name;

    public bool Equals(ISourceProvider? other) =>
        other is AdhocSourceProvider adhocSourceProvider &&
        Equals(adhocSourceProvider);

    public override int GetHashCode() =>
        HashCode.Combine(Text, Name);
}
