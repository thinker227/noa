using Noa.Compiler.Diagnostics;
using Noa.Compiler.Symbols;

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

    public static DiagnosticTemplate<VariableSymbol> ClosuresUnsupported { get; } =
        DiagnosticTemplate.Create<VariableSymbol>(
            "NOA-MISC-002",
            (symbol, page) => page
                .Raw("Cannot reference variable ")
                .Symbol(symbol)
                .Raw(" because it would cause a ")
                .Keyword("closure")
                .Raw(" to be created. ")
                .Keyword("Closures")
                .Raw(" are currently ")
                .Emphasized("unsupported")
                .Raw("."),
            Severity.Error);
}
