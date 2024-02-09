using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols;

public static class SymbolResolution
{
    public static IReadOnlyCollection<IDiagnostic> ResolveSymbols(Ast ast)
    {
        var globalScope = new MapScope(null, ast.Root);
        
        var visitor = new Visitor(globalScope);
        visitor.Visit(ast.Root);

        ast.GlobalScope = globalScope;
        var diagnostics = visitor.Diagnostics;
        
        return diagnostics;
    }
}

// Int here is just used as a useless type.
file sealed class Visitor(IScope globalScope) : Visitor<int>
{
    private IScope currentScope = globalScope;
    
    public List<IDiagnostic> Diagnostics { get; } = [];

    protected override void BeforeVisit(Node node) =>
        node.Scope = new(currentScope);

    private void InScope(IScope scope, Action action)
    {
        var parent = currentScope;
        currentScope = scope;
        
        action();

        currentScope = parent;
    }

    private IScope DeclareBlock(IBlockNode node)
    {
        // Begin by declaring all functions in the block
        // since they are accessible regardless of location within the block.
        
        var functions = new Dictionary<string, FunctionSymbol>();
        
        foreach (var statement in node.Statements)
        {
            if (statement.Declaration is not FunctionDeclaration func) continue;
            
            var functionSymbol = new FunctionSymbol()
            {
                Name = func.Identifier.Name,
                Declaration = func
            };

            func.Symbol = functionSymbol;

            foreach (var param in func.Parameters)
            {
                var parameterSymbol = new ParameterSymbol()
                {
                    Name = param.Identifier.Name,
                    Declaration = param,
                    Function = functionSymbol
                };

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
        // Note: the root is in the global scope, not the block scope it itself declares.
        
        var blockScope = DeclareBlock(node);
        InScope(blockScope, () =>
        {
            Visit(node.Statements);
        });

        return default;
    }
    
    // No need to visit let declarations because they've already been fully declared
    // and their bodies don't need additional scopes.

    protected override int VisitFunctionDeclaration(FunctionDeclaration node)
    {
        // All symbols here *should* already have been set by DeclareBlock.
        
        var functionSymbol = node.Symbol.Value;
        
        Visit(node.Identifier);
        
        var paramScope = new MapScope(currentScope, node);
        foreach (var param in node.Parameters)
        {
            var parameterSymbol = param.Symbol.Value;
            
            var result = paramScope.Declare(parameterSymbol);
            functionSymbol.AddParameter(parameterSymbol);

            if (result.ConflictingSymbol is not null)
            {
                var diagnostic = SymbolDiagnostics.SymbolAlreadyDeclared.Format(
                    parameterSymbol.Name,
                    param.Location);
                Diagnostics.Add(diagnostic);
            }
            
            Visit(param);
        }

        var bodyScope = new BlockingScope(paramScope, node);
        InScope(bodyScope, () =>
        {
            Visit(node.ExpressionBody);
            Visit(node.BlockBody);
        });

        return default;
    }

    protected override int VisitBlockExpression(BlockExpression node)
    {
        var blockScope = DeclareBlock(node);
        InScope(blockScope, () =>
        {
            Visit(node.Statements);
        });

        return default;
    }

    protected override int VisitLambdaExpression(LambdaExpression node)
    {
        var paramScope = new MapScope(currentScope, node);
        foreach (var param in node.Parameters)
        {
            var symbol = new ParameterSymbol()
            {
                Name = param.Identifier.Name,
                Declaration = param,
                Function = null
            };

            param.Symbol = symbol;
            
            var result = paramScope.Declare(symbol);
            
            if (result.ConflictingSymbol is not null)
            {
                var diagnostic = SymbolDiagnostics.SymbolAlreadyDeclared.Format(
                    symbol.Name,
                    param.Location);
                Diagnostics.Add(diagnostic);
            }
            
            Visit(param);
        }
        
        InScope(paramScope, () =>
        {
            Visit(node.Body);
        });

        return default;
    }

    protected override int VisitIdentifierExpression(IdentifierExpression node)
    {
        var identifier = node.Identifier;
        
        if (currentScope.LookupSymbol(identifier, node) is not var (symbol, accessibility))
        {
            Diagnostics.Add(SymbolDiagnostics.SymbolCannotBeFound.Format(identifier, node.Location));

            node.ReferencedSymbol = new ErrorSymbol();
            
            return default;
        }

        node.ReferencedSymbol = new(symbol);

        switch (accessibility)
        {
        case SymbolAccessibility.Blocked:
            Diagnostics.Add(SymbolDiagnostics.BlockedByFunction.Format(symbol, node.Location));
            break;
        
        case SymbolAccessibility.DeclaredLater:
            Diagnostics.Add(SymbolDiagnostics.DeclaredLater.Format(symbol, node.Location));
            break;
        }

        return default;
    }
}
