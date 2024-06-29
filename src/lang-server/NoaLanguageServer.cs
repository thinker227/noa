using Draco.Lsp.Model;
using Draco.Lsp.Server;
using Noa.LangServer.Logging;
using Serilog;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer(
    ILanguageClient client,
    ILogger logger,
    CancellationToken cancellationToken)
    : ILanguageServer
{
    public InitializeResult.ServerInfoResult? Info { get; } = new()
    {
        Name = "Noa Language Server",
        Version = "0.1.0"
    };

    public static IList<DocumentFilter> DocumentSelector { get; } =
    [
        new()
        {
            Language = "noa",
            Pattern = "**/*.noa"
        }
    ];
}
