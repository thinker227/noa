using Noa.Compiler.Bytecode.Builders;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;
using SuperLinq;

namespace Noa.Compiler.Emit;

internal sealed class FunctionBodyEmitter : Visitor<int>
{
    private readonly IFunction function;
    private readonly FunctionBuilder builder;
    private readonly IReadOnlyDictionary<IFunction, FunctionBuilder> builders;
    private readonly StringSectionBuilder strings;
    private readonly IReadOnlyDictionary<IVariableSymbol, VariableIndex> variableIndices;

    private CodeBuilder Code => builder.Code;

    private LocalsInator Locals => builder.Locals;

    private FunctionBodyEmitter(
        IFunction function,
        FunctionBuilder builder,
        IReadOnlyDictionary<IFunction, FunctionBuilder> builders,
        StringSectionBuilder strings,
        IReadOnlyDictionary<IVariableSymbol, VariableIndex> variableIndices)
    {
        this.function = function;
        this.builder = builder;
        this.builders = builders;
        this.strings = strings;
        this.variableIndices = variableIndices;
    }

    public static void Emit(
        IFunction function,
        IReadOnlyDictionary<IFunction, FunctionBuilder> builders,
        StringSectionBuilder strings)
    {
        var builder = builders[function];

        var variableIndices = CreateVariables(function, builder);
        
        var emitter = new FunctionBodyEmitter(function, builder, builders, strings, variableIndices);
        
        emitter.Visit(function.Body);
        emitter.Code.Ret();
    }

    private static IReadOnlyDictionary<IVariableSymbol, VariableIndex> CreateVariables(
        IFunction function,
        FunctionBuilder builder)
    {
        var variableIndices = new Dictionary<IVariableSymbol, VariableIndex>();
        
        foreach (var (paramIndex, param) in function.Parameters.Index())
        {
            variableIndices[param] = builder.Locals.GetParameterVariable(paramIndex);
        }

        foreach (var variable in function.GetLocals())
        {
            variableIndices[variable] = builder.Locals.CreateVariable();
        }

        return variableIndices;
    }

    protected override int VisitFunctionDeclaration(FunctionDeclaration node) => default;

    protected override int VisitExpressionStatement(ExpressionStatement node)
    {
        Visit(node.Expression);
        Code.Pop();

        return default;
    }

    protected override int VisitRoot(Root node) => VisitBlockExpression(node);

    protected override int VisitBlockExpression(BlockExpression node)
    {
        Visit(node.Statements);

        if (node.TrailingExpression is not null) Visit(node.TrailingExpression);
        else Code.PushNil();

        return default;
    }

    protected override int VisitAssignmentStatement(AssignmentStatement node) => throw new NotImplementedException();

    protected override int VisitUnaryExpression(UnaryExpression node)
    {
        Visit(node.Operand);
        
        switch (node.Kind)
        {
        case UnaryKind.Identity:
            Code.NoOp();
            break;

        case UnaryKind.Negate:
            Code.PushInt(0);
            Code.Swap();
            Code.Sub();
            break;

        case UnaryKind.Not:
            Code.Not();
            break;
        
        default: throw new UnreachableException();
        }
        
        return default;
    }

    protected override int VisitBinaryExpression(BinaryExpression node)
    {
        Visit(node.Left);
        Visit(node.Right);
        
        switch (node.Kind)
        {
        case BinaryKind.Plus:
            Code.Add();
            break;
        
        case BinaryKind.Minus:
            Code.Sub();
            break;
        
        case BinaryKind.Mult:
            Code.Mult();
            break;
        
        case BinaryKind.Div:
            Code.Div();
            break;
        
        case BinaryKind.Equal:
            Code.Equal();
            break;
        
        case BinaryKind.NotEqual:
            Code.Equal();
            Code.Not();
            break;
        
        case BinaryKind.LessThan:
            Code.LessThan();
            break;
        
        case BinaryKind.GreaterThan:
            Code.GreaterThan();
            break;
        
        case BinaryKind.LessThanOrEqual:
            Code.GreaterThan();
            Code.Not();
            
            break;
        
        case BinaryKind.GreaterThanOrEqual:
            Code.LessThan();
            Code.Not();
            break;
        
        default: throw new UnreachableException();
        }
        
        return default;
    }

    protected override int VisitNumberExpression(NumberExpression node)
    {
        Code.PushInt(node.Value);

        return default;
    }

    protected override int VisitBoolExpression(BoolExpression node)
    {
        Code.PushBool(node.Value);

        return default;
    }

    protected override int VisitLoopExpression(LoopExpression node)
    {
        throw new NotImplementedException();

        return default;
    }

    protected override int VisitIfExpression(IfExpression node)
    {
        Visit(node.Condition);

        var jumpToTrue = Code.JumpIf();

        Visit(node.IfFalse);
        var jumpToEnd = Code.Jump();
        
        jumpToTrue.SetAddress(Code.CurrentAddress);
        Visit(node.IfTrue);
        
        jumpToEnd.SetAddress(Code.CurrentAddress);

        return default;
    }

    protected override int VisitCallExpression(CallExpression node)
    {
        Visit(node.Target);

        Visit(node.Arguments);
        
        Code.Call((uint)node.Arguments.Length);

        return default;
    }

    protected override int VisitLambdaExpression(LambdaExpression node)
    {
        var lambdaFunctionId = builders[node.Function.Value].Id;
        Code.PushFunc(lambdaFunctionId);

        return default;
    }

    protected override int VisitIdentifierExpression(IdentifierExpression node)
    {
        switch (node.ReferencedSymbol.Value)
        {
        case NomialFunction func:
            var funcId = builders[func].Id;
            Code.PushFunc(funcId);
            break;

        case IVariableSymbol var:
            var index = variableIndices[var];
            Code.LoadVar(index);
            break;
        }

        return default;
    }

    protected override int VisitReturnExpression(ReturnExpression node)
    {
        if (node.Expression is not null) Visit(node.Expression);
        else Code.PushNil();

        Code.Ret();

        return default;
    }

    protected override int VisitLetDeclaration(LetDeclaration node)
    {
        Visit(node.Expression);

        var var = variableIndices[node.Symbol.Value];
        Code.StoreVar(var);
        
        return default;
    }
}
