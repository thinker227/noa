using Noa.Compiler.Nodes;

namespace Noa.Compiler.Symbols;

internal sealed class LocalsVisitor : Visitor<int>
{
    private readonly List<VariableSymbol> locals = [];
    
    /// <summary>
    /// Gets a sequence of every local variable declared by a node and its descendants,
    /// bounded by the current function.
    /// </summary>
    /// <param name="node">The node to get the locals of.</param>
    public static IReadOnlyCollection<VariableSymbol> GetLocals(Node node)
    {
        var visitor = new LocalsVisitor();
        visitor.Visit(node);
        return visitor.locals;
    }
    
    private LocalsVisitor() {}

    protected override int VisitLetDeclaration(LetDeclaration node)
    {
        locals.Add(node.Symbol.Value);

        Visit(node.Identifier);
        Visit(node.Expression);
        
        return 0;
    }

    // Function declarations and lambdas 'terminate' the chain of collection of locals.
    
    protected override int VisitFunctionDeclaration(FunctionDeclaration node) => 0;

    protected override int VisitLambdaExpression(LambdaExpression node) => 0;
}
