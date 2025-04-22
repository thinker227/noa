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
        var ast = Ast.Parse(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([]);

        var x1Decl = (LetDeclaration)ast.Root.Block.Statements[0];
        var x2Decl = (LetDeclaration)ast.Root.Block.Statements[1];
        
        var x1 = x1Decl.Symbol.Value;
        var x2 = x2Decl.Symbol.Value;

        x1.Declaration.ShouldBe(x1Decl);
        x2.Declaration.ShouldBe(x2Decl);

        var scope = x1Decl.Scope.Value;

        var x2Lookup = scope.LookupSymbol("x", LookupLocation.AtNode(x2Decl)).ShouldNotBeNull();
        x2Lookup.Symbol.ShouldBe(x1);
        x2Lookup.Accessibility.ShouldBe(SymbolAccessibility.Accessible);

        var endLookup = scope.LookupSymbol("x", LookupLocation.AtEnd()).ShouldNotBeNull();
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
        var ast = Ast.Parse(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([]);

        var x = ((LetDeclaration)ast.Root.Block.Statements[0]).Symbol.Value;
        var y = ((LetDeclaration)ast.Root.Block.Statements[1]).Symbol.Value;

        var scope = ast.Root.Block.Statements[0].Scope.Value;

        var xLookup = scope.LookupSymbol("x", LookupLocation.AtEnd()).ShouldNotBeNull();
        xLookup.Symbol.ShouldBe(x);
        xLookup.Accessibility.ShouldBe(SymbolAccessibility.Accessible);
        
        var yLookup = scope.LookupSymbol("y", LookupLocation.AtEnd()).ShouldNotBeNull();
        yLookup.Symbol.ShouldBe(y);
        yLookup.Accessibility.ShouldBe(SymbolAccessibility.Accessible);

        var declared = scope.DeclaredAt(LookupLocation.AtEnd());
        declared.ShouldBe([x, y], ignoreOrder: true);
        
        var accessible = scope.AccessibleAt(LookupLocation.AtEnd());
        accessible.ShouldBe([x, y], ignoreOrder: true);
    }

    [Fact]
    public void ReferenceInTrailingExpression_LooksUpSymbol_InBlock()
    {
        var text = """
        {
            let x = 0;
            x
        }
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([]);

        var declaration = (LetDeclaration)ast.Root.FindNodeAt(6)!;
        var reference = (IdentifierExpression)ast.Root.FindNodeAt(21)!;
        
        reference.ReferencedSymbol.Value.ShouldBe(declaration.Symbol.Value);
    }

    [Fact]
    public void ReferenceInBlock_InTrailingExpression_LooksUpSymbol_InBlock()
    {
        var text = """
        {
            let x = 0;
            {
                x
            }
        }
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([]);

        var declaration = (LetDeclaration)ast.Root.FindNodeAt(6)!;
        var reference = (IdentifierExpression)ast.Root.FindNodeAt(31)!;
        
        reference.ReferencedSymbol.Value.ShouldBe(declaration.Symbol.Value);
    }

    [Fact]
    public void ReferenceInSubNodeInBlock_LooksUpSymbol_InParentBlock()
    {
        var text = """
        let x = 0;
        {
            print(x);
        }
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        
        var x = ast.TopLevelFunction.BodyScope.LookupSymbol("x", LookupLocation.AtEnd())!.Value.Symbol;
        
        var assignmentIdentifier = (IdentifierExpression)ast.Root.FindNodeAt(23)!;
        
        assignmentIdentifier.ReferencedSymbol.Value.ShouldBe(x);
    }

    [Fact]
    public void SymbolsInParentScopes_AreAccessible_InSubNodeInBlock()
    {
        var text = """
        let x = 0;
        {
            let y = 1;
            print(0);
        }
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        
        var at = ast.Root.FindNodeAt(38)!;
        
        var x = ast.TopLevelFunction.BodyScope.LookupSymbol("x", LookupLocation.AtEnd())!.Value.Symbol;
        var y = at.Scope.Value.LookupSymbol("y", LookupLocation.AtEnd())!.Value.Symbol;
        
        at.Scope.Value.AccessibleAt(LookupLocation.AtNode(at)).ShouldBe([x, y], ignoreOrder: true);
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
        var ast = Ast.Parse(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([]);

        var declaration = (LetDeclaration)ast.Root.Block.Statements[0];
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
        var ast = Ast.Parse(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([
            (SymbolDiagnostics.FunctionAlreadyDeclared.Id, new("test-input", 17, 18))
        ]);
    }

    [Fact]
    public void DuplicateParameters_Produces_SymbolAlreadyDeclared()
    {
        var text = """
        func f(a, a) {}
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

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
        var ast = Ast.Parse(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([
            (SymbolDiagnostics.VariableShadowsFunction.Id, new("test-input", 4, 5))
        ]);
    }

    [Fact]
    public void BadReference_Produces_SymbolCannotBeFound()
    {
        var text = """
        let x = v;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

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
        var ast = Ast.Parse(source);

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
        var ast = Ast.Parse(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.DiagnosticsShouldBe([
            (SymbolDiagnostics.DeclaredLater.Id, new("test-input", 8, 9))
        ]);
    }
}
