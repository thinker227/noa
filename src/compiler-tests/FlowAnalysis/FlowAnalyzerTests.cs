using Noa.Compiler.Tests;

namespace Noa.Compiler.FlowAnalysis.Tests;

public class FlowAnalyzerTests
{
    [Fact]
    public void Return_OutsideFunction_Produces_ReturnOutsideFunction()
    {
        var text = """
        return;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([
            (FlowDiagnostics.ReturnOutsideFunction.Id, new("test-input", 0, 6))
        ]);
    }
    
    [Fact]
    public void Break_OutsideLoop_Produces_BreakOutsideLoop()
    {
        var text = """
        break;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([
            (FlowDiagnostics.BreakOutsideFunction.Id, new("test-input", 0, 5))
        ]);
    }
    
    [Fact]
    public void Continue_OutsideLoop_Produces_ContinueOutsideLoop()
    {
        var text = """
        continue;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([
            (FlowDiagnostics.ContinueOutsideFunction.Id, new("test-input", 0, 8))
        ]);
    }
    
    [Fact]
    public void Break_InsideFunction_InsideLoop_Produces_BreakOutsideLoop()
    {
        var text = """
        loop {
            func f() {
                break;
            };
        };
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([
            (FlowDiagnostics.BreakOutsideFunction.Id, new("test-input", 30, 35))
        ]);
    }
    
    [Fact]
    public void Continue_InsideFunction_InsideLoop_Produces_ContinueOutsideLoop()
    {
        var text = """
        loop {
            func f() {
                continue;
            };
        };
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([
            (FlowDiagnostics.ContinueOutsideFunction.Id, new("test-input", 30, 38))
        ]);
    }
    
    [Fact]
    public void Return_InsideFunction_DoesNotProduceDiagnostics()
    {
        var text = """
        func f() {
            return;
        };
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([]);
    }
    
    [Fact]
    public void Return_InsideLambda_DoesNotProduceDiagnostics()
    {
        var text = """
        let f = () => {
            return;
        };
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([]);
    }
    
    [Fact]
    public void Break_InsideLoop_DoesNotProduceDiagnostics()
    {
        var text = """
        loop {
            continue;
        };
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([]);
    }
    
    [Fact]
    public void Continue_InsideLoop_DoesNotProduceDiagnostics()
    {
        var text = """
        loop {
            continue;
        };
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Create(source);

        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([]);
    }
}
