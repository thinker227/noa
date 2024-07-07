// <auto-generated/>

using System.Diagnostics;

namespace Noa.Compiler.Nodes;

public abstract partial class Visitor<T>
{
    public virtual T Visit(Node node)
    {
        if (!Filter(node, out var result)) return result;

        BeforeVisit(node);

        result = node switch
        {
            Identifier x => VisitIdentifier(x),
            Statement x => VisitStatement(x),
            Parameter x => VisitParameter(x),
            Expression x => VisitExpression(x),
            _ => throw new UnreachableException()
        };

        AfterVisit(node, result);

        return result;
    }

    protected virtual T VisitRoot(Root node)
    {
        Visit(node.Statements);
        Visit(node.TrailingExpression);

        return GetDefault(node);
    }

    protected virtual T VisitIdentifier(Identifier node) => GetDefault(node);

    protected virtual T VisitStatement(Statement node) => node switch
    {
        Declaration x => VisitDeclaration(x),
        AssignmentStatement x => VisitAssignmentStatement(x),
        ExpressionStatement x => VisitExpressionStatement(x),
        _ => throw new UnreachableException()
    };

    protected virtual T VisitParameter(Parameter node)
    {
        Visit(node.Identifier);

        return GetDefault(node);
    }

    protected virtual T VisitDeclaration(Declaration node) => node switch
    {
        FunctionDeclaration x => VisitFunctionDeclaration(x),
        LetDeclaration x => VisitLetDeclaration(x),
        _ => throw new UnreachableException()
    };

    protected virtual T VisitFunctionDeclaration(FunctionDeclaration node)
    {
        Visit(node.Identifier);
        Visit(node.Parameters);
        Visit(node.ExpressionBody);
        Visit(node.BlockBody);

        return GetDefault(node);
    }

    protected virtual T VisitLetDeclaration(LetDeclaration node)
    {
        Visit(node.Identifier);
        Visit(node.Expression);

        return GetDefault(node);
    }

    protected virtual T VisitAssignmentStatement(AssignmentStatement node)
    {
        Visit(node.Target);
        Visit(node.Value);

        return GetDefault(node);
    }

    protected virtual T VisitExpressionStatement(ExpressionStatement node)
    {
        Visit(node.Expression);

        return GetDefault(node);
    }

    protected virtual T VisitExpression(Expression node) => node switch
    {
        ErrorExpression x => VisitErrorExpression(x),
        BlockExpression x => VisitBlockExpression(x),
        CallExpression x => VisitCallExpression(x),
        LambdaExpression x => VisitLambdaExpression(x),
        TupleExpression x => VisitTupleExpression(x),
        IfExpression x => VisitIfExpression(x),
        LoopExpression x => VisitLoopExpression(x),
        ReturnExpression x => VisitReturnExpression(x),
        BreakExpression x => VisitBreakExpression(x),
        ContinueExpression x => VisitContinueExpression(x),
        UnaryExpression x => VisitUnaryExpression(x),
        BinaryExpression x => VisitBinaryExpression(x),
        IdentifierExpression x => VisitIdentifierExpression(x),
        StringExpression x => VisitStringExpression(x),
        BoolExpression x => VisitBoolExpression(x),
        NumberExpression x => VisitNumberExpression(x),
        _ => throw new UnreachableException()
    };

    protected virtual T VisitErrorExpression(ErrorExpression node) => GetDefault(node);

    protected virtual T VisitBlockExpression(BlockExpression node)
    {
        Visit(node.Statements);
        Visit(node.TrailingExpression);

        return GetDefault(node);
    }

    protected virtual T VisitCallExpression(CallExpression node)
    {
        Visit(node.Target);
        Visit(node.Arguments);

        return GetDefault(node);
    }

    protected virtual T VisitLambdaExpression(LambdaExpression node)
    {
        Visit(node.Parameters);
        Visit(node.Body);

        return GetDefault(node);
    }

    protected virtual T VisitTupleExpression(TupleExpression node)
    {
        Visit(node.Expressions);

        return GetDefault(node);
    }

    protected virtual T VisitIfExpression(IfExpression node)
    {
        Visit(node.Condition);
        Visit(node.IfTrue);
        Visit(node.IfFalse);

        return GetDefault(node);
    }

    protected virtual T VisitLoopExpression(LoopExpression node)
    {
        Visit(node.Block);

        return GetDefault(node);
    }

    protected virtual T VisitReturnExpression(ReturnExpression node)
    {
        Visit(node.Expression);

        return GetDefault(node);
    }

    protected virtual T VisitBreakExpression(BreakExpression node)
    {
        Visit(node.Expression);

        return GetDefault(node);
    }

    protected virtual T VisitContinueExpression(ContinueExpression node) => GetDefault(node);

    protected virtual T VisitUnaryExpression(UnaryExpression node)
    {
        Visit(node.Operand);

        return GetDefault(node);
    }

    protected virtual T VisitBinaryExpression(BinaryExpression node)
    {
        Visit(node.Left);
        Visit(node.Right);

        return GetDefault(node);
    }

    protected virtual T VisitIdentifierExpression(IdentifierExpression node) => GetDefault(node);

    protected virtual T VisitStringExpression(StringExpression node) => GetDefault(node);

    protected virtual T VisitBoolExpression(BoolExpression node) => GetDefault(node);

    protected virtual T VisitNumberExpression(NumberExpression node) => GetDefault(node);
}
