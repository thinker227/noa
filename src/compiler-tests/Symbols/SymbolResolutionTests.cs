using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols.Tests;

public class SymbolResolutionTests
{
    [Fact]
    public void Variables_AreShadowed()
    {
        var text = """
        let x = 0;
        let x = 1;
        {}
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.ShouldBeEmpty();

        var x1Decl = (LetDeclaration)ast.Root.Statements[0].Declaration!;
        var x2Decl = (LetDeclaration)ast.Root.Statements[1].Declaration!;
        var end = ast.Root.Statements[2];
        
        var x1 = x1Decl.Symbol.Value;
        var x2 = x2Decl.Symbol.Value;

        x1.Declaration.ShouldBe(x1Decl);
        x2.Declaration.ShouldBe(x2Decl);

        var scope = x1Decl.Scope.Value;

        var x2Lookup = scope.LookupSymbol("x", x2Decl).ShouldNotBeNull();
        x2Lookup.Symbol.ShouldBe(x1);
        x2Lookup.Accessibility.ShouldBe(SymbolAccessibility.Accessible);

        var endLookup = scope.LookupSymbol("x", end).ShouldNotBeNull();
        endLookup.Symbol.ShouldBe(x2);
        endLookup.Accessibility.ShouldBe(SymbolAccessibility.Accessible);
    }

    [Fact]
    public void EarlyLookup_Returns_DeclaredLater()
    {
        var text = """
        let x = 0;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.ShouldBeEmpty();

        var decl = (LetDeclaration)ast.Root.Statements[0].Declaration!;
        var x = decl.Symbol.Value;

        var scope = decl.Scope.Value;

        var lookup = scope.LookupSymbol("x", decl).ShouldNotBeNull();
        lookup.Symbol.ShouldBe(x);
        lookup.Accessibility.ShouldBe(SymbolAccessibility.DeclaredLater);
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

        diagnostics.ShouldBeEmpty();

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
        declared.ShouldBe([x, y]);
        
        var accessible = scope.AccessibleAt(null);
        accessible.ShouldBe([
            new(x, SymbolAccessibility.Accessible),
            new(y, SymbolAccessibility.Accessible)
        ]);
    }
}
