using System.CommandLine;
using Spectre.Console;

namespace Noa.Cli.Commands;

public sealed class Runtime(
    IAnsiConsole console,
    bool silent,
    FileInfo? runtimeOverride)
{
    public static Command CreateCommand(IAnsiConsole console)
    {
        var command = new Command("runtime")
        {
            Description = "Searches for the currently configured default Noa runtime."
        };

        var silentOption = new Option<bool>("--silent")
        {
            Description = """
                Does not print any info to the console.
                Only the exit code will indicate success (0 for success and 1 for failure).
                """
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

        command.Add(silentOption);
        command.Add(runtimeOption);

        command.SetAction((ctx, ct) =>
            Task.FromResult(new Runtime(
                    console,
                    ctx.GetValue(silentOption),
                    ctx.GetValue(runtimeOption))
                .Execute()));
        
        return command;
    }

    private int Execute()
    {
        var runtime = FindRuntime.Search(
            console: !silent ? console : null,
            runtimeOverride);

        if (runtime is null) return 1;

        if (!silent)
        {
            console.MarkupLine($"[green]Runtime located at [white]{runtime.FullName}[/].[/]");
        }

        return 0;
    }
}
