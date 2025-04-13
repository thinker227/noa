namespace Noa.Compiler.Syntax;

/// <summary>
/// Syntax which can be navigated through.
/// </summary>
public interface ISyntaxNavigable
{
    /// <summary>
    /// Gets the first token within the syntax.
    /// </summary>
    /// <param name="includeInvisible">
    /// Whether to include invisible tokens.
    /// If <see langword="true"/>, the method will return <see langword="null"/>
    /// if the syntax does not have any visible tokens.
    /// </param>
    ITokenLike? GetFirstToken(bool includeInvisible = false);

    /// <summary>
    /// Gets the last token within the syntax.
    /// </summary>
    /// <param name="includeInvisible">
    /// Whether to include invisible tokens.
    /// If <see langword="true"/>, the method will return <see langword="null"/>
    /// if the syntax does not have any visible tokens.
    /// </param>
    ITokenLike? GetLastToken(bool includeInvisible = false);

    /// <summary>
    /// Gets the token preceding the syntax.
    /// </summary>
    /// <returns>The preceding token, or <see langword="null"/> if none could be found.</returns>
    /// <param name="includeInvisible">Whether to include invisible tokens.</param>
    ITokenLike? GetPreviousToken(bool includeInvisible = false);
}
