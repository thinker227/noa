using Cocona;
using Noa.LangServer;
using Spectre.Console;

namespace Noa.Cli;

public sealed class LangServer(IAnsiConsole console, CancellationToken ct)
{
    [Command("lang-server", Description = "Starts the Noa language server")]
    public async Task<int> Start(
        [Option("stdio", Description = "Specifies that the language server should communicate across standard IO")]
        bool stdio)
    {
        if (!stdio)
        {
            console.MarkupLine("[red]Language server does not support non-stdio transport kinds. " +
                               "Specify [/][white]--stdio[/][red] to launch the server through stdio.[/]");
            return 1;
        }
        
        try
        {
            await NoaLanguageServer.RunAsync("this/is/very/obviously/a/path/lmao", ct);
        }
        catch (TaskCanceledException) {}

        return 0;
    }
}
