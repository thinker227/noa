using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;

namespace Noa.Compiler.FlowAnalysis;

public static class FlowAnalyzer
{
    /// <summary>
    /// Analyzes the flow of an AST.
    /// </summary>
    /// <param name="ast">The AST to analyze.</param>
    /// <returns>The diagnostics produced by the analysis.</returns>
    public static IReadOnlyCollection<IDiagnostic> Analyze(Ast ast)
    {
        var visitor = new Visitor();
        
        visitor.Visit(ast.Root);

        return visitor.Diagnostics;
    }
}

file sealed class Visitor : Visitor<int>
{
    private readonly Stack<FunctionDeclaration> functions = [];
    private readonly Stack<LoopExpression> loops = [];

    public List<IDiagnostic> Diagnostics { get; } = [];
    
    protected override int VisitFunctionDeclaration(FunctionDeclaration node)
    {
        functions.Push(node);

        Visit(node.Identifier);
        Visit(node.Parameters);
        Visit(node.ExpressionBody);
        Visit(node.BlockBody);

        functions.Pop();

        return default;
    }

    protected override int VisitLoopExpression(LoopExpression node)
    {
        loops.Push(node);

        Visit(node.Block);
        
        loops.Pop();
        
        return default;
    }

    protected override int VisitReturnExpression(ReturnExpression node)
    {
        if (functions.TryPeek(out var func))
        {
            node.Function = func;
        }
        else
        {
            node.Function = null;
            Diagnostics.Add(FlowDiagnostics.ReturnOutsideFunction.Format(node.Location));
        }

        Visit(node.Expression);

        return default;
    }

    protected override int VisitBreakExpression(BreakExpression node)
    {
        if (loops.TryPeek(out var loop))
        {
            node.Loop = loop;
        }
        else
        {
            node.Loop = null;
            Diagnostics.Add(FlowDiagnostics.BreakOutsideFunction.Format(node.Location));
        }

        Visit(node.Expression);

        return default;
    }

    protected override int VisitContinueExpression(ContinueExpression node)
    {
        if (loops.TryPeek(out var loop))
        {
            node.Loop = loop;
        }
        else
        {
            node.Loop = null;
            Diagnostics.Add(FlowDiagnostics.ContinueOutsideFunction.Format(node.Location));
        }

        return default;
    }
}
