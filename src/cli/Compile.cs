using Cocona;
using Noa.Compiler;
using Spectre.Console;

namespace Noa.Cli;

public sealed class Compile(IAnsiConsole console, CancellationToken ct) : CommandBase(console)
{
    [Command("build", Description = "Compiles a source file")]
    public int Execute(
        [Argument("input-file", Description = "The file to run")] string inputFile)
    {
        var file = new FileInfo(inputFile);
        var displayPath = GetDisplayPath(file);
        
        if (!file.Exists)
        {
            console.MarkupLine($"{Emoji.Known.WhiteQuestionMark} [aqua]{displayPath}[/] [red]does not exist.[/]");
            return 1;
        }

        console.MarkupLine($"{Emoji.Known.Wrench} Building [aqua]{displayPath}[/]...");

        Ast ast;
        TimeSpan time;
        try
        {
            (ast, time) = CoreCompile(file, ct);
        }
        catch (OperationCanceledException)
        {
            console.MarkupLine($"{Emoji.Known.Multiply}  [red]Build cancelled[/]️");
            
            return 1;
        }

        console.Write(DisplayBuildDuration(time));
        console.WriteLine();
        PrintStatus(ast.Source, ast.Diagnostics);
        
        return 0;
    }
}
