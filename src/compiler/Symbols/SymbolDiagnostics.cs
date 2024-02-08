namespace Noa.Compiler.Symbols;

internal static class SymbolDiagnostics
{
    public static DiagnosticTemplate<FunctionSymbol> FunctionAlreadyDeclared { get; } =
        DiagnosticTemplate.Create<FunctionSymbol>(
            function => $"Function '{function.Name}' has already been declared in this scope. " +
                        $"Functions cannot shadow other functions in the same scope",
            Severity.Error);

    public static DiagnosticTemplate<string> SymbolAlreadyDeclared { get; } =
        DiagnosticTemplate.Create<string>(
            name => $"A symbol with the name '{name}' has already been declared in this scope",
            Severity.Error);
    
    public static DiagnosticTemplate<(VariableSymbol, FunctionSymbol)> VariableShadowsFunction { get; } =
        DiagnosticTemplate.Create<(VariableSymbol var, FunctionSymbol func)>(
            arg => $"Variable '{arg.var.Name}' shadows function '{arg.func.Name}'. " +
                   $"Variables cannot shadow functions",
            Severity.Error);
    
    public static DiagnosticTemplate<string> SymbolCannotBeFound { get; } =
        DiagnosticTemplate.Create<string>(
            name => $"Symbol '{name}' cannot be found in the current scope",
            Severity.Error);
    
    public static DiagnosticTemplate<ISymbol> BlockedByFunction { get; } =
        DiagnosticTemplate.Create<ISymbol>(
            symbol => $"Cannot reference variable or parameter '{symbol.Name}' inside function body. " +
                      $"Functions cannot reference variables or parameters from their containing scope",
            Severity.Error);
    
    public static DiagnosticTemplate<ISymbol> DeclaredLater { get; } =
        DiagnosticTemplate.Create<ISymbol>(
            symbol => $"Cannot reference variable '{symbol.Name}' because it has not yet been declared",
            Severity.Error);
}
