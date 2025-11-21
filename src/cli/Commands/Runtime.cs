using System.CommandLine;
using Spectre.Console;

namespace Noa.Cli.Commands;

public sealed class Runtime(
    IAnsiConsole console,
    bool plain,
    FileInfo? runtimeOverride)
{
    public static Command CreateCommand(IAnsiConsole console)
    {
        var command = new Command("runtime")
        {
            Description = "Searches for the currently configured default Noa runtime."
        };

        var plainOption = new Option<bool>("--plain")
        {
            Description = "Disables fancy formatting and just prints the found runtime path without a newline."
        };

        var runtimeOption = new ExtraHelpOption<FileInfo>("--runtime", "-r")
        {
            Description = """
                The path to the runtime executable.
                If specified, overrides any existing runtime executable.
                """,
            HelpValue = "executable"
        };
        runtimeOption.AcceptLegalFilePathsOnly();
        runtimeOption.AcceptExistingOnly();

        command.Add(plainOption);
        command.Add(runtimeOption);

        command.SetAction((ctx, ct) =>
            Task.FromResult(new Runtime(
                    console,
                    ctx.GetValue(plainOption),
                    ctx.GetValue(runtimeOption))
                .Execute()));
        
        return command;
    }

    private int Execute()
    {
        var config = Config.TryGetEnvironmentConfig();

        var runtime = FindRuntime.Search(
            console: !plain ? console : null,
            config,
            runtimeOverride);

        if (runtime is null) return 1;

        if (plain)
        {
            console.Write(runtime.FullName);
        }
        else
        {
            console.MarkupLine($"[green]Runtime located at [white]{runtime.FullName}[/].[/]");
        }

        return 0;
    }
}
