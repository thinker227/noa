using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;
using SuperLinq;

namespace Noa.Compiler.Symbols;

internal static class SymbolResolution
{
    /// <summary>
    /// Resolves scopes and symbols of an AST.
    /// </summary>
    /// <param name="ast">The AST to resolve the scopes and symbols of.</param>
    /// <param name="cancellationToken">The cancellation token which signals the symbol resolver to cancel.</param>
    /// <returns>The diagnostics produced by the resolution.</returns>
    public static IReadOnlyCollection<IDiagnostic> ResolveSymbols(
        Ast ast,
        CancellationToken cancellationToken = default)
    {
        var globalScope = new MapScope(null, ast.Root);
        
        var visitor = new Visitor(globalScope, cancellationToken);
        visitor.Visit(ast.Root);

        ast.GlobalScope = globalScope;
        var diagnostics = visitor.Diagnostics;
        
        return diagnostics;
    }
}

// Int here is just used as a useless type.
file sealed class Visitor(IScope globalScope, CancellationToken cancellationToken) : Visitor<int>
{
    private IScope currentScope = globalScope;
    private readonly Stack<IFunction> functionStack = [];

    public List<IDiagnostic> Diagnostics { get; } = [];

    protected override int GetDefault(Node node) => default;

    protected override void BeforeVisit(Node node)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        node.Scope = new(currentScope);
    }

    private void InScope(IScope scope, Action action)
    {
        var parent = currentScope;
        currentScope = scope;
        
        action();

        currentScope = parent;
    }

    private IScope DeclareBlock(BlockExpression block)
    {
        // Begin by declaring all functions in the block
        // since they are accessible regardless of location within the block.

        var containingFunction = functionStack.Peek();
        var functions = new Dictionary<string, NomialFunction>();
        
        foreach (var statement in block.Statements)
        {
            // These two foreach loops may take a substantial amount of time depending on the amount of
            // statements within the block. Placing a cancellation point at the start of each iteration
            // prevents a requested cancellation from taking too long here. 
            cancellationToken.ThrowIfCancellationRequested();
            
            if (statement is not FunctionDeclaration func) continue;
            
            var functionSymbol = new NomialFunction()
            {
                Name = func.Identifier.Name,
                Declaration = func,
                ContainingFunction = containingFunction
            };

            func.Symbol = functionSymbol;

            foreach (var (index, param) in func.Parameters.Index())
            {
                var parameterSymbol = new ParameterSymbol()
                {
                    Name = param.Identifier.Name,
                    Declaration = param,
                    Function = functionSymbol,
                    ParameterIndex = index
                };

                param.Symbol = parameterSymbol;
            }

            // Try add the function to the declared functions
            // and report an error if the function has already been declared.
            if (!functions.TryAdd(functionSymbol.Name, functionSymbol))
            {
                var diagnostic = SymbolDiagnostics.FunctionAlreadyDeclared.Format(
                    functionSymbol,
                    func.Identifier.Location);
                Diagnostics.Add(diagnostic);
            }
        }
        
        // After all functions have been declared, declare all variables sequentially on a timeline
        // so that variables may shadow previously declared ones.
        
        var timelineIndex = 0;
        var timelineIndexMap = new Dictionary<Node, int>();
        var variables = ImmutableDictionary.Create<string, VariableSymbol>();
        var variableTimeline = new List<ImmutableDictionary<string, VariableSymbol>>() { variables };
        
        foreach (var statement in block.Statements)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            timelineIndexMap[statement] = timelineIndex;

            if (statement is not LetDeclaration let) continue;
            
            var variableSymbol = new VariableSymbol()
            {
                Name = let.Identifier.Name,
                Declaration = let,
                ContainingFunction = containingFunction
            };

            let.Symbol = variableSymbol;

            timelineIndex++;
            variables = variables.SetItem(variableSymbol.Name, variableSymbol);
            variableTimeline.Add(variables);

            if (functions.TryGetValue(variableSymbol.Name, out var func))
            {
                var diagnostic = SymbolDiagnostics.VariableShadowsFunction.Format(
                    (variableSymbol, func),
                    let.Identifier.Location);
                Diagnostics.Add(diagnostic);
            }
        }

        // The trailing expression is always assigned the absolute last index.
        if (block.TrailingExpression is not null) timelineIndexMap[block.TrailingExpression] = timelineIndex;
        
        return new BlockScope(currentScope, block, functions, variableTimeline, timelineIndexMap);
    }
    
    protected override int VisitRoot(Root node)
    {
        
        var function = new TopLevelFunction()
        {
            Declaration = node
        };
        node.Function = function;
        
        functionStack.Push(function);
        
        // Note: the root is in the global scope, not the block scope it itself declares.
        
        var blockScope = DeclareBlock(node);
        node.DeclaredScope = new(blockScope);
        InScope(blockScope, () =>
        {
            Visit(node.Statements);
            Visit(node.TrailingExpression);
        });

        functionStack.Pop();

        return default;
    }
    
    // No need to visit let declarations because they've already been fully declared
    // and their bodies don't need additional scopes.

    protected override int VisitFunctionDeclaration(FunctionDeclaration node)
    {
        // All symbols here *should* already have been set by DeclareBlock.
        
        var functionSymbol = node.Symbol.Value;
        
        functionStack.Push(functionSymbol);
        
        Visit(node.Identifier);
        
        var blockingScope = new BlockingScope(currentScope, node);
        var bodyScope = new MapScope(blockingScope, node);
        foreach (var param in node.Parameters)
        {
            var parameterSymbol = param.Symbol.Value;
            
            var result = bodyScope.Declare(parameterSymbol);
            functionSymbol.parameters.Add(parameterSymbol);

            if (result.ConflictingSymbol is not null)
            {
                var diagnostic = SymbolDiagnostics.SymbolAlreadyDeclared.Format(
                    parameterSymbol.Name,
                    param.Identifier.Location);
                Diagnostics.Add(diagnostic);
            }
            
            Visit(param);
        }

        InScope(bodyScope, () =>
        {
            Visit(node.ExpressionBody);
            Visit(node.BlockBody);
        });

        functionStack.Pop();

        return default;
    }

    protected override int VisitBlockExpression(BlockExpression node)
    {
        var blockScope = DeclareBlock(node);
        node.DeclaredScope = new(blockScope);
        InScope(blockScope, () =>
        {
            Visit(node.Statements);
            Visit(node.TrailingExpression);
        });

        return default;
    }

    protected override int VisitLambdaExpression(LambdaExpression node)
    {
        var paramScope = new MapScope(currentScope, node);

        var containingFunction = functionStack.Peek();
        var function = new LambdaFunction()
        {
            Declaration = node,
            ContainingFunction = containingFunction
        };

        functionStack.Push(function);
        
        foreach (var (index, param) in node.Parameters.Index())
        {
            var symbol = new ParameterSymbol()
            {
                Name = param.Identifier.Name,
                Declaration = param,
                Function = function,
                ParameterIndex = index
            };

            param.Symbol = symbol;
            function.parameters.Add(symbol);
            
            var result = paramScope.Declare(symbol);
            
            if (result.ConflictingSymbol is not null)
            {
                var diagnostic = SymbolDiagnostics.SymbolAlreadyDeclared.Format(
                    symbol.Name,
                    param.Identifier.Location);
                Diagnostics.Add(diagnostic);
            }
            
            Visit(param);
        }
        
        node.Function = function;
        
        InScope(paramScope, () =>
        {
            Visit(node.Body);
        });

        functionStack.Pop();

        return default;
    }

    protected override int VisitIdentifierExpression(IdentifierExpression node)
    {
        var identifier = node.Identifier;
        
        if (currentScope.LookupSymbol(identifier, node) is not var (symbol, accessibility))
        {
            Diagnostics.Add(SymbolDiagnostics.SymbolCannotBeFound.Format(
                (identifier, currentScope, node),
                node.Location));

            node.ReferencedSymbol = new ErrorSymbol();
            
            return default;
        }

        // The symbol is still *referenced* even if it's not accessible.
        node.ReferencedSymbol = new(symbol);

        if (accessibility is SymbolAccessibility.Accessible &&
            symbol is IVariableSymbol variable &&
            !variable.ContainingFunction.Equals(functionStack.Peek()))
        {
            Diagnostics.Add(MiscellaneousDiagnostics.ClosuresUnsupported.Format(variable, node.Location));
        }

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
