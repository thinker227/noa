using Draco.Lsp.Attributes;
using Draco.Lsp.Model;
using Draco.Lsp.Server.Language;
using Range = Draco.Lsp.Model.Range;

namespace Noa.LangServer.Addon;

[ClientCapability("TextDocument.PrepareRename")]
internal interface IPrepareRename : IRename
{
    [Request("textDocument/prepareRename")]
    Task<OneOf<Range, PrepareRenameResult>?> PrepareRenameAsync(
        PrepareRenameParams param,
        CancellationToken cancellationToken);
}
