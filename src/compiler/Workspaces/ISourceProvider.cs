namespace Noa.Compiler.Workspaces;

/// <summary>
/// Provides <see cref="Source"/> instances for documents containing source code.
/// </summary>
/// <typeparam name="TUri">The type of URI of the documents.</typeparam>
public interface ISourceProvider<TUri>
    where TUri : notnull
{
    /// <summary>
    /// Gets the source for a document with a specified URI.
    /// </summary>
    /// <param name="uri">The URI of the document.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    Source GetSource(TUri uri, CancellationToken cancellationToken);

    /// <summary>
    /// Constructs a source from a URI and an existing text.
    /// The returned source is expected to have a <see cref="Source.Name"/>
    /// extrapolated from the <paramref name="uri"/>,
    /// and a <see cref="Source.Text"/> equal to <paramref name="text"/>.
    /// </summary>
    /// <param name="uri">The URI to create the source from.</param>
    /// <param name="text">The text of the source.</param>
    Source CreateSourceFrom(TUri uri, string text);
}
