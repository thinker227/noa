using Draco.Lsp.Attributes;
using Draco.Lsp.Model;

namespace Noa.LangServer.Addon;

[ClientCapability("TextDocument.CodeLens")]
internal interface ICodeLens
{
    [ServerCapability(nameof(ServerCapabilities.CodeLensProvider))]
    ICodeLensOptions Capability => CodeLensRegistrationOptions;

    [Request("textDocument/codeLens")]
    Task<IList<CodeLens>?> CodeLensAsync(
        CodeLensParams param,
        CancellationToken cancellationToken);
    
    [RegistrationOptions("textDocument/codeLens")]
    CodeLensRegistrationOptions CodeLensRegistrationOptions { get; }
}
