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
