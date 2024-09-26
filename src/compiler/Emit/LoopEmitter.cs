using Noa.Compiler.Bytecode.Builders;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.Compiler.Emit;

internal sealed class LoopEmitter(
    IDeclaredFunction function,
    IReadOnlyDictionary<IDeclaredFunction, FunctionBuilder> functionBuilders,
    StringSectionBuilder strings,
    uint startOffset,
    AddressOffsetData endOffsetData
    )
    : BlockEmitter(function, functionBuilders, strings)
{
    protected override void VisitBreakExpression(BreakExpression node)
    {
        // Emit the code for the returned expression and store it in a temporary variable
        // to persist it across the stack frame exit.

        if (node.Expression is not null) Visit(node.Expression);
        else Code.PushNil();
        
        using (var temp = Locals.GetTemp())
        {
            Code.StoreVar(temp.Variable);
            Code.ExitTempFrame();
            Code.LoadVar(temp.Variable);
        }
        
        Code.Jump(endOffsetData);
    }

    protected override void VisitContinueExpression(ContinueExpression node)
    {
        // A new temp frame is entered at the start of each loop,
        // so an enter temp frame instruction doesn't have to be emitted here.
        
        Code.ExitTempFrame();
        Code.Jump(startOffset);
    }
}
