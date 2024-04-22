using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;
using Noa.Compiler.Tests;

namespace Noa.Compiler.FlowAnalysis.Tests;

public class FlowAnalyzerTests
{
    [Fact]
    public void Return_OutsideFunction_Produces_ReturnOutsideFunction_AndSetsFunction_ToNull()
    {
        var text = """
        return;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([
            (FlowDiagnostics.ReturnOutsideFunction.Id, new("test-input", 0, 6))
        ]);

        var @return = (ReturnExpression)ast.Root.FindNodeAt(0)!;
        
        @return.Function.Value.ShouldBeNull();
    }
    
    [Fact]
    public void Break_OutsideLoop_Produces_BreakOutsideLoop_AndSetsLoop_ToNull()
    {
        var text = """
        break;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([
            (FlowDiagnostics.BreakOutsideFunction.Id, new("test-input", 0, 5))
        ]);

        var @break = (BreakExpression)ast.Root.FindNodeAt(0)!;
        
        @break.Loop.Value.ShouldBeNull();
    }
    
    [Fact]
    public void Continue_OutsideLoop_Produces_ContinueOutsideLoop_AndSetsLoop_ToNull()
    {
        var text = """
        continue;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([
            (FlowDiagnostics.ContinueOutsideFunction.Id, new("test-input", 0, 8))
        ]);

        var @continue = (ContinueExpression)ast.Root.FindNodeAt(0)!;
        
        @continue.Loop.Value.ShouldBeNull();
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
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
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
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([
            (FlowDiagnostics.ContinueOutsideFunction.Id, new("test-input", 30, 38))
        ]);
    }
    
    [Fact]
    public void Return_InsideFunction_DoesNotProduceDiagnostics_AndSetsFunction_ToFunction()
    {
        var text = """
        func f() {
            return;
        };
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([]);

        var func = (FunctionDeclaration)ast.Root.FindNodeAt(0)!;
        var @return = (ReturnExpression)ast.Root.FindNodeAt(15)!;
        
        @return.Function.Value!.ShouldBeOfType<NomialFunction>();
        @return.Function.Value!.ShouldBe(func.Symbol.Value);
    }
    
    [Fact]
    public void Return_InsideLambda_DoesNotProduceDiagnostics_AndSetsFunction_ToLambda()
    {
        var text = """
        let f = () => {
            return;
        };
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([]);

        var lambda = (LambdaExpression)ast.Root.FindNodeAt(8)!;
        var @return = (ReturnExpression)ast.Root.FindNodeAt(20)!;

        @return.Function.Value!.ShouldBeOfType<LambdaFunction>();
        @return.Function.Value!.ShouldBe(lambda.Function.Value);
    }
    
    [Fact]
    public void Break_InsideLoop_DoesNotProduceDiagnostics_AndSetsLoop_ToLoop()
    {
        var text = """
        loop {
            break;
        };
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([]);

        var loop = (LoopExpression)ast.Root.FindNodeAt(0)!;
        var @break = (BreakExpression)ast.Root.FindNodeAt(11)!;

        @break.Loop.Value!.ShouldBe(loop);
    }
    
    [Fact]
    public void Continue_InsideLoop_DoesNotProduceDiagnostics_AndSetsLoop_ToLoop()
    {
        var text = """
        loop {
            continue;
        };
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        SymbolResolution.ResolveSymbols(ast);
        var diagnostics = FlowAnalyzer.Analyze(ast);

        diagnostics.DiagnosticsShouldBe([]);

        var loop = (LoopExpression)ast.Root.FindNodeAt(0)!;
        var @continue = (ContinueExpression)ast.Root.FindNodeAt(11)!;

        @continue.Loop.Value!.ShouldBe(loop);
    }
}
