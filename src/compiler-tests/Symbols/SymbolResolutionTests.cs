using Noa.Compiler.Nodes;
using Noa.Compiler.Tests;

namespace Noa.Compiler.Symbols.Tests;

public class SymbolResolutionTests
{
    [Fact]
    public void Variables_AreShadowed()
    {
        var text = """
        let x = 0;
        let x = 1;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([]);

        var x1Decl = (LetDeclaration)ast.Root.Statements[0].Declaration!;
        var x2Decl = (LetDeclaration)ast.Root.Statements[1].Declaration!;
        
        var x1 = x1Decl.Symbol.Value;
        var x2 = x2Decl.Symbol.Value;

        x1.Declaration.ShouldBe(x1Decl);
        x2.Declaration.ShouldBe(x2Decl);

        var scope = x1Decl.Scope.Value;

        var x2Lookup = scope.LookupSymbol("x", x2Decl).ShouldNotBeNull();
        x2Lookup.Symbol.ShouldBe(x1);
        x2Lookup.Accessibility.ShouldBe(SymbolAccessibility.Accessible);

        var endLookup = scope.LookupSymbol("x", null).ShouldNotBeNull();
        endLookup.Symbol.ShouldBe(x2);
        endLookup.Accessibility.ShouldBe(SymbolAccessibility.Accessible);
    }

    [Fact]
    public void EndLookup_Returns_AllVariables()
    {
        var text = """
        let x = 0;
        let y = 0;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([]);

        var x = ((LetDeclaration)ast.Root.Statements[0].Declaration!).Symbol.Value;
        var y = ((LetDeclaration)ast.Root.Statements[1].Declaration!).Symbol.Value;

        var scope = ast.Root.Statements[0].Scope.Value;

        var xLookup = scope.LookupSymbol("x", null).ShouldNotBeNull();
        xLookup.Symbol.ShouldBe(x);
        xLookup.Accessibility.ShouldBe(SymbolAccessibility.Accessible);
        
        var yLookup = scope.LookupSymbol("y", null).ShouldNotBeNull();
        yLookup.Symbol.ShouldBe(y);
        yLookup.Accessibility.ShouldBe(SymbolAccessibility.Accessible);

        var declared = scope.DeclaredAt(null);
        declared.ShouldBe([x, y], ignoreOrder: true);
        
        var accessible = scope.AccessibleAt(null);
        accessible.ShouldBe([x, y], ignoreOrder: true);
    }

    [Fact]
    public void ReferenceInSubScope_LooksUpSymbol_InParentScopes()
    {
        var text = """
        let x = 0;
        {
            {
                let v = x;
            }
        }
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([]);

        var declaration = (LetDeclaration)ast.Root.Statements[0].Declaration!;
        var reference = (IdentifierExpression)ast.Root.FindNodeAt(35)!;
        
        reference.ReferencedSymbol.Value.ShouldBe(declaration.Symbol.Value);
    }
    
    [Fact]
    public void DuplicateFunctionsDeclaration_Produces_FunctionAlreadyDeclared()
    {
        var text = """
        func f() {}
        func f() {}
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([
            (SymbolDiagnostics.FunctionAlreadyDeclared.Id, new("test-input", 12, 23))
        ]);
    }

    [Fact]
    public void DuplicateParameters_Produces_SymbolAlreadyDeclared()
    {
        var text = """
        func f(a, a) {}
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([
            (SymbolDiagnostics.SymbolAlreadyDeclared.Id, new("test-input", 10, 11))
        ]);
    }

    [Fact]
    public void Variable_WithSameNameAsFunction_Produces_VariableShadowsFunction()
    {
        var text = """
        let x = 0;
        func x {}
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([
            (SymbolDiagnostics.VariableShadowsFunction.Id, new("test-input", 0, 9))
        ]);
    }

    [Fact]
    public void BadReference_Produces_SymbolCannotBeFound()
    {
        var text = """
        let x = v;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([
            (SymbolDiagnostics.SymbolCannotBeFound.Id, new("test-input", 8, 9))
        ]);
    }

    [Fact]
    public void ReferenceToVariable_FromOuterScope_InsideFunction_Produces_BlockedByFunction()
    {
        var text = """
        let x = 0;
        func f() => x;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([
            (SymbolDiagnostics.BlockedByFunction.Id, new("test-input", 23, 24))
        ]);
    }
    
    [Fact]
    public void EarlyLookup_Produces_DeclaredLater()
    {
        var text = """
        let x = x;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([
            (SymbolDiagnostics.DeclaredLater.Id, new("test-input", 8, 9))
        ]);
    }
}
