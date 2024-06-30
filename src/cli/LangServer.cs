using Cocona;
using Noa.LangServer;
using Spectre.Console;

namespace Noa.Cli;

public sealed class LangServer(IAnsiConsole console)
{
    [Command("lang-server", Description = "Starts the Noa language server")]
    public async Task<int> Start(
        [Option("stdio", Description = "Specifies that the language server should communicate across standard IO")]
        bool stdio,
        [Option("log", Description = "A path to a file to which logs will be written.")]
        string? logFilePath)
    {
        if (!stdio)
        {
            console.MarkupLine("[red]Language server does not support non-stdio transport kinds. " +
                               "Specify [/][white]--stdio[/][red] to launch the server through stdio.[/]");
            return 1;
        }
        
        try
        {
            await NoaLanguageServer.RunAsync(logFilePath);
        }
        catch (TaskCanceledException) {}
        catch (OperationCanceledException) {}

        console.WriteLine("Language server died a peaceful death.");

        return 0;
    }
}
