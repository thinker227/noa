using System.CommandLine;
using Spectre.Console;

namespace Noa.Cli.Commands;

public sealed class Build
{
    public static Command CreateCommand(IAnsiConsole console)
    {
        var command = new Command("build")
        {
            Description = "Builds a source file."
        };

        var inputFileArgument = new Argument<FileInfo>("input-file")
        {
            Description = "The file to build."
        };
        inputFileArgument.AcceptLegalFilePathsOnly();
        inputFileArgument.AcceptExistingOnly();

        var outputFileOption = new Option<FileInfo>("--output", "-o")
        {
            Description = "The output .ark file."
        };
        outputFileOption.AcceptLegalFilePathsOnly();

        command.Add(inputFileArgument);
        command.Add(outputFileOption);

        return command;
    }
}
