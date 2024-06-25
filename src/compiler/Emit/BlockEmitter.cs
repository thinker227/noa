using Noa.Compiler.Bytecode.Builders;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.Compiler.Emit;

internal class BlockEmitter(
    IFunction function,
    IReadOnlyDictionary<IFunction, FunctionBuilder> functionBuilders,
    StringSectionBuilder strings)
    : FunctionEmitter(function, functionBuilders, strings)
{
    private int stackSize = 0;
    
    protected override int VisitFunctionDeclaration(FunctionDeclaration node) => default;

    protected override int VisitExpressionStatement(ExpressionStatement node)
    {
        Visit(node.Expression);
        
        // Discard the evaluated value.
        Code.Pop();
        stackSize--;

        return default;
    }

    protected override int VisitRoot(Root node) => VisitBlockExpression(node);

    protected override int VisitBlockExpression(BlockExpression node)
    {
        var emitter = new BlockEmitter(function, functionBuilders, strings);
        
        emitter.Visit(node.Statements);

        if (node.TrailingExpression is not null) emitter.Visit(node.TrailingExpression);
        else Code.PushNil();
        
        // Evaluating the block will always push one value onto the stack.
        stackSize++;

        return default;
    }

    protected override int VisitAssignmentStatement(AssignmentStatement node)
    {
        Visit(node.Value);
        
        // TODO: refactor this to allow targets other than identifiers
        var target = (IdentifierExpression)node.Target;

        var varIndex = Locals.GetOrCreateVariable((IVariableSymbol)target.ReferencedSymbol.Value);
        
        Code.StoreVar(varIndex);
        stackSize -= 1;
        
        return default;
    }

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
        
        // The cumulative effect of evaluating any unary expression is the stack remaining the same size.
        
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
        
        // The cumulative effect of evaluating any binary expression is the stack decreasing by one.
        stackSize -= 1;
        
        return default;
    }

    protected override int VisitNumberExpression(NumberExpression node)
    {
        Code.PushInt(node.Value);
        stackSize += 1;

        return default;
    }

    protected override int VisitBoolExpression(BoolExpression node)
    {
        Code.PushBool(node.Value);
        stackSize += 1;

        return default;
    }

    protected override int VisitLoopExpression(LoopExpression node) => throw new NotImplementedException();

    protected override int VisitIfExpression(IfExpression node)
    {
        var emitter = new BlockEmitter(function, functionBuilders, strings);
        
        Visit(node.Condition);

        var jumpToTrue = Code.JumpIf();

        emitter.Visit(node.IfFalse);
        var jumpToEnd = Code.Jump();
        
        jumpToTrue.SetAddress(Code.AddressOffset);
        emitter.Visit(node.IfTrue);
        
        jumpToEnd.SetAddress(Code.AddressOffset);
        
        // The cumulative effect of evaluating an if expression is the stack remaining the same size
        // (pop 1 for the conditional jump, push 1 for the evaluated value).

        return default;
    }

    protected override int VisitCallExpression(CallExpression node)
    {
        Visit(node.Target);

        Visit(node.Arguments);
        
        Code.Call((uint)node.Arguments.Length);
        
        // The arguments are popped off the stack, as well as the function itself.
        stackSize -= node.Arguments.Length + 1;

        return default;
    }

    protected override int VisitLambdaExpression(LambdaExpression node)
    {
        var lambdaFunctionId = functionBuilders[node.Function.Value].Id;
        
        Code.PushFunc(lambdaFunctionId);
        stackSize += 1;

        return default;
    }

    protected override int VisitIdentifierExpression(IdentifierExpression node)
    {
        switch (node.ReferencedSymbol.Value)
        {
        case NomialFunction func:
            var funcId = functionBuilders[func].Id;
            
            Code.PushFunc(funcId);
            
            break;

        case IVariableSymbol var:
            var varIndex = Locals.GetOrCreateVariable(var);
            
            Code.LoadVar(varIndex);
            
            break;
        
        default: throw new UnreachableException();
        }
        
        // Stack increases by 1 regardless of whether a function or variable is pushed onto the stack.
        stackSize += 1;

        return default;
    }

    protected override int VisitReturnExpression(ReturnExpression node)
    {
        if (node.Expression is not null) Visit(node.Expression);
        else
        {
            Code.PushNil();
            stackSize += 1;
        }

        Code.Ret();
        stackSize -= 1;

        return default;
    }

    protected override int VisitLetDeclaration(LetDeclaration node)
    {
        Visit(node.Expression);

        var var = Locals.GetOrCreateVariable(node.Symbol.Value);
        
        Code.StoreVar(var);
        stackSize -= 1;
        
        return default;
    }
}
