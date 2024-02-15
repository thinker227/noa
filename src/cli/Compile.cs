using Cocona;
using Spectre.Console;

namespace Noa.Cli;

public sealed class Compile(IAnsiConsole console) : CommandBase(console)
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

        var (ast, time) = CoreCompile(file);

        console.Write(DisplayBuildDuration(time));
        console.WriteLine();
        PrintStatus(ast.Source, ast.Diagnostics);
        
        return 0;
    }
}
