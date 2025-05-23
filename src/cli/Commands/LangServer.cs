using System.CommandLine;
using Noa.LangServer;
using Spectre.Console;

namespace Noa.Cli.Commands;

public sealed class LangServer(
    IAnsiConsole console,
    bool stdio,
    FileInfo? logFile,
    LogLevel logLevel)
{
    public static Command CreateCommand(IAnsiConsole console)
    {
        var command = new Command("lang-server")
        {
            Description = "Manages the Noa language server."
        };

        var stdioOption = new Option<bool>("--stdio")
        {
            Description = "Specifies that the language server should communicate across standard IO."
        };

        var logOption = new ExtraHelpOption<FileInfo>("--log")
        {
            Description = "A path to a file to which logs will be written.",
            HelpValue = "file"
        };
        logOption.AcceptLegalFilePathsOnly();

        var logLevelOption = new ExtraHelpOption<LogLevel>("--log-level")
        {
            Description = """
                The level of messages to log from the language server.
                Default: [b]info[/]
                """,
            HelpValue = "info|debug"
        };
        logLevelOption.AcceptOnlyFromAmong("info", "debug");
        logLevelOption.DefaultValueFactory = _ => LogLevel.Info;

        command.Add(stdioOption);
        command.Add(logOption);
        command.Add(logLevelOption);

        command.SetAction((ctx, _) =>
            new LangServer(
                console,
                ctx.GetValue(stdioOption),
                ctx.GetValue(logOption),
                ctx.GetValue(logLevelOption))
            .Execute());

        return command;
    }

    private async Task<int> Execute()
    {
        if (!stdio)
        {
            console.MarkupLine(
                "[red]Language server does not support non-stdio transport kinds. " +
                "Specify [white]--stdio[/] to launch the server through stdio.[/]");
            return 1;
        }
        
        try
        {
            await NoaLanguageServer.RunAsync(logFile?.FullName, logLevel);
        }
        catch (TaskCanceledException) {}
        catch (OperationCanceledException) {}

        console.WriteLine("Language server died a peaceful death.");

        return 0;
    }
}
