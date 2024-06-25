using Noa.Compiler.Bytecode.Builders;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.Compiler.Emit;

internal abstract class FunctionEmitter(
    IFunction function,
    IReadOnlyDictionary<IFunction, FunctionBuilder> functionBuilders,
    StringSectionBuilder strings) : Visitor<int>
{
    protected readonly IFunction function = function;
    protected readonly FunctionBuilder builder = functionBuilders[function];
    protected readonly IReadOnlyDictionary<IFunction, FunctionBuilder> functionBuilders = functionBuilders;
    protected readonly StringSectionBuilder strings = strings;

    protected CodeBuilder Code => builder.Code;

    protected LocalsInator Locals => builder.Locals;
    
    public static void EmitFunction(
        IFunction function,
        IReadOnlyDictionary<IFunction, FunctionBuilder> functionBuilders,
        StringSectionBuilder strings)
    {
        var emitter = new BlockEmitter(function, functionBuilders, strings);
        
        emitter.Visit(function.Body);
        emitter.Code.Ret();
    }
}
