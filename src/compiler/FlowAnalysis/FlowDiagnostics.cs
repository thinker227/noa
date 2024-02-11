using Noa.Compiler.Diagnostics;

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
}
