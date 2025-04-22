using Draco.Lsp.Model;
using Noa.Compiler.Workspaces;
using Serilog;

namespace Noa.LangServer;

internal sealed class DiskSourceProvider(ILogger logger) : ISourceProvider<DocumentUri>
{
    public string GetSourceName(DocumentUri uri, CancellationToken cancellationToken) =>
        GetPath(uri);

    public string GetSourceText(DocumentUri uri, CancellationToken cancellationToken)
    {
        var path = GetPath(uri);

        logger.Debug("Reading file '{path}'.", path);

        return File.ReadAllText(path);
    }

    private static string GetPath(DocumentUri documentUri)
    {
        var uri = documentUri.ToUri();

        if (!uri.IsAbsoluteUri)
            throw new NotSupportedException("Document URI provided by language client is not an absolute URI.");
        
        if (!uri.IsFile)
            throw new NotSupportedException("Document URI provided by language client does not refer to a file " +
                                            "(does not use the file:// scheme).");
        
        // Remove "file://" prefix
        return uri.ToString()[7..];
    }
}
