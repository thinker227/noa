using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;

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
        
        var visitor = new SymbolVisitor(globalScope, cancellationToken);
        visitor.Visit(ast.Root);

        ast.GlobalScope = globalScope;
        var diagnostics = visitor.Diagnostics;
        
        return diagnostics;
    }
}

file sealed class SymbolVisitor(IScope globalScope, CancellationToken cancellationToken) : Visitor
{
    private IScope currentScope = globalScope;
    private readonly Stack<IDeclaredFunction> functionStack = [];

    public List<IDiagnostic> Diagnostics { get; } = [];

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

    private IScope DeclareBlock(Block block)
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

    protected override void VisitBlock(Block node)
    {
        var blockScope = DeclareBlock(node);
        node.DeclaredScope = new(blockScope);
        InScope(blockScope, () =>
        {
            Visit(node.Statements);
            if (node.TrailingExpression is not null) Visit(node.TrailingExpression);
        });
    }

    protected override void VisitRoot(Root node)
    {
        
        var function = new TopLevelFunction()
        {
            Declaration = node
        };
        node.Function = function;
        
        functionStack.Push(function);
        
        // Note: the root is in the global scope, not the block scope it itself declares.
        
        Visit(node.Block);

        functionStack.Pop();
    }
    
    // No need to visit let declarations because they've already been fully declared
    // and their bodies don't need additional scopes.

    protected override void VisitFunctionDeclaration(FunctionDeclaration node)
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
            if (node.ExpressionBody is not null) Visit(node.ExpressionBody);
            if (node.BlockBody is not null) Visit(node.BlockBody);
        });

        functionStack.Pop();
    }

    protected override void VisitLambdaExpression(LambdaExpression node)
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
    }

    protected override void VisitIdentifierExpression(IdentifierExpression node)
    {
        var identifier = node.Identifier;
        
        var location = LookupLocation.AtNode(node);
        if (currentScope.LookupSymbol(identifier, location) is not var (symbol, accessibility))
        {
            Diagnostics.Add(SymbolDiagnostics.SymbolCannotBeFound.Format(
                (identifier, currentScope, location),
                node.Location));

            node.ReferencedSymbol = new ErrorSymbol();
            
            return;
        }

        // The symbol is still *referenced* even if it's not accessible.
        node.ReferencedSymbol = new(symbol);

        switch (accessibility)
        {
        case SymbolAccessibility.Accessible:
            {
                var containingFunction = functionStack.Peek();

                if (symbol is IVariableSymbol variable &&
                    containingFunction is LambdaFunction &&
                    !variable.ContainingFunction.Equals(containingFunction))
                {
                    // We are inside a lambda function and the referenced variable is captured from an outer function.
                    // Traverse the current function stack and, for all lambdas at the top of the stack,
                    // add the lambda to the variable's list of referants and the variable to the lambda's list of captures.

                    var lambdas = functionStack
                        .TakeWhile(x => x is LambdaFunction)
                        .OfType<LambdaFunction>();
                        
                    foreach (var lambda in lambdas)
                    {
                        variable.Capture.CaptureInto(lambda);
                        lambda.AddCapture(variable);
                    }
                }

                break;
            }
        case SymbolAccessibility.Blocked:
            Diagnostics.Add(SymbolDiagnostics.BlockedByFunction.Format(symbol, node.Location));
            break;
        
        case SymbolAccessibility.DeclaredLater:
            Diagnostics.Add(SymbolDiagnostics.DeclaredLater.Format(symbol, node.Location));
            break;
        }
    }
}
