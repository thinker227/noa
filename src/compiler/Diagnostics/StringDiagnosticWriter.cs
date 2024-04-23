using System.Text;
using Noa.Compiler.Symbols;

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
    
    public IDiagnosticPage<string> CreatePage(IDiagnostic diagnostic) => new StringPage();
}

file sealed class StringPage : IDiagnosticPage<string>
{
    private readonly StringBuilder builder = new();
    
    public IDiagnosticPage Raw(string text)
    {
        builder.Append(text);
        return this;
    }

    public IDiagnosticPage Name(string name)
    {
        builder.Append($"'{name}'");
        return this;
    }

    public IDiagnosticPage Source(string source)
    {
        builder.Append($"'{source}'");
        return this;
    }

    public IDiagnosticPage Symbol(ISymbol symbol)
    {
        builder.Append($"'{symbol.Name}'");
        return this;
    }

    public string Write() => builder.ToString();
}
