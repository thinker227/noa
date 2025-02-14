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
        var visitor = new FlowVisitor(cancellationToken);
        
        visitor.Visit(ast.Root);

        return visitor.Diagnostics;
    }
}

file sealed class FlowVisitor(CancellationToken cancellationToken) : Visitor
{
    private readonly Stack<IFunction> functions = [];
    private readonly Stack<LoopExpression?> loops = [];
    
    public List<IDiagnostic> Diagnostics { get; } = [];

    protected override void BeforeVisit(Node node) =>
        cancellationToken.ThrowIfCancellationRequested();

    protected override void VisitRoot(Root node)
    {
        functions.Push(node.Function.Value);

        Visit(node.Block.Statements);
        if (node.Block.TrailingExpression is not null) Visit(node.Block.TrailingExpression);
        
        functions.Pop();
    }

    protected override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        functions.Push(node.Symbol.Value);
        
        // Push null to the loop stack to indicate that we're at
        // the top-level inside a function body, which is not a loop.
        loops.Push(null);

        Visit(node.Identifier);
        Visit(node.Parameters);
        if (node.ExpressionBody is not null) Visit(node.ExpressionBody);
        if (node.BlockBody is not null) Visit(node.BlockBody);

        functions.Pop();
        loops.Pop();
    }

    protected override void VisitAssignmentStatement(AssignmentStatement node)
    {
        if (node.Target is IdentifierExpression identifier)
        {
            CheckAssignmentTargetSymbol(node.Target, identifier.ReferencedSymbol.Value);
        }
        
        Visit(node.Target);
        Visit(node.Value);
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

    protected override void VisitLambdaExpression(LambdaExpression node)
    {
        functions.Push(node.Function.Value);
        
        loops.Push(null);

        Visit(node.Parameters);
        Visit(node.Body);

        functions.Pop();
        loops.Pop();
    }

    protected override void VisitLoopExpression(LoopExpression node)
    {
        loops.Push(node);

        Visit(node.Block);
        
        loops.Pop();
    }

    protected override void VisitReturnExpression(ReturnExpression node)
    {
        if (functions.TryPeek(out var func))
        {
            node.Function = new(func);
        }
        else
        {
            node.Function = null;
            var syntax = (Syntax.ReturnExpressionSyntax)node.Syntax;
            var location = new Location(node.Ast.Source.Name, syntax.Return.Span);
            Diagnostics.Add(FlowDiagnostics.ReturnOutsideFunction.Format(location));
        }

        if (node.Expression is not null) Visit(node.Expression);
    }

    protected override void VisitBreakExpression(BreakExpression node)
    {
        if (loops.TryPeek(out var loop) && loop is not null)
        {
            node.Loop = loop;
        }
        else
        {
            node.Loop = null;
            var syntax = (Syntax.BreakExpressionSyntax)node.Syntax;
            var location = new Location(node.Ast.Source.Name, syntax.Break.Span);
            Diagnostics.Add(FlowDiagnostics.BreakOutsideFunction.Format(location));
        }

        if (node.Expression is not null) Visit(node.Expression);
    }

    protected override void VisitContinueExpression(ContinueExpression node)
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
    }
}
