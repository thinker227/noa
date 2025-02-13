using Noa.Compiler.Nodes;

namespace Noa.Compiler.ControlFlow;

internal static class ControlFlowMarker
{
    /// <summary>
    /// Marks the control flow of an AST.
    /// </summary>
    /// <param name="ast">The AST to mark.</param>
    /// <param name="cancellationToken">The cancellation token which signals the marker to cancel.</param>
    public static void Mark(Ast ast, CancellationToken cancellationToken = default)
    {
        var visitor = new ControlFlowVisitor(Reachability.Reachable, cancellationToken);

        visitor.Visit(ast.Root);
    }
}

file sealed class ControlFlowVisitor(Reachability current, CancellationToken cancellationToken)
    : Visitor<ControlFlowResult>
{
    private Reachability current = current;
    
    protected override ControlFlowResult GetDefault(Node node) => new(current, current);

    protected override void BeforeVisit(Node node) =>
        cancellationToken.ThrowIfCancellationRequested();

    protected override void AfterVisit(Node node, ControlFlowResult result)
    {
        switch (node)
        {
        case Statement statement:
            statement.Reachability = result.Current;
            break;
        case Expression expression:
            expression.Reachability = result.Current;
            break;
        }
        
        current = result.Next;
    }

    private ControlFlowVisitor CreateSubVisitor() => new(current, cancellationToken);

    private ControlFlowVisitor CreateNewVisitor() =>
        new(Reachability.Reachable, cancellationToken);

    protected override ControlFlowResult VisitFunctionDeclaration(FunctionDeclaration node)
    {
        var bodyVisitor = CreateNewVisitor();
        if (node.BlockBody is not null) bodyVisitor.Visit(node.BlockBody);
        if (node.ExpressionBody is not null) bodyVisitor.Visit(node.ExpressionBody);
        
        return new(current, current);
    }

    protected override ControlFlowResult VisitLetDeclaration(LetDeclaration node) =>
        Visit(node.Expression);

    protected override ControlFlowResult VisitAssignmentStatement(AssignmentStatement node) =>
        Visit(node.Value);

    protected override ControlFlowResult VisitBlockExpression(BlockExpression node)
    {
        var blockVisitor = CreateSubVisitor();
        var statements = blockVisitor.Visit(node.Block.Statements, true);
        var trailingExpression = node.Block.TrailingExpression is not null
            ? blockVisitor.Visit(node.Block.TrailingExpression)
            : null as ControlFlowResult?;

        var next = (statements, trailingExpression) switch
        {
            (_, not null) => trailingExpression.Value.Next,
            ([.., var last], null) => last.Next,
            _ => current
        };

        node.Block.TailReachability = next;

        return new(current, next);
    }

    protected override ControlFlowResult VisitReturnExpression(ReturnExpression node)
    {
        if (node.Expression is not null) Visit(node.Expression);

        var next = current with
        {
            CanFallThrough = false,
            CanReturn = current.CanReturn || current.IsReachable
        };
        return new(current, next);
    }

    protected override ControlFlowResult VisitBreakExpression(BreakExpression node)
    {
        if (node.Expression is not null) Visit(node.Expression);

        var next = current with
        {
            CanFallThrough = false,
            CanBreak = current.CanBreak || current.IsReachable
        };
        return new(current, next);
    }

    protected override ControlFlowResult VisitContinueExpression(ContinueExpression node)
    {
        var next = current with
        {
            CanFallThrough = false,
            CanContinue = current.CanContinue || current.IsReachable
        };
        return new(current, next);
    }

    protected override ControlFlowResult VisitIfExpression(IfExpression node)
    {
        Visit(node.Condition);
        
        var ifTrueNext = CreateSubVisitor().Visit(node.IfTrue).Next;
        var ifFalseNext = node.Else is { IfFalse: var ifFalse }
            ? CreateSubVisitor().Visit(ifFalse).Next
            : current;

        var next = ifTrueNext | ifFalseNext;

        return new(current, next);
    }

    protected override ControlFlowResult VisitLoopExpression(LoopExpression node)
    {
        var tail = CreateSubVisitor().Visit(node.Block).Next;

        var next = current with
        {
            CanFallThrough = tail.CanBreak,
            CanReturn = tail.CanReturn
        };

        return new(current, next);
    }

    protected override ControlFlowResult VisitLambdaExpression(LambdaExpression node)
    {
        var bodyVisitor = CreateNewVisitor();
        bodyVisitor.Visit(node.Body);

        return new(current, current);
    }
}

internal readonly record struct ControlFlowResult(Reachability Current, Reachability Next);
