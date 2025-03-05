using Noa.Compiler.Services.Context;

namespace Noa.Compiler.Services.Completion;

/// <summary>
/// A provider for a set of completion items based on a syntax context.
/// </summary>
public interface ICompletionProvider
{
    /// <summary>
    /// Gets the completion items for the provider based on the current syntax context.
    /// </summary>
    /// <param name="ctx">The current syntax context.</param>
    IEnumerable<CompletionItem> GetCompletionItems(SyntaxContext ctx);
}
