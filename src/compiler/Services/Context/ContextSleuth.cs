namespace Noa.Compiler.Services.Context;

public static class ContextSleuth
{
    /// <summary>
    /// Fetches the syntax context for a specified position within an AST.
    /// </summary>
    /// <param name="ast">The AST to get the context within.</param>
    /// <param name="position">The position within the AST to get the context at.</param>
    public static SyntaxContext GetSyntaxContext(this Ast ast, int position)
    {
        var context = SyntaxContext.None;
        
        return context;
    }
}
