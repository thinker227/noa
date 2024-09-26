using Noa.Compiler.Bytecode.Builders;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.Compiler.Emit;

internal abstract class FunctionEmitter(
    IDeclaredFunction function,
    IReadOnlyDictionary<IDeclaredFunction, FunctionBuilder> functionBuilders,
    StringSectionBuilder strings) : Visitor
{
    protected readonly IDeclaredFunction function = function;
    protected readonly FunctionBuilder builder = functionBuilders[function];
    protected readonly IReadOnlyDictionary<IDeclaredFunction, FunctionBuilder> functionBuilders = functionBuilders;
    protected readonly StringSectionBuilder strings = strings;

    protected CodeBuilder Code => builder.Code;

    protected LocalsInator Locals => builder.Locals;
    
    public static void EmitFunction(
        IDeclaredFunction function,
        IReadOnlyDictionary<IDeclaredFunction, FunctionBuilder> functionBuilders,
        StringSectionBuilder strings)
    {
        var emitter = new BlockEmitter(function, functionBuilders, strings);
        
        emitter.Visit(function.Body);
        emitter.Code.Ret();
    }
}
