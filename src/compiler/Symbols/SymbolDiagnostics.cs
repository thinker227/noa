using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols;

internal static class SymbolDiagnostics
{
    public static DiagnosticTemplate<FunctionSymbol> FunctionAlreadyDeclared { get; } =
        DiagnosticTemplate.Create<FunctionSymbol>(
            "NOA-SYM-001",
            function => $"Function '{function.Name}' has already been declared in this scope. " +
                        $"Functions cannot shadow other functions in the same scope",
            Severity.Error);

    public static DiagnosticTemplate<string> SymbolAlreadyDeclared { get; } =
        DiagnosticTemplate.Create<string>(
            "NOA-SYM-002",
            name => $"A symbol with the name '{name}' has already been declared in this scope",
            Severity.Error);
    
    public static DiagnosticTemplate<(VariableSymbol, FunctionSymbol)> VariableShadowsFunction { get; } =
        DiagnosticTemplate.Create<(VariableSymbol var, FunctionSymbol func)>(
            "NOA-SYM-003",
            arg => $"Variable '{arg.var.Name}' shadows function '{arg.func.Name}'. " +
                   $"Variables cannot shadow functions",
            Severity.Error);
    
    public static DiagnosticTemplate<(string, IScope, Node)> SymbolCannotBeFound { get; } =
        DiagnosticTemplate.Create<(string name, IScope scope, Node at)>(
            "NOA-SYM-004",
            arg =>
            {
                var (name, scope, at) = arg;
                var corrections = LookupCorrection.FindPossibleCorrections(name, scope, at);

                if (corrections.Count == 0) return $"Symbol '{name}' cannot be found in the current scope";

                var correctionsString = Formatting.JoinOxfordOr(corrections
                    .Select(s => $"'{s.Name}'"));

                return $"Symbol '{name}' cannot be found in the current scope. " +
                       $"Did you perhaps mean {correctionsString}?";
            },
            Severity.Error);
    
    public static DiagnosticTemplate<ISymbol> BlockedByFunction { get; } =
        DiagnosticTemplate.Create<ISymbol>(
            "NOA-SYM-005",
            symbol => $"Cannot reference variable or parameter '{symbol.Name}' inside function body. " +
                      $"Functions cannot reference variables or parameters from their containing scope",
            Severity.Error);
    
    public static DiagnosticTemplate<ISymbol> DeclaredLater { get; } =
        DiagnosticTemplate.Create<ISymbol>(
            "NOA-SYM-006",
            symbol => $"Cannot reference variable '{symbol.Name}' because it has not yet been declared",
            Severity.Error);
}
