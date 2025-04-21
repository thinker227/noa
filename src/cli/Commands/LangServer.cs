using System.CommandLine;
using Noa.LangServer;
using Spectre.Console;

namespace Noa.Cli.Commands;

public sealed class LangServer
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

        var logOption = new Option<FileInfo>("--log")
        {
            Description = "A path to a file to which logs will be written."
        };
        logOption.AcceptLegalFilePathsOnly();

        var logLevelOption = new Option<LogLevel>("--log-level")
        {
            Description = "The level of messages to log from the language server."
        };
        logLevelOption.AcceptOnlyFromAmong("info", "debug");
        logLevelOption.DefaultValueFactory = _ => LogLevel.Info;

        command.Add(stdioOption);
        command.Add(logOption);
        command.Add(logLevelOption);

        return command;
    }
}
