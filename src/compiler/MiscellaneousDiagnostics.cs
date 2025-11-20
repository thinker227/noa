using Noa.Compiler.Diagnostics;
using Noa.Compiler.Symbols;

namespace Noa.Compiler;

internal static class MiscellaneousDiagnostics
{
    public static DiagnosticTemplate<Unit> TuplesUnsupported { get; } =
        DiagnosticTemplate.Create(
            "NOA-MISC-001",
            page => page
                .Keyword("Tuples")
                .Raw(" are currently ")
                .Emphasized("unsupported")
                .Raw("."),
            Severity.Error);
}
