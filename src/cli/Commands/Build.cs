using System.CommandLine;
using Noa.Compiler;
using Spectre.Console;

namespace Noa.Cli.Commands;

public sealed class Build(
    IAnsiConsole console,
    CancellationToken ct,
    FileInfo inputFile,
    FileInfo? outputFile)
{
    public static Command CreateCommand(IAnsiConsole console)
    {
        var command = new Command("build")
        {
            Description = "Builds a Noa file into an Ark file, without running it."
        };

        var inputFileArgument = new Argument<FileInfo>("input-file")
        {
            Description = "The file to build."
        };
        inputFileArgument.AcceptLegalFilePathsOnly();
        inputFileArgument.AcceptExistingOnly();

        var outputFileOption = new Option<FileInfo>("--output", "-o")
        {
            Description = "A path to the intermediate .ark file to output."
        };
        outputFileOption.AcceptLegalFilePathsOnly();

        command.Add(inputFileArgument);
        command.Add(outputFileOption);

        command.SetAction((ctx, ct) =>
            Task.FromResult(
                new Build(
                    console,
                    ct,
                    ctx.GetValue(inputFileArgument)!,
                    ctx.GetValue(outputFileOption))
                .Execute()));

        return command;
    }

    private int Execute()
    {
        var inputDisplayPath = Compile.GetDisplayPath(inputFile);
        
        if (!inputFile.Exists)
        {
            console.MarkupLine($"{Emoji.Known.WhiteQuestionMark} [aqua]{inputDisplayPath}[/] [red]does not exist.[/]");
            return 1;
        }

        console.MarkupLine($"{Emoji.Known.Wrench} Building [aqua]{inputDisplayPath}[/]...");

        Ast ast;
        TimeSpan time;
        try
        {
            (ast, time) = Compile.CoreCompile(inputFile, ct);
        }
        catch (OperationCanceledException)
        {
            console.MarkupLine($"{Emoji.Known.Multiply}  [red]Build cancelled[/]️");
            
            return 1;
        }

        console.Write(Compile.DisplayBuildDuration(time));
        console.WriteLine();
        Compile.PrintStatus(console, ast.Source, ast.Diagnostics);

        if (ast.HasErrors) return 1;
        
        if (outputFile is null)
        {
            var outputFileName = $"{Path.GetFileNameWithoutExtension(inputFile.Name)}.ark";
            var outputFilePath = Path.Combine(inputFile.Directory?.FullName ?? "", outputFileName);
            outputFile = new(outputFilePath);
        }

        if (!outputFile.Directory!.Exists) Directory.CreateDirectory(outputFile.Directory.FullName);
        
        var outputDisplayPath = Compile.GetDisplayPath(outputFile);
        
        console.MarkupLine($"{Emoji.Known.Hammer} Assembling ark to [aqua]{outputDisplayPath}[/]...");

        using var stream = outputFile.OpenWrite();
        ast.Emit(stream);
        
        return 0;
    }
}
