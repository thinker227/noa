using Noa.Compiler.Bytecode.Builders;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.Compiler.Emit;

internal sealed class FunctionBodyEmitter : Visitor<int>
{
    private readonly IFunction function;
    private readonly FunctionBuilder builder;
    private readonly IReadOnlyDictionary<IFunction, FunctionBuilder> builders;
    private readonly StringSectionBuilder strings;

    private CodeBuilder Code => builder.Code;

    private FunctionBodyEmitter(
        IFunction function,
        IReadOnlyDictionary<IFunction, FunctionBuilder> builders,
        StringSectionBuilder strings)
    {
        this.function = function;
        builder = builders[function];
        this.builders = builders;
        this.strings = strings;
    }

    public static void Emit(
        IFunction function,
        IReadOnlyDictionary<IFunction, FunctionBuilder> builders,
        StringSectionBuilder strings)
    {
        var emitter = new FunctionBodyEmitter(function, builders, strings);
        
        emitter.Visit(function.Body);
        emitter.Code.Ret();
    }

    protected override int VisitFunctionDeclaration(FunctionDeclaration node) => default;

    protected override int VisitExpressionStatement(ExpressionStatement node)
    {
        Visit(node.Expression);
        Code.Pop();

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
            throw new NotImplementedException();
            // Need a temporary variable
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
            // > = !(<=) = !(< || =)
            throw new NotImplementedException();
            Code.LessThan();
            // Need a temporary variable
            break;
        
        case BinaryKind.LessThanOrEqual:
            // <= = (< || =)
            throw new NotImplementedException();
            Code.LessThan();
            // Need a temporary variable
            break;
        
        case BinaryKind.GreaterThanOrEqual:
            // >= = !(<)
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
        
        case VariableSymbol:
            throw new NotImplementedException();
            break;
        
        case ParameterSymbol:
            throw new NotImplementedException();
            break;
        }

        return default;
    }

    protected override int VisitReturnExpression(ReturnExpression node)
    {
        Visit(node.Expression);

        Code.Ret();

        return default;
    }
}
