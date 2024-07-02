using Noa.Compiler.Diagnostics;

namespace Noa.Compiler;

internal static class MiscellaneousDiagnostics
{
    public static DiagnosticTemplate TuplesUnsupported { get; } =
        DiagnosticTemplate.Create(
            "NOA-MISC-001",
            page => page
                .Keyword("Tuples")
                .Raw(" are currently ")
                .Emphasized("unsupported")
                .Raw("."),
            Severity.Error);
}
