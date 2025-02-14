using Noa.Compiler.Parsing;
using Noa.Compiler.Syntax;

namespace Noa.Compiler.Nodes;

internal sealed class IntoAst(Ast ast)
{
    public Block FromBlock(BlockSyntax syntax) => new(ast, syntax)
    {
        Statements = syntax.Statements.Select(FromStatement).ToImmutableArray(),
        TrailingExpression = syntax.TrailingExpression is not null
            ? FromExpression(syntax.TrailingExpression)
            : null
    };

    public Root FromRoot(RootSyntax syntax) => new(ast, syntax)
    {
        Block = FromBlock(syntax.Block)
    };
    
    public Identifier FromIdentifier(Token syntax) => new(ast, syntax)
    {
        Name = syntax.Text
    };
    
    public Parameter FromParameter(ParameterSyntax syntax) => new(ast, syntax)
    {
        IsMutable = syntax.Mut is not null,
        Identifier = FromIdentifier(syntax.Name)
    };

    public Statement FromStatement(StatementSyntax syntax) => syntax switch
    {
        DeclarationSyntax decl => FromDeclaration(decl),
        AssignmentStatementSyntax assign => new AssignmentStatement(ast, syntax)
        {
            Target = FromExpression(assign.Target),
            Kind = assign.Operator.Kind.ToAssignmentKind()
                ?? throw new InvalidOperationException(),
            Value = FromExpression(assign.Value)
        },
        FlowControlStatementSyntax flow => new ExpressionStatement(ast, syntax)
        {
            Expression = FromExpression(flow.Expression)
        },
        ExpressionStatementSyntax expr => new ExpressionStatement(ast, syntax)
        {
            Expression = FromExpression(expr.Expression)
        },
        _ => throw new UnreachableException()
    };

    public Declaration FromDeclaration(DeclarationSyntax syntax) => syntax switch
    {
        FunctionDeclarationSyntax function => new FunctionDeclaration(ast, syntax)
        {
            Identifier = FromIdentifier(function.Name),
            Parameters = function.Parameters.Parameters.Nodes().Select(FromParameter).ToImmutableArray(),
            BlockBody = function.Body is BlockBodySyntax block
                ? FromBlockExpression(block.Block)
                : null,
            ExpressionBody = function.Body is ExpressionBodySyntax expr
                ? FromExpression(expr.Expression)
                : null
        },
        LetDeclarationSyntax let => new LetDeclaration(ast, syntax)
        {
            IsMutable = let.Mut is not null,
            Identifier = FromIdentifier(let.Name),
            Expression = FromExpression(let.Value)
        },
        _ => throw new UnreachableException()
    };
    
    public Expression FromExpression(ExpressionSyntax syntax) => syntax switch
    {
        ErrorExpressionSyntax => new ErrorExpression(ast, syntax),
        BlockExpressionSyntax block => FromBlockExpression(block),
        CallExpressionSyntax call => new CallExpression(ast, syntax)
        {
            Target = FromExpression(call.Target),
            Arguments = call.Arguments.Nodes().Select(FromExpression).ToImmutableArray()
        },
        LambdaExpressionSyntax lambda => new LambdaExpression(ast, syntax)
        {
            Parameters = lambda.Parameters.Parameters.Nodes().Select(FromParameter).ToImmutableArray(),
            Body = FromExpression(lambda.Expression)
        },
        TupleExpressionSyntax tuple => new TupleExpression(ast, syntax)
        {
            Expressions = tuple.Expressions.Nodes().Select(FromExpression).ToImmutableArray()
        },
        ParenthesizedExpressionSyntax parens => FromExpression(parens.Expression),
        IfExpressionSyntax @if => new IfExpression(ast, syntax)
        {
            Condition = FromExpression(@if.Condition),
            IfTrue = FromBlockExpression(@if.Body),
            Else = @if.Else is not null
                ? new ElseClause(ast, @if.Else)
                {
                    IfFalse = FromBlockExpression(@if.Else.Body)
                }
                : null
        },
        LoopExpressionSyntax loop => new LoopExpression(ast, syntax)
        {
            Block = FromBlockExpression(loop.Body)
        },
        ReturnExpressionSyntax @return => new ReturnExpression(ast, syntax)
        {
            Expression = @return.Value is not null
                ? FromExpression(@return.Value)
                : null
        },
        BreakExpressionSyntax @break => new BreakExpression(ast, syntax)
        {
            Expression = @break.Value is not null
                ? FromExpression(@break.Value)
                : null
        },
        ContinueExpressionSyntax => new ContinueExpression(ast, syntax),
        UnaryExpressionSyntax unary => new UnaryExpression(ast, syntax)
        {
            Kind = unary.Operator.Kind.ToUnaryKind()
                ?? throw new InvalidOperationException(),
            Operand = FromExpression(unary.Operand)
        },
        BinaryExpressionSyntax binary => new BinaryExpression(ast, syntax)
        {
            Left = FromExpression(binary.Left),
            Kind = binary.Operator.Kind.ToBinaryKind()
                ?? throw new InvalidOperationException(),
            Right = FromExpression(binary.Right)
        },
        IdentifierExpressionSyntax identifier => new IdentifierExpression(ast, syntax)
        {
            Identifier = identifier.Identifier.Text
        },
        StringExpressionSyntax @string => new StringExpression(ast, syntax)
        {
            Parts = @string.Parts.Select(FromStringPart).ToImmutableArray()
        },
        BoolExpressionSyntax @bool => new BoolExpression(ast, syntax)
        {
            Value = @bool.Value.Kind switch
            {
                TokenKind.True => true,
                TokenKind.False => false,
                _ => throw new InvalidOperationException()
            }
        },
        NumberExpressionSyntax number => new NumberExpression(ast, syntax)
        {
            Value = double.Parse(number.Value.Text)
        },
        NilExpressionSyntax => new NilExpression(ast, syntax),
        _ => throw new UnreachableException()
    };

    public BlockExpression FromBlockExpression(BlockExpressionSyntax syntax) => new BlockExpression(ast, syntax)
    {
        Block = FromBlock(syntax.Block)
    };
    
    public StringPart FromStringPart(StringPartSyntax syntax) => syntax switch
    {
        TextStringPartSyntax text => new TextStringPart(ast, syntax)
        {
            Text = text.Text.Text
        },
        InterpolationStringPartSyntax interpolation => new InterpolationStringPart(ast, syntax)
        {
            Expression = FromExpression(interpolation.Expression)
        },
        _ => throw new UnreachableException()
    };
}
