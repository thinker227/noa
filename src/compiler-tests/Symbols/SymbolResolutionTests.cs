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
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = SymbolResolution.ResolveSymbols(ast);

        diagnostics.ShouldBeEmpty();

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
}
