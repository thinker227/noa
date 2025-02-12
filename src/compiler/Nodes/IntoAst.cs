using Noa.Compiler.Parsing;
using Noa.Compiler.Syntax;

namespace Noa.Compiler.Nodes;

internal sealed class IntoAst(Ast ast) : IntoAstBase
{
    public override Root FromRoot(RootSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Statements = syntax.Statements.Select(FromStatement).ToImmutableArray(),
        TrailingExpression = syntax.TrailingExpression is not null
            ? FromExpression(syntax.TrailingExpression)
            : null
    };
    
    public override Identifier FromIdentifier(Token syntax) => new(syntax)
    {
        Ast = ast,
        Name = syntax.Text
    };
    
    public override Parameter FromParameter(ParameterSyntax syntax) => new(syntax)
    {
        Ast = ast,
        IsMutable = syntax.Mut is not null,
        Identifier = FromIdentifier(syntax.Name)
    };
    
    public override FunctionDeclaration FromFunctionDeclaration(FunctionDeclarationSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Identifier = FromIdentifier(syntax.Name),
        Parameters = syntax.Parameters.Parameters.Nodes().Select(FromParameter).ToImmutableArray(),
        BlockBody = syntax.Body is BlockBodySyntax block
            ? FromBlockExpression(block.Block)
            : null,
        ExpressionBody = syntax.Body is ExpressionBodySyntax expr
            ? FromExpression(expr.Expression)
            : null
    };
    
    public override LetDeclaration FromLetDeclaration(LetDeclarationSyntax syntax) => new(syntax)
    {
        Ast = ast,
        IsMutable = syntax.Mut is not null,
        Identifier = FromIdentifier(syntax.Name),
        Expression = FromExpression(syntax.Value)
    };
    
    public override AssignmentStatement FromAssignmentStatement(AssignmentStatementSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Target = FromExpression(syntax.Target),
        Kind = syntax.Operator.Kind.ToAssignmentKind()
            ?? throw new InvalidOperationException(),
        Value = FromExpression(syntax.Value)
    };
    
    public override ExpressionStatement FromExpressionStatement(ExpressionStatementSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Expression = FromExpression(syntax.Expression)
    };
    
    public override ErrorExpression FromErrorExpression(ErrorExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast
    };
    
    public override BlockExpression FromBlockExpression(BlockExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Statements = syntax.Statements.Select(FromStatement).ToImmutableArray(),
        TrailingExpression = syntax.TrailingExpression is not null
            ? FromExpression(syntax.TrailingExpression)
            : null
    };
    
    public override CallExpression FromCallExpression(CallExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Target = FromExpression(syntax.Target),
        Arguments = syntax.Arguments.Nodes().Select(FromExpression).ToImmutableArray()
    };
    
    public override LambdaExpression FromLambdaExpression(LambdaExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Parameters = syntax.Parameters.Parameters.Nodes().Select(FromParameter).ToImmutableArray(),
        Body = FromExpression(syntax.Expression)
    };
    
    public override TupleExpression FromTupleExpression(TupleExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Expressions = syntax.Expressions.Nodes().Select(FromExpression).ToImmutableArray()
    };
    
    public override IfExpression FromIfExpression(IfExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Condition = FromExpression(syntax.Condition),
        IfTrue = FromBlockExpression(syntax.Body),
        Else = syntax.Else is not null
            ? FromElseClause(syntax.Else)
            : null
    };
    
    public override ElseClause FromElseClause(ElseClauseSyntax syntax) => new(syntax)
    {
        Ast = ast,
        IfFalse = FromBlockExpression(syntax.Body)
    };
    
    public override LoopExpression FromLoopExpression(LoopExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Block = FromBlockExpression(syntax.Body)
    };
    
    public override ReturnExpression FromReturnExpression(ReturnExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Expression = syntax.Value is not null
            ? FromExpression(syntax.Value)
            : null
    };
    
    public override BreakExpression FromBreakExpression(BreakExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Expression = syntax.Value is not null
            ? FromExpression(syntax.Value)
            : null
    };
    
    public override ContinueExpression FromContinueExpression(ContinueExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast
    };
    
    public override UnaryExpression FromUnaryExpression(UnaryExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Kind = syntax.Operator.Kind.ToUnaryKind()
            ?? throw new InvalidOperationException(),
        Operand = FromExpression(syntax.Operand)
    };
    
    public override BinaryExpression FromBinaryExpression(BinaryExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Left = FromExpression(syntax.Left),
        Kind = syntax.Operator.Kind.ToBinaryKind()
            ?? throw new InvalidOperationException(),
        Right = FromExpression(syntax.Right)
    };
    
    public override IdentifierExpression FromIdentifierExpression(IdentifierExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Identifier = syntax.Identifier.Text
    };
    
    public override StringExpression FromStringExpression(StringExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Parts = syntax.Parts.Select(FromStringPart).ToImmutableArray()
    };
    
    public override TextStringPart FromTextStringPart(TextStringPartSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Text = syntax.Text.Text
    };
    
    public override InterpolationStringPart FromInterpolationStringPart(InterpolationStringPartSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Expression = FromExpression(syntax.Expression)
    };
    
    public override BoolExpression FromBoolExpression(BoolExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Value = syntax.Value.Kind switch
        {
            TokenKind.True => true,
            TokenKind.False => false,
            _ => throw new InvalidOperationException()
        }
    };
    
    public override NumberExpression FromNumberExpression(NumberExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast,
        Value = double.Parse(syntax.Value.Text)
    };
    
    public override NilExpression FromNilExpression(NilExpressionSyntax syntax) => new(syntax)
    {
        Ast = ast
    };
}
