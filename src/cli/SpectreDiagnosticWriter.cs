using System.Text;
using Noa.Compiler.Diagnostics;
using Noa.Compiler.Symbols;

namespace Noa.Cli;

public sealed class SpectreDiagnosticWriter : IDiagnosticWriter<string>
{
    public static SpectreDiagnosticWriter Writer { get; } = new();
    
    private SpectreDiagnosticWriter() {}
    
    public IDiagnosticPage<string> CreatePage(IDiagnostic diagnostic) => new SpectreDiagnosticPage();
}

file sealed class SpectreDiagnosticPage : IDiagnosticPage<string>
{
    private readonly StringBuilder builder = new();

    public IDiagnosticPage Raw(string text)
    {
        builder.Append(text);
        return this;
    }

    public IDiagnosticPage Emphasized(string text)
    {
        builder.Append($"[italic]{text}[/]");
        return this;
    }

    public IDiagnosticPage Source(string source)
    {
        builder.Append($"[white]{source}[/]");
        return this;
    }

    public IDiagnosticPage Keyword(string keyword)
    {
        builder.Append($"[fuchsia]{keyword}[/]");
        return this;
    }

    public IDiagnosticPage Name(string name)
    {
        builder.Append($"[olive]{name}[/]");
        return this;
    }

    public IDiagnosticPage Symbol(ISymbol symbol) => Name(symbol.Name);

    public string Write() => builder.ToString();
}
