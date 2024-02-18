using Noa.Compiler.Diagnostics;
using Noa.Compiler.Symbols;

namespace Noa.Compiler.FlowAnalysis;

internal static class FlowDiagnostics
{
    public static DiagnosticTemplate ReturnOutsideFunction { get; } =
        DiagnosticTemplate.Create(
            "NOA-FLW-001",
            "Return expressions cannot be used outside functions bodies",
            Severity.Error);
    
    public static DiagnosticTemplate BreakOutsideFunction { get; } =
        DiagnosticTemplate.Create(
            "NOA-FLW-002",
            "Break expressions cannot be used outside loop blocks",
            Severity.Error);
    
    public static DiagnosticTemplate ContinueOutsideFunction { get; } =
        DiagnosticTemplate.Create(
            "NOA-FLW-003",
            "Continue expressions cannot be used outside loop blocks",
            Severity.Error);
    
    public static DiagnosticTemplate<ISymbol> AssignmentToInvalidSymbol { get; } =
        DiagnosticTemplate.Create<ISymbol>(
            "NOA-FLW-004",
            symbol => $"Cannot assign to '{symbol.Name}' because it is not a variable or parameter",
            Severity.Error);
    
    public static DiagnosticTemplate<IVariableSymbol> AssignmentToImmutableSymbol { get; } =
        DiagnosticTemplate.Create<IVariableSymbol>(
            "NOA-FLW-005",
            symbol => $"Cannot assign to '{symbol.Name}' because it is immutable",
            Severity.Error);
}
