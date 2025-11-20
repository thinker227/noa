using Noa.Compiler.Bytecode;
using Noa.Compiler.Bytecode.Builders;
using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.Compiler.Emit;

internal static class Emitter
{
    private const string MainName = "main";
    
    public static void Emit(Ast ast, Stream stream)
    {
        var strings = new StringSectionBuilder();
        var (functionsBuilder, main) = FunctionSectionBuilder.Create(
            strings.GetOrAdd(MainName));
        
        var functionsForEmission = ast.Root.DescendantsAndSelf()
            .Select(IDeclaredFunction? (x) => x switch
            {
                FunctionDeclaration f => f.Symbol.Value,
                LambdaExpression l => l.Function.Value,
                _ => null
            })
            .Where(x => x is not null);
        
        var functionBuilders = new Dictionary<IDeclaredFunction, FunctionBuilder>()
        {
            [ast.TopLevelFunction] = main
        };
        foreach (var function in functionsForEmission)
        {
            var name = function!.GetFullName();
            var arity = (uint)function!.Parameters.Count;
            var captures = (function as LambdaFunction)?.Captures
                ?? ImmutableHashSet<IVariableSymbol>.Empty;

            var builder = functionsBuilder.CreateFunction(strings.GetOrAdd(name), arity, captures);
            functionBuilders.Add(function, builder);
        }

        foreach (var function in functionBuilders.Keys)
        {
            FunctionEmitter.EmitFunction(function, functionBuilders, strings);
        }

        var ark = new Ark(functionsBuilder, strings);
        
        ark.Write(stream);
    }
}
