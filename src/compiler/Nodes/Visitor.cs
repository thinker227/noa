namespace Noa.Compiler.Nodes;

/// <summary>
/// Visits AST nodes.
/// </summary>
/// <typeparam name="T">The type the visitor returns.</typeparam>
public abstract class Visitor<T>
{
    /// <summary>
    /// Called before visiting each node.
    /// </summary>
    /// <param name="node">The node being visited.</param>
    protected virtual void BeforeVisit(Node node) {}

    /// <summary>
    /// Visits a collection of nodes.
    /// </summary>
    /// <param name="nodes">The nodes to visit.</param>
    /// <returns>The results of visiting the nodes.</returns>
    public ImmutableArray<T> Visit(IEnumerable<Node> nodes)
    {
        var builder = ImmutableArray.CreateBuilder<T>();

        foreach (var node in nodes)
        {
            var x = Visit(node);
            builder.Add(x);
        }

        return builder.ToImmutable();
    }
    
    /// <summary>
    /// Visits a node.
    /// </summary>
    /// <param name="node">The node to visit.</param>
    /// <returns>The result of visiting the node.</returns>
    public T Visit(Node? node)
    {
        if (node is null) return default!;
        
        BeforeVisit(node);

        return node switch
        {
            Root x => VisitRoot(x),
            Identifier x => VisitIdentifier(x),
            Statement x => VisitStatement(x),
            Parameter x => VisitParameter(x),
            Declaration x => VisitDeclaration(x),
            Expression x => VisitExpression(x),
            _ => throw new UnreachableException()
        };
    }
    
    protected virtual T VisitRoot(Root node)
    {
        Visit(node.Statements);
        
        return default!;
    }
    
    protected virtual T VisitIdentifier(Identifier node) => default!;

    protected virtual T VisitStatement(Statement node)
    {
        Visit(node.Declaration);
        Visit(node.Expression);
        
        return default!;
    }
    
    protected virtual T VisitParameter(Parameter node)
    {
        Visit(node.Identifier);

        return default!;
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
        
        return default!;
    }
    
    protected virtual T VisitLetDeclaration(LetDeclaration node)
    {
        Visit(node.Identifier);
        Visit(node.Expression);
        
        return default!;
    }

    protected virtual T VisitExpression(Expression node) => node switch
    {
        BinaryExpression x => VisitBinaryExpression(x),
        BlockExpression x => VisitBlockExpression(x),
        BoolExpression x => VisitBoolExpression(x),
        BreakExpression x => VisitBreakExpression(x),
        CallExpression x => VisitCallExpression(x),
        ContinueExpression x => VisitContinueExpression(x),
        ErrorExpression x => VisitErrorExpression(x),
        IdentifierExpression x => VisitIdentifierExpression(x),
        IfExpression x => VisitIfExpression(x),
        LambdaExpression x => VisitLambdaExpression(x),
        LoopExpression x => VisitLoopExpression(x),
        NumberExpression x => VisitNumberExpression(x),
        ReturnExpression x => VisitReturnExpression(x),
        StringExpression x => VisitStringExpression(x),
        TupleExpression x => VisitTupleExpression(x),
        UnaryExpression x => VisitUnaryExpression(x),
        _ => throw new UnreachableException()
    };
    
    protected virtual T VisitErrorExpression(ErrorExpression node) => default!;

    protected virtual T VisitBlockExpression(BlockExpression node)
    {
        Visit(node.Statements);
        Visit(node.TrailingExpression);

        return default!;
    }
    
    protected virtual T VisitCallExpression(CallExpression node)
    {
        Visit(node.Target);
        Visit(node.Arguments);

        return default!;
    }
    
    protected virtual T VisitLambdaExpression(LambdaExpression node)
    {
        Visit(node.Parameters);
        Visit(node.Body);

        return default!;
    }
    
    protected virtual T VisitTupleExpression(TupleExpression node)
    {
        Visit(node.Expressions);

        return default!;
    }
    
    protected virtual T VisitIfExpression(IfExpression node)
    {
        Visit(node.Condition);
        Visit(node.IfTrue);
        Visit(node.IfFalse);

        return default!;
    }
    
    protected virtual T VisitLoopExpression(LoopExpression node)
    {
        Visit(node.Block);

        return default!;
    }
    
    protected virtual T VisitReturnExpression(ReturnExpression node)
    {
        Visit(node.Expression);

        return default!;
    }
    
    protected virtual T VisitBreakExpression(BreakExpression node)
    {
        Visit(node.Expression);

        return default!;
    }

    protected virtual T VisitContinueExpression(ContinueExpression node) => default!;
    
    protected virtual T VisitUnaryExpression(UnaryExpression node)
    {
        Visit(node.Operand);

        return default!;
    }
    
    protected virtual T VisitBinaryExpression(BinaryExpression node)
    {
        Visit(node.Left);
        Visit(node.Right);

        return default!;
    }
    
    protected virtual T VisitIdentifierExpression(IdentifierExpression node) => default!;

    protected virtual T VisitStringExpression(StringExpression node) => default!;

    protected virtual T VisitBoolExpression(BoolExpression node) => default!;

    protected virtual T VisitNumberExpression(NumberExpression node) => default!;
}
