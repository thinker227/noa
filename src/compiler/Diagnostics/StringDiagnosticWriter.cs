using System.Text;

namespace Noa.Compiler.Diagnostics;

/// <summary>
/// An <see cref="IDiagnosticWriter"/> which writes diagnostic messages
/// as simple strings with no special formatting.
/// </summary>
public sealed class StringDiagnosticWriter : IDiagnosticWriter<string>
{
    /// <summary>
    /// The singleton <see cref="StringDiagnosticWriter"/>.
    /// </summary>
    public static StringDiagnosticWriter Writer { get; } = new();
    
    private StringDiagnosticWriter() {}
    
    public IDiagnosticPage<string> CreatePage(IDiagnostic diagnostic, Ast ast) => new StringPage();
}

file sealed class StringPage : IDiagnosticPage<string>
{
    private readonly StringBuilder builder = new();
    
    public IDiagnosticPage Raw(string text)
    {
        builder.Append(text);
        return this;
    }

    public string Write() => builder.ToString();
}
