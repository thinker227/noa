using Draco.Lsp.Model;
using Noa.Compiler;
using Noa.Compiler.Workspaces;

namespace Noa.LangServer;

internal sealed class DiskSourceProvider : ISourceProvider<DocumentUri>
{
    public Source GetSource(DocumentUri uri, CancellationToken cancellationToken)
    {
        var path = uri.ToUri().AbsolutePath;
        var text = File.ReadAllText(path);
        return new(text, path);
    }
    
    public Source CreateSourceFrom(DocumentUri uri, string text) =>
        new(text, uri.ToUri().AbsolutePath);
}
