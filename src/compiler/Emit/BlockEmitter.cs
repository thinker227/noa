using Noa.Compiler.Bytecode.Builders;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.Compiler.Emit;

internal class BlockEmitter(
    IDeclaredFunction function,
    IReadOnlyDictionary<IDeclaredFunction, FunctionBuilder> functionBuilders,
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

    protected override void VisitBlock(Block node)
    {
        Visit(node.Statements);

        if (node.TrailingExpression is not null) Visit(node.TrailingExpression);
        else Code.PushNil();
    }

    protected override void VisitAssignmentStatement(AssignmentStatement node)
    {
        switch (node.Target)
        {
        case IdentifierExpression ident:
            EmitIdentifierAssignment(ident, node.Kind, node.Value);
            break;
        
        case AccessExpression access:
            EmitAccessAssignment(access, node.Kind, node.Value);
            break;
        
        case IndexExpression index:
            EmitIndexAssignment(index, node.Kind, node.Value);
            break;
        
        default:
            throw new UnreachableException();
        }
    }

    private void EmitIdentifierAssignment(IdentifierExpression target, AssignmentKind kind, Expression operand)
    {
        var varIndex = Locals.GetOrCreateVariable((IVariableSymbol)target.ReferencedSymbol.Value);

        if (kind is not AssignmentKind.Assign)
        {
            Code.LoadVar(varIndex);
            Visit(operand);
            EmitCompoundAssignmentKind(kind);
        }
        else
        {
            Visit(operand);
        }

        Code.StoreVar(varIndex);
    }

    private void EmitAccessAssignment(AccessExpression target, AssignmentKind kind, Expression operand)
    {
        Visit(target.Target); // [.., obj]
        Visit(target.Name); // [.., obj, name]

        if (kind is not AssignmentKind.Assign)
        {
            // Store the name into a temporary variable.
            using var nameTempVar = Locals.GetTemp();
            Code.StoreVar(nameTempVar.Variable);  // [.., obj]
            
            // Duplicate the object and load the name.
            Code.Dup();                         // [.., obj, obj]
            Code.LoadVar(nameTempVar.Variable); // [.., obj, obj, name]

            // Read field and emit the operation.
            Code.ReadField();                 // [.., obj, field]
            Visit(operand);                   // [.., obj, field, operand]
            EmitCompoundAssignmentKind(kind); // [.., obj, value]

            // Setup stack for write.
            Code.LoadVar(nameTempVar.Variable); // [.., obj, value, name]
            Code.Swap();                        // [.., obj, name, value]
        }
        else
        {
            Visit(operand); // [.., obj, name, value]
        }

        Code.WriteField();
    }

    private void EmitIndexAssignment(IndexExpression target, AssignmentKind kind, Expression operand)
    {
        Visit(target.Target); // [.., list]
        Visit(target.Index); // [.., list, index]

        if (kind is not AssignmentKind.Assign)
        {
            // Same logic as field compound assignment.

            using var indexTempVar = Locals.GetTemp();
            Code.StoreVar(indexTempVar.Variable); // [.., list]

            Code.Dup(); // [.., list, list]
            Code.LoadVar(indexTempVar.Variable); // [.., list, list, index]

            Code.ReadElement();               // [.., list, element]
            Visit(operand);                   // [.., list, element, operand]
            EmitCompoundAssignmentKind(kind); // [.., list, value]

            Code.LoadVar(indexTempVar.Variable); // [.., list, value, index]
            Code.Swap();                         // [.., list, index, value]
        }
        else
        {
            Visit(operand);
        }

        Code.WriteElement();
    }

    private void EmitCompoundAssignmentKind(AssignmentKind kind)
    {
        switch (kind)
        {
        case AssignmentKind.Assign:
            throw new InvalidOperationException();
        
        case AssignmentKind.Plus:
            Code.Add();
            break;
        
        case AssignmentKind.Minus:
            Code.Sub();
            break;
        
        case AssignmentKind.Mult:
            Code.Mult();
            break;
        
        case AssignmentKind.Div:
            Code.Div();
            break;
        
        default:
            throw new UnreachableException();
        }
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
            Code.PushFloat(0);
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
        // Special short-circuiting operators
        switch (node.Kind)
        {
        case BinaryKind.Or:
            EmitBinaryOr(node);
            return;
        
        case BinaryKind.And:
            EmitBinaryAnd(node);
            return;
        }

        // Normal operators
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

    private void EmitBinaryOr(BinaryExpression node)
    {
        Visit(node.Left);
        Code.Dup();

        var hole = Code.JumpIf();

        Code.Pop();
        Visit(node.Right);

        hole.SetAddress(Code.AddressOffset);
    }

    private void EmitBinaryAnd(BinaryExpression node)
    {
        Visit(node.Left);
        Code.Dup();
        Code.Not();

        var hole = Code.JumpIf();

        Code.Pop();
        Visit(node.Right);

        hole.SetAddress(Code.AddressOffset);
    }

    protected override void VisitNumberExpression(NumberExpression node) => Code.PushFloat(node.Value);

    protected override void VisitBoolExpression(BoolExpression node) => Code.PushBool(node.Value);

    protected override void VisitStringExpression(StringExpression node)
    {
        var first = true;

        if (node.Parts is [])
        {
            var empty = strings.GetOrAdd("");
            Code.PushString(empty);
            
            return;
        }

        foreach (var part in node.Parts)
        {
            Visit(part);
            
            if (!first) Code.Concat();
            first = false;
        }
    }

    protected override void VisitTextStringPart(TextStringPart node)
    {
        var index = strings.GetOrAdd(node.Text);
        Code.PushString(index);
    }

    protected override void VisitInterpolationStringPart(InterpolationStringPart node)
    {
        Visit(node.Expression);
        Code.ToString(); // This is not built-in `ToString` which returns a string.
    }

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

        if (node.Else is { IfFalse: var ifFalse })
        {
            var jumpToTrue = Code.JumpIf();

            Visit(ifFalse);
        
            var jumpToEnd = Code.Jump();
        
            jumpToTrue.SetAddress(Code.AddressOffset);
            Visit(node.IfTrue);
        
            jumpToEnd.SetAddress(Code.AddressOffset);
        }
        else
        {
            Code.Not();
            var jumpToEnd = Code.JumpIf();
            
            Visit(node.IfTrue);
            Code.Pop();
            
            jumpToEnd.SetAddress(Code.AddressOffset);

            Code.PushNil();
        }
    }

    protected override void VisitCallExpression(CallExpression node)
    {
        Visit(node.Target);

        Visit(node.Arguments);
        
        Code.Call((uint)node.Arguments.Length);
    }

    protected override void VisitLambdaExpression(LambdaExpression node)
    {
        var function = node.Function.Value;
        
        if (function.Captures.Count > 0)
        {
            throw new NotImplementedException();
        }

        var lambdaFunctionId = functionBuilders[function].Id;
        
        var captureIndices = function.Captures
            .Select(Locals.GetOrCreateVariable)
            .ToList();

        Code.PushFunc(lambdaFunctionId, captureIndices);
    }

    protected override void VisitObjectExpression(ObjectExpression node)
    {
        Code.PushObject(node.IsDynamic);

        foreach (var field in node.Fields)
        {
            // Object
            Code.Dup();
            
            // Name
            Visit(field.Name);

            // Value
            Visit(field.Value);

            Code.AddField(field.IsMutable);
        }
    }

    protected override void VisitListExpression(ListExpression node)
    {
        Code.PushList();

        foreach (var element in node.Elements)
        {
            Code.Dup();

            Visit(element);

            Code.AppendElement();
        }
    }

    protected override void VisitAccessExpression(AccessExpression node)
    {
        // Object
        Visit(node.Target);

        // Name
        Visit(node.Name);

        Code.ReadField();
    }

    protected override void VisitSimpleFieldName(SimpleFieldName node)
    {
        var index = strings.GetOrAdd(node.Name);
        Code.PushString(index);
    }

    protected override void VisitExpressionFieldName(ExpressionFieldName node)
    {
        Visit(node.Expression);
        Code.ToString();
    }

    protected override void VisitInferredFieldName(InferredFieldName node)
    {
        var index = strings.GetOrAdd(node.Name);
        Code.PushString(index);
    }

    protected override void VisitIndexExpression(IndexExpression node)
    {
        Visit(node.Target);

        Visit(node.Index);

        Code.ReadElement();
    }

    protected override void VisitIdentifierExpression(IdentifierExpression node)
    {
        switch (node.ReferencedSymbol.Value)
        {
        case NomialFunction func:
            var funcId = functionBuilders[func].Id;
            
            Code.PushFunc(funcId, []);
            
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
