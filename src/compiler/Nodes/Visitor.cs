using System.Diagnostics.CodeAnalysis;

namespace Noa.Compiler.Nodes;

/// <summary>
/// Visits AST nodes.
/// </summary>
/// <typeparam name="T">The type the visitor returns.</typeparam>
public abstract class Visitor<T>
{
    /// <summary>
    /// Gets the default return value for a node, or a general default value if the node is null.
    /// </summary>
    /// <param name="node">The node to get the default value for, or null to get a general default value.</param>
    protected abstract T GetDefault(Node node);
    
    /// <summary>
    /// Called before visiting each node.
    /// </summary>
    /// <param name="node">The node being visited.</param>
    protected virtual void BeforeVisit(Node node) {}
    
    /// <summary>
    /// Called after visiting each node.
    /// </summary>
    /// <param name="node">The node being visited.</param>
    /// <param name="result">The result of visiting the node.</param>
    protected virtual void AfterVisit(Node node, T result) {}
    
    /// <summary>
    /// Visits a collection of nodes.
    /// </summary>
    /// <param name="nodes">The nodes to visit.</param>
    /// <param name="useReturn">Specifies whether the method should return the results of visiting the nodes.</param>
    /// <returns>
    /// Either the results of visiting the nodes if <paramref name="useReturn"/> is true,
    /// otherwise returns <see cref="ImmutableArray{T}.Empty"/>.
    /// </returns>
    public ImmutableArray<T> Visit(IEnumerable<Node> nodes, bool useReturn = false)
    {
        var builder = useReturn
            ? ImmutableArray.CreateBuilder<T>()
            : null;

        foreach (var node in nodes)
        {
            var x = Visit(node);
            builder?.Add(x);
        }

        return builder?.ToImmutable() ?? ImmutableArray<T>.Empty;
    }
    
    /// <summary>
    /// Visits a node.
    /// </summary>
    /// <param name="node">The node to visit.</param>
    /// <returns>The result of visiting the node.</returns>
    // Technically since T can be null this is a lie,
    // but it's much nicer to use NotNullIfNotNull here than not to.
    [return: NotNullIfNotNull(nameof(node))]
    public T? Visit(Node? node)
    {
        if (node is null) return default;
        
        BeforeVisit(node);

        var result = node switch
        {
            Root x => VisitRoot(x),
            Identifier x => VisitIdentifier(x),
            Statement x => VisitStatement(x),
            Parameter x => VisitParameter(x),
            Expression x => VisitExpression(x),
            _ => throw new UnreachableException()
        };
        
        AfterVisit(node, result);

        return result!;
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
