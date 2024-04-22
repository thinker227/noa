using Noa.Compiler.Diagnostics;
using Noa.Compiler.Symbols;

namespace Noa.Compiler.FlowAnalysis;

internal static class FlowDiagnostics
{
    public static DiagnosticTemplate ReturnOutsideFunction { get; } =
        DiagnosticTemplate.Create(
            "NOA-FLW-001",
            page => page
                .Keyword("Return expressions")
                .Raw(" cannot be used outside ")
                .Emphasized("function bodies"),
            Severity.Error);
    
    public static DiagnosticTemplate BreakOutsideFunction { get; } =
        DiagnosticTemplate.Create(
            "NOA-FLW-002",
            page => page
                .Keyword("Break expressions")
                .Raw(" cannot be used outside ")
                .Emphasized("loop blocks"),
            Severity.Error);
    
    public static DiagnosticTemplate ContinueOutsideFunction { get; } =
        DiagnosticTemplate.Create(
            "NOA-FLW-003",
            page => page
                .Keyword("Continue expressions")
                .Raw(" cannot be used outside ")
                .Emphasized("loop blocks"),
            Severity.Error);

    public static DiagnosticTemplate<ISymbol> AssignmentToInvalidSymbol { get; } =
        DiagnosticTemplate.Create<ISymbol>(
            "NOA-FLW-004",
            (symbol, page) => page
                .Raw("Cannot assign to ")
                .Symbol(symbol)
                .Raw(" because it is not a ")
                .Emphasized("variable")
                .Raw(" or ")
                .Emphasized("parameter"),
            Severity.Error);

    public static DiagnosticTemplate<IVariableSymbol> AssignmentToImmutableSymbol { get; } =
        DiagnosticTemplate.Create<IVariableSymbol>(
            "NOA-FLW-005",
            (symbol, page) => page
                .Raw("Cannot assign to ")
                .Symbol(symbol)
                .Raw(" because it is ")
                .Emphasized("immutable"),
            Severity.Error);
}
