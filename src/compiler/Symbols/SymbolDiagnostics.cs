namespace Noa.Compiler.Symbols;

internal static class SymbolDiagnostics
{
    public static DiagnosticTemplate<FunctionSymbol> FunctionAlreadyDeclared { get; } =
        DiagnosticTemplate.Create<FunctionSymbol>(
            function => $"Function '{function.Name}' has already been declared in this scope. " +
                        $"Functions cannot shadow other functions in the same scope",
            Severity.Error);
    
    public static DiagnosticTemplate<(VariableSymbol, FunctionSymbol)> VariableShadowsFunction { get; } =
        DiagnosticTemplate.Create<(VariableSymbol var, FunctionSymbol func)>(
            arg => $"Variable '{arg.var.Name}' shadows function '{arg.func.Name}'. " +
                   $"Variables cannot shadow functions",
            Severity.Error);
}
