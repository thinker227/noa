namespace Noa.Compiler.Workspaces;

/// <summary>
/// Provides <see cref="Source"/> instances for documents containing source code.
/// </summary>
/// <typeparam name="TUri">The type of URI of the documents.</typeparam>
public interface ISourceProvider<TUri>
    where TUri : notnull
{
    /// <summary>
    /// Gets the source text for a document with a specified URI.
    /// </summary>
    /// <param name="uri">The URI of the document.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    string GetSourceText(TUri uri, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the source name for a document with a specified URI.
    /// </summary>
    /// <param name="uri">The URI of the document.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    string GetSourceName(TUri uri, CancellationToken cancellationToken);
}

public static class SourceProviderExtensions
{
    /// <summary>
    /// Gets a full <see cref="Source"/> for a document with a specified URI.
    /// </summary>
    /// <param name="uri">The URI of the document.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public static Source GetSource<TUri>(
        this ISourceProvider<TUri> sourceProvider,
        TUri uri,
        CancellationToken cancellationToken = default)
        where TUri : notnull
    {
        var text = sourceProvider.GetSourceText(uri, cancellationToken);
        var name = sourceProvider.GetSourceName(uri, cancellationToken);
        return new(text, name);
    }
}
