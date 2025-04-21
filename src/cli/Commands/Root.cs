using System.CommandLine;
using System.CommandLine.Help;
using Spectre.Console;

namespace Noa.Cli.Commands;

public sealed class Root
{
    public static Command CreateCommand(
        HelpBuilder helpBuilder,
        IAnsiConsole console)
    {
        var command = new Command ("noa")
        {
            Description = "Compiles and runs a source file."
        };
        command.Options.Clear();

        var inputFileArgument = new Argument<FileInfo>("input-file")
        {
            Description = "The file to run."
        };
        inputFileArgument.AcceptLegalFilePathsOnly();
        inputFileArgument.AcceptExistingOnly();

        var argsArgument = new Argument<string[]>("args")
        {
            Description  = "The command-line arguments passed to the compiled program when running."
        };

        var outputFileOption = new Option<FileInfo>("--output", "-o")
        {
            Description = "The output .ark file."
        };
        outputFileOption.AcceptLegalFilePathsOnly();

        var runtimeOption = new Option<FileInfo>("--runtime", "-r")
        {
            Description = "The path to the runtime executable."
        };
        runtimeOption.AcceptLegalFilePathsOnly();
        runtimeOption.AcceptExistingOnly();

        var printRetOption = new Option<bool>("--print-ret")
        {
            Description = "Prints the return value from the main function."
        };

        var timeOption = new Option<bool>("--time")
        {
            Description = "Prints the execution time of the program."
        };

        var helpOption = new HelpOption("--help", "-h")
        {
            Action = new HelpAction()
            {
                Builder = helpBuilder
            },
            Description = "Shows info about how to use the CLI."
        };

        var buildCommand = Build.CreateCommand(console);

        var langServerCommand = LangServer.CreateCommand(console);

        command.Add(inputFileArgument);
        command.Add(argsArgument);
        command.Add(outputFileOption);
        command.Add(runtimeOption);
        command.Add(printRetOption);
        command.Add(timeOption);
        command.Add(helpOption);
        command.Add(buildCommand);
        command.Add(langServerCommand);

        return command;
    }
}
