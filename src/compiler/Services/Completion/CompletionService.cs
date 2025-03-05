using Noa.Compiler.Services.Context;
using Noa.Compiler.Symbols;

namespace Noa.Compiler.Services.Completion;

/// <summary>
/// A completion item.
/// </summary>
public abstract record CompletionItem
{
    /// <summary>
    /// A completion item for a symbol.
    /// </summary>
    /// <param name="Sym">The symbol.</param>
    public sealed record Symbol(ISymbol Sym) : CompletionItem;

    /// <summary>
    /// A completion item for a keyword.
    /// </summary>
    /// <param name="Word">The string representation of the keyword.</param>
    public sealed record Keyword(string Word) : CompletionItem;
}

/// <summary>
/// Service for fetching completion items.
/// </summary>
/// <param name="providers">The providers which provide completion items.</param>
public sealed class CompletionService(ImmutableArray<ICompletionProvider> providers)
{
    /// <summary>
    /// Fetches the completion items for a specified position within an AST
    /// </summary>
    /// <param name="ast">The AST to get the context within.</param>
    /// <param name="position">The position within the AST to get the context at.</param>
    public ImmutableArray<CompletionItem> GetCompletions(Ast ast, int position)
    {
        var ctx = ContextService.GetSyntaxContext(ast, position);

        var items = ImmutableArray.CreateBuilder<CompletionItem>();
        foreach (var provider in providers)
        {
            items.AddRange(provider.GetCompletionItems(ctx));
        }

        return items.ToImmutable();
    }
}
