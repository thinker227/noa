using Cocona;
using Noa.Compiler;
using Spectre.Console;

namespace Noa.Cli;

public sealed class Compile(IAnsiConsole console, CancellationToken ct) : CommandBase(console)
{
    [Command("build", Description = "Compiles a source file")]
    public int Execute(
        [Argument("input-file", Description = "The file to run")] string inputFilePath,
        [Option("output-file", ['o'], Description = "The output Ark file")] string? outputFilePath)
    {
        var inputFile = new FileInfo(inputFilePath);
        var inputDisplayPath = GetDisplayPath(inputFile);
        
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
            (ast, time) = CoreCompile(inputFile, ct);
        }
        catch (OperationCanceledException)
        {
            console.MarkupLine($"{Emoji.Known.Multiply}  [red]Build cancelled[/]️");
            
            return 1;
        }

        console.Write(DisplayBuildDuration(time));
        console.WriteLine();
        PrintStatus(ast.Source, ast.Diagnostics);

        if (ast.HasErrors) return 1;

        var outputFileName = $"{Path.GetFileNameWithoutExtension(inputFile.Name)}.ark";
        outputFilePath ??= Path.Combine(inputFile.Directory?.FullName ?? "", outputFileName);
        
        var outputFile = new FileInfo(outputFilePath);
        var outputDisplayPath = GetDisplayPath(outputFile);
        
        console.MarkupLine($"{Emoji.Known.Hammer} Assembling ark to [aqua]{outputDisplayPath}[/]...");

        using var stream = outputFile.OpenWrite();
        
        ast.Emit(stream);
        
        return 0;
    }
}
