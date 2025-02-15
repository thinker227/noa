using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;
using Noa.Compiler.Services.LookupCorrection;

namespace Noa.Compiler.Symbols;

internal static class SymbolDiagnostics
{
    public static DiagnosticTemplate<NomialFunction> FunctionAlreadyDeclared { get; } =
        DiagnosticTemplate.Create<NomialFunction>(
            "NOA-SYM-001",
            (function, page) => page
                .Raw("Function ")
                .Symbol(function)
                .Raw(" has already been declared in this scope. Functions ")
                .Emphasized("cannot shadow others functions")
                .Raw(" the same scope."),
            Severity.Error);

    public static DiagnosticTemplate<string> SymbolAlreadyDeclared { get; } =
        DiagnosticTemplate.Create<string>(
            "NOA-SYM-002",
            (name, page) => page
                .Raw("A symbol with the name ")
                .Name(name)
                .Raw(" has ")
                .Emphasized("already been declared")
                .Raw(" in this scope."),
            Severity.Error);
    
    public static DiagnosticTemplate<(VariableSymbol, NomialFunction)> VariableShadowsFunction { get; } =
        DiagnosticTemplate.Create<(VariableSymbol var, NomialFunction func)>(
            "NOA-SYM-003",
            (arg, page) => page
                .Raw("Variable ")
                .Symbol(arg.var)
                .Raw(" shadows function ")
                .Symbol(arg.func)
                .Raw(". Variables ")
                .Emphasized("cannot shadow functions")
                .Raw(" in the same scope."),
            Severity.Error);
    
    public static DiagnosticTemplate<(string, IScope, Node)> SymbolCannotBeFound { get; } =
        DiagnosticTemplate.Create<(string name, IScope scope, Node at)>(
            "NOA-SYM-004",
            (arg, page) =>
            {
                var (name, scope, at) = arg;
                var corrections = LookupCorrectionService.FindPossibleCorrections(name, scope, at);

                page.Raw("Cannot find a symbol with the name ")
                    .Name(name)
                    .Raw(" in the current scope.");
                
                if (corrections.Count == 0) return;
                
                var correctionActions = DiagnosticPageUtility.ToPageActions(
                    corrections,
                    (s, p) => p.Symbol(s));

                page.Raw(" Did you perhaps mean ")
                    .Many(correctionActions, ManyTerminator.Or)
                    .Raw("?");
            },
            Severity.Error);
    
    public static DiagnosticTemplate<ISymbol> BlockedByFunction { get; } =
        DiagnosticTemplate.Create<ISymbol>(
            "NOA-SYM-005",
            (symbol, page) => page
                .Raw("Cannot reference variable or parameter ")
                .Symbol(symbol)
                .Raw(" inside function body. Functions cannot reference ")
                .Emphasized("variables")
                .Raw(" or ")
                .Emphasized("parameters")
                .Raw(" from their containing scope."),
            Severity.Error);

    public static DiagnosticTemplate<ISymbol> DeclaredLater { get; } =
        DiagnosticTemplate.Create<ISymbol>(
            "NOA-SYM-006",
            (symbol, page) => page
                .Raw("Cannot reference variable ")
                .Symbol(symbol)
                .Raw(" because it has ")
                .Emphasized("not been declared yet")
                .Raw("."),
            Severity.Error);
}
