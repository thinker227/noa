using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.Compiler.FlowAnalysis;

internal static class FlowAnalyzer
{
    /// <summary>
    /// Analyzes the flow of an AST.
    /// </summary>
    /// <param name="ast">The AST to analyze.</param>
    /// <param name="cancellationToken">The cancellation token which signals the flow analyzer to cancel.</param>
    /// <returns>The diagnostics produced by the analysis.</returns>
    public static IReadOnlyCollection<IDiagnostic> Analyze(
        Ast ast,
        CancellationToken cancellationToken = default)
    {
        var visitor = new Visitor(cancellationToken);
        
        visitor.Visit(ast.Root);

        return visitor.Diagnostics;
    }
}

file sealed class Visitor(CancellationToken cancellationToken) : Visitor<int>
{
    private readonly Stack<IFunction> functions = [];
    private readonly Stack<LoopExpression?> loops = [];

    public List<IDiagnostic> Diagnostics { get; } = [];

    protected override void BeforeVisit(Node node) =>
        cancellationToken.ThrowIfCancellationRequested();

    protected override int VisitFunctionDeclaration(FunctionDeclaration node)
    {
        functions.Push(node.Symbol.Value);
        
        // Push null to the loop stack to indicate that we're at
        // the top-level inside a function body, which is not a loop.
        loops.Push(null);

        Visit(node.Identifier);
        Visit(node.Parameters);
        Visit(node.ExpressionBody);
        Visit(node.BlockBody);

        functions.Pop();
        loops.Pop();
        
        return default;
    }

    protected override int VisitAssignmentStatement(AssignmentStatement node)
    {
        if (node.Target is IdentifierExpression identifier)
        {
            CheckAssignmentTargetSymbol(node.Target, identifier.ReferencedSymbol.Value);
        }
        
        Visit(node.Target);
        Visit(node.Value);

        return default;
    }

    private void CheckAssignmentTargetSymbol(Expression target, ISymbol symbol)
    {
        if (symbol is ErrorSymbol) return;

        if (symbol is not IVariableSymbol variable)
        {
            Diagnostics.Add(FlowDiagnostics.AssignmentToInvalidSymbol.Format(symbol, target.Location));
            return;
        }

        if (!variable.IsMutable)
        {
            Diagnostics.Add(FlowDiagnostics.AssignmentToImmutableSymbol.Format(variable, target.Location));
        }
    }

    protected override int VisitLambdaExpression(LambdaExpression node)
    {
        var lambda = new LambdaFunction()
        {
            Declaration = node
        };
        var parameters = node.Parameters.Select(p => p.Symbol.Value);
        lambda.parameters.AddRange(parameters);
        node.Function = lambda;
        
        functions.Push(lambda);
        
        loops.Push(null);

        Visit(node.Parameters);
        Visit(node.Body);

        functions.Pop();
        loops.Pop();
        
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
            node.Function = new(func);
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
        if (loops.TryPeek(out var loop) && loop is not null)
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
        if (loops.TryPeek(out var loop) && loop is not null)
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
