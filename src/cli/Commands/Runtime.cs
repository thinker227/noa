using System.CommandLine;
using Spectre.Console;

namespace Noa.Cli.Commands;

public sealed class Runtime(IAnsiConsole console, bool silent)
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

        command.Add(silentOption);

        command.SetAction((ctx, ct) =>
            Task.FromResult(new Runtime(
                    console,
                    ctx.GetValue(silentOption))
                .Execute()));
        
        return command;
    }

    private int Execute()
    {
        var runtime = FindRuntime.Search(
            console: !silent ? console : null,
            runtimeOverride: null);

        return runtime is not null
            ? 0
            : 1;
    }
}
