using TextMappingUtils;

namespace Noa.Compiler.Workspaces;

/// <summary>
/// A workspace which manages <see cref="NoaDocument{TUri}"/>s.
/// </summary>
/// <typeparam name="TUri">The type of the URI of the documents in the workspace.</typeparam>
/// <param name="sourceProvider">
/// The <see cref="ISourceProvider{TUri}"/> which provides source code for documents in the workspace.
/// </param>
public sealed class Workspace<TUri>(ISourceProvider<TUri> sourceProvider)
    where TUri : notnull
{
    private readonly Dictionary<TUri, NoaDocument<TUri>> documents = [];

    /// <summary>
    /// Gets an existing document, or creates a new one and saves it if one doesn't already exist.
    /// Calling this method with the same URI multiple times will return the same document.
    /// </summary>
    /// <param name="uri">The URI of the document to get or create.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The existing or newly created document.</returns>
    public NoaDocument<TUri> GetOrCreateDocument(
        TUri uri,
        CancellationToken cancellationToken = default) =>
        documents.TryGetValue(uri, out var document)
            ? document
            : UpdateOrCreateDocument(uri, null, cancellationToken);
    
    /// <summary>
    /// Updates an existing document, or creates a new one if one doesn't already exist.
    /// The updated or newly created document is subsequently saved.
    /// </summary>
    /// <param name="uri">The URI of the document to update or create.</param>
    /// <param name="text">
    /// The text to update or create the document from.
    /// If specified, calls <see cref="ISourceProvider{TUri}.CreateSourceFrom"/> on the source provider
    /// to create a new source from the specified text,
    /// otherwise calls <see cref="ISourceProvider{TUri}.GetSource"/> using the <paramref name="uri"/>.
    /// </param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The updated or newly created document.</returns>
    public NoaDocument<TUri> UpdateOrCreateDocument(
        TUri uri,
        string? text = null,
        CancellationToken cancellationToken = default)
    {
        var document = CreateDocument(uri, text, cancellationToken);
        documents[uri] = document;
        return document;
    }

    /// <summary>
    /// Creates a new document. The created document is <b>not</b> saved after being created.
    /// </summary>
    /// <param name="uri">The URI of the document to create.</param>
    /// <param name="text">
    /// The text to update or create the document from.
    /// If specified, calls <see cref="ISourceProvider{TUri}.CreateSourceFrom"/> on the source provider
    /// to create a new source from the specified text,
    /// otherwise calls <see cref="ISourceProvider{TUri}.GetSource"/> using the <paramref name="uri"/>.
    /// </param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The newly created document.</returns>
    public NoaDocument<TUri> CreateDocument(
        TUri uri,
        string? text = null,
        CancellationToken cancellationToken = default)
    {
        var source = text is not null
            ? new(text, sourceProvider.GetSourceName(uri, cancellationToken))
            : sourceProvider.GetSource(uri, cancellationToken);

        var ast = Ast.Create(source, cancellationToken);
        
        var lineMap = LineMap.Create(source.Text);

        return new(ast, source, lineMap, uri);
    }

    /// <summary>
    /// Deletes a document.
    /// This is in essence the same as <see cref="MarkAsUpdated"/> because it will also
    /// cause a document to be re-compiled the next time <see cref="GetOrCreateDocument"/>
    /// or <see cref="UpdateOrCreateDocument"/> is called, but <see cref="DeleteDocument"/>
    /// completely removes it from the workspace, allowing the GC to collect the memory.
    /// </summary>
    /// <param name="uri">The URI of the document to delete.</param>
    /// <returns>Whether the document existed in the workspace and therefore was deleted.</returns>
    public bool DeleteDocument(TUri uri) =>
        documents.Remove(uri);
}
