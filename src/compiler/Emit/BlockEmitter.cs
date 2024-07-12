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
    protected override void VisitFunctionDeclaration(FunctionDeclaration node) {}

    protected override void VisitExpressionStatement(ExpressionStatement node)
    {
        Visit(node.Expression);
        
        // Discard the evaluated value.
        Code.Pop();
    }

    protected override void VisitRoot(Root node) => VisitBlockExpression(node);

    protected override void VisitBlockExpression(BlockExpression node)
    {
        Visit(node.Statements);

        if (node.TrailingExpression is not null) Visit(node.TrailingExpression);
        else
        {
            Code.PushNil();
        }
    }

    protected override void VisitAssignmentStatement(AssignmentStatement node)
    {
        Visit(node.Value);
        
        // TODO: refactor this to allow targets other than identifiers
        var target = (IdentifierExpression)node.Target;

        var varIndex = Locals.GetOrCreateVariable((IVariableSymbol)target.ReferencedSymbol.Value);
        
        Code.StoreVar(varIndex);
    }

    protected override void VisitUnaryExpression(UnaryExpression node)
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
    }

    protected override void VisitBinaryExpression(BinaryExpression node)
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
    }

    protected override void VisitNumberExpression(NumberExpression node) => Code.PushInt(node.Value);

    protected override void VisitBoolExpression(BoolExpression node) => Code.PushBool(node.Value);

    protected override void VisitNilExpression(NilExpression node) => Code.PushNil();

    protected override void VisitLoopExpression(LoopExpression node)
    {
        // Setting the start offset as the address of the enter temp frame instruction
        // makes things a lot simpler when emitting continue expressions.
        // However, break has to manually emit the instruction to exit the temp frame
        // as it has to push a value onto the stack after exiting.
        
        var startOffset = Code.AddressOffset;
        var endOffsetData = new AddressOffsetData(Code, 0xFFFFFFFF);
        
        Code.EnterTempFrame();

        var emitter = new LoopEmitter(function, functionBuilders, strings, startOffset, endOffsetData);
        emitter.Visit(node.Block);

        Code.ExitTempFrame();
        Code.Jump(startOffset);

        endOffsetData.Offset = Code.AddressOffset;
    }

    protected override void VisitBreakExpression(BreakExpression node) =>
        throw new InvalidOperationException("Cannot emit a break expression outside of a loop.");

    protected override void VisitContinueExpression(ContinueExpression node) =>
        throw new InvalidOperationException("Cannot emit a continue expression outside of a loop.");

    protected override void VisitIfExpression(IfExpression node)
    {
        Visit(node.Condition);

        var jumpToTrue = Code.JumpIf();

        Visit(node.IfFalse);
        
        var jumpToEnd = Code.Jump();
        
        jumpToTrue.SetAddress(Code.AddressOffset);
        Visit(node.IfTrue);
        
        jumpToEnd.SetAddress(Code.AddressOffset);
    }

    protected override void VisitCallExpression(CallExpression node)
    {
        Visit(node.Target);

        Visit(node.Arguments);
        
        Code.Call((uint)node.Arguments.Length);
    }

    protected override void VisitLambdaExpression(LambdaExpression node)
    {
        var lambdaFunctionId = functionBuilders[node.Function.Value].Id;
        
        Code.PushFunc(lambdaFunctionId);
    }

    protected override void VisitIdentifierExpression(IdentifierExpression node)
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
    }

    protected override void VisitReturnExpression(ReturnExpression node)
    {
        if (node.Expression is not null) Visit(node.Expression);
        else
        {
            Code.PushNil();
        }

        Code.Ret();
    }

    protected override void VisitLetDeclaration(LetDeclaration node)
    {
        Visit(node.Expression);

        var var = Locals.GetOrCreateVariable(node.Symbol.Value);
        
        Code.StoreVar(var);
    }
}
