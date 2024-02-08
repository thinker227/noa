using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols;

public static class SymbolResolution
{
    public static IReadOnlyCollection<IDiagnostic> ResolveSymbols(Ast ast)
    {
        var visitor = new Visitor();
        
        visitor.Visit(ast.Root);

        var diagnostics = visitor.Diagnostics;
        
        return diagnostics;
    }
}

// Int here is just used as a useless type.
file sealed class Visitor : Visitor<int>
{
    private IScope currentScope = null!;
    
    public List<IDiagnostic> Diagnostics { get; } = [];

    protected override void BeforeVisit(Node node) =>
        node.Scope = new(currentScope);

    private IScope DeclareBlock(IBlockNode node)
    {
        // Begin by declaring all functions in the block
        // since they are accessible regardless of location within the block.
        
        var functions = new Dictionary<string, FunctionSymbol>();
        
        foreach (var statement in node.Statements)
        {
            if (statement.Declaration is not FunctionDeclaration func) continue;
            
            var parameters = new List<ParameterSymbol>();
            var functionSymbol = new FunctionSymbol()
            {
                Name = func.Identifier.Name,
                Declaration = func,
                Parameters = parameters
            };

            foreach (var param in func.Parameters)
            {
                var parameterSymbol = new ParameterSymbol()
                {
                    Name = param.Identifier.Name,
                    Declaration = param,
                    Function = functionSymbol
                };
                
                parameters.Add(parameterSymbol);

                param.Symbol = parameterSymbol;
            }

            // Try add the function to the declared functions
            // and report an error if the function has already been declared.
            if (!functions.TryAdd(functionSymbol.Name, functionSymbol))
            {
                var diagnostic = SymbolDiagnostics.FunctionAlreadyDeclared.Format(
                    functionSymbol,
                    func.Location);
                Diagnostics.Add(diagnostic);
            }
        }
        
        // After all functions have been declared, declare all variables sequentially on a timeline
        // so that variables may shadow previously declared ones.
        
        var timelineIndex = 0;
        var timelineIndexMap = new Dictionary<Statement, int>();
        var variables = ImmutableDictionary.Create<string, VariableSymbol>();
        var variableTimeline = new List<ImmutableDictionary<string, VariableSymbol>>() { variables };
        
        foreach (var statement in node.Statements)
        {
            timelineIndexMap[statement] = timelineIndex;

            if (statement.Declaration is not LetDeclaration let) continue;
            
            var variableSymbol = new VariableSymbol()
            {
                Name = let.Identifier.Name,
                Declaration = let
            };

            let.Symbol = variableSymbol;

            timelineIndex++;
            variables = variables.SetItem(variableSymbol.Name, variableSymbol);
            variableTimeline.Add(variables);

            if (functions.TryGetValue(variableSymbol.Name, out var func))
            {
                var diagnostic = SymbolDiagnostics.VariableShadowsFunction.Format(
                    (variableSymbol, func),
                    let.Location);
                Diagnostics.Add(diagnostic);
            }
        }

        return new BlockScope(currentScope, (Node)node, functions, variableTimeline, timelineIndexMap);
    }
    
    protected override int VisitRoot(Root node)
    {
        DeclareBlock(node);

        Visit(node.Statements);

        return default;
    }
}
