using Noa.Compiler.Symbols;
using Noa.Compiler.Tests;

namespace Noa.Compiler.FlowAnalysis.Tests;

public class AssignmentTests
{
    [Fact]
    public void Assignment_ToMutableVariable_DoesNotReport()
    {
        var text = """
        let mut x = 0;
        x = 1;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([]);
    }

    [Fact]
    public void Assignment_ToMutableParameter_DoesNotReport()
    {
        var text = """
        func f(mut x) {
            x = 1;
        }
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([]);
    }
    
    [Fact]
    public void Assignment_ToImmutableVariable_Reports_AssignmentToImmutableSymbol()
    {
        var text = """
        let x = 0;
        x = 1;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([
            (FlowDiagnostics.AssignmentToImmutableSymbol.Id, new("test-input", 11, 12))
        ]);
    }
    
    [Fact]
    public void Assignment_ToImmutableParameter_Reports_AssignmentToImmutableSymbol()
    {
        var text = """
        func f(x) {
            x = 1;
        }
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([
            (FlowDiagnostics.AssignmentToImmutableSymbol.Id, new("test-input", 16, 17))
        ]);
    }

    [Fact]
    public void Assignment_ToFunction_Reports_AssignmentToInvalidSymbol()
    {
        var text = """
        func f() {}
        f = 0;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([
            (FlowDiagnostics.AssignmentToInvalidSymbol.Id, new("test-input", 12, 13))
        ]);
    }

    [Fact]
    public void Assignment_ToError_DoesNotReport()
    {
        var text = "x = 0;";
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        // Note: the diagnostics from the symbol resolution will not be empty,
        // but the ones from the flow analysis should be.
        
        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);
        
        diagnostics.DiagnosticsShouldBe([]);
    }
}
