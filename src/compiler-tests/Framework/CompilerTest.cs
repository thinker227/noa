using Noa.Compiler.Diagnostics;

namespace Noa.Compiler.Tests.Framework;

public sealed class CompilerTest
{
    public Source Source { get; set; }
    
    public CompilePhase Phase { get; set; }

    public List<ExpectedDiagnostic> ExpectedDiagnostics { get; } = [];

    public List<string> IgnoredCategories { get; } = [];

    public List<DiagnosticId> IgnoredDiagnostics { get; } = [];

    public static CompilerTest Create(CompilePhase phase, string source) => new()
    {
        Source = new(source, "test-input"),
        Phase = phase
    };
}

public readonly record struct ExpectedDiagnostic(DiagnosticId Id, Location Location);

public enum CompilePhase
{
    Syntax,
    Symbols,
}
