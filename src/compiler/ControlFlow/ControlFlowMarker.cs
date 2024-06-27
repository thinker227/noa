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
        var visitor = new Visitor(Reachability.Reachable, cancellationToken);

        visitor.Visit(ast.Root);
    }
}

file sealed class Visitor(Reachability current, CancellationToken cancellationToken)
    : Visitor<ControlFlowResult>
{
    protected override ControlFlowResult GetDefault(Node? node)
    {
        if (node is null) throw new InvalidOperationException();
        
        return new(current, current);
    }

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

    private Visitor CreateSubVisitor() => new(current, cancellationToken);

    private Visitor CreateNewVisitor() =>
        new(Reachability.Reachable, cancellationToken);

    protected override ControlFlowResult VisitRoot(Root node) => VisitBlockExpression(node);

    protected override ControlFlowResult VisitExpressionStatement(ExpressionStatement node) =>
        Visit(node.Expression);

    protected override ControlFlowResult VisitFunctionDeclaration(FunctionDeclaration node)
    {
        var bodyVisitor = CreateNewVisitor();
        bodyVisitor.Visit(node.BlockBody);
        bodyVisitor.Visit(node.ExpressionBody);
        
        return new(current, current);
    }

    protected override ControlFlowResult VisitLetDeclaration(LetDeclaration node) =>
        Visit(node.Expression);

    protected override ControlFlowResult VisitAssignmentStatement(AssignmentStatement node) =>
        Visit(node.Value);

    protected override ControlFlowResult VisitBlockExpression(BlockExpression node)
    {
        var blockVisitor = CreateSubVisitor();
        var statements = blockVisitor.Visit(node.Statements, true);
        var trailingExpression = node.TrailingExpression is not null
            ? blockVisitor.Visit(node.TrailingExpression)
            : null as ControlFlowResult?;

        var next = (statements, trailingExpression) switch
        {
            (_, not null) => trailingExpression.Value.Next,
            ([.., var last], null) => last.Next,
            _ => current
        };

        return new(current, next);
    }

    protected override ControlFlowResult VisitReturnExpression(ReturnExpression node)
    {
        var reachability = Visit(node.Expression).Next;
        return new(reachability, Reachability.Unreachable(UnreachabilitySource.Return));
    }

    protected override ControlFlowResult VisitBreakExpression(BreakExpression node)
    {
        var reachability = Visit(node.Expression).Next;
        return new(reachability, Reachability.Unreachable(UnreachabilitySource.Break));
    }

    protected override ControlFlowResult VisitContinueExpression(ContinueExpression node) =>
        new(current, Reachability.Unreachable(UnreachabilitySource.Continue));

    protected override ControlFlowResult VisitIfExpression(IfExpression node)
    {
        Visit(node.Condition);
        
        var ifTrueNext = CreateSubVisitor().Visit(node.IfTrue).Next;
        var ifFalseNext = CreateSubVisitor().Visit(node.IfFalse).Next;

        var next = ifTrueNext.Join(ifFalseNext);

        return new(current, next);
    }

    protected override ControlFlowResult VisitLoopExpression(LoopExpression node)
    {
        var blockVisitor = CreateSubVisitor();
        var next = blockVisitor.Visit(node.Block).Next;
        
        return new(current, next);
    }

    protected override ControlFlowResult VisitUnaryExpression(UnaryExpression node) =>
        Visit(node.Operand);

    protected override ControlFlowResult VisitBinaryExpression(BinaryExpression node)
    {
        var leftNext = Visit(node.Left).Next;
        var rightNext = Visit(node.Right).Next;

        var next = leftNext.Join(rightNext);

        return new(current, next);
    }

    protected override ControlFlowResult VisitCallExpression(CallExpression node)
    {
        Visit(node.Target);
        Visit(node.Arguments);

        return new(current, current);
    }

    protected override ControlFlowResult VisitLambdaExpression(LambdaExpression node)
    {
        var bodyVisitor = CreateNewVisitor();
        bodyVisitor.Visit(node.Body);

        return new(current, current);
    }

    protected override ControlFlowResult VisitTupleExpression(TupleExpression node)
    {
        Visit(node.Expressions);

        return new(current, current);
    }
}

internal readonly record struct ControlFlowResult(Reachability Current, Reachability Next);
