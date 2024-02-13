using Cocona;
using Spectre.Console;

namespace Noa.Cli;

public sealed class Compile(IAnsiConsole console) : CommandBase
{
    [Command("build", Description = "Compiles a source file")]
    public int Execute(
        [Argument("input-file", Description = "The file to run")] string inputFile)
    {
        var file = new FileInfo(inputFile);
        var displayPath = GetDisplayPath(file);
        
        if (!file.Exists)
        {
            console.MarkupLine($"\u2754 [aqua]{displayPath}[/] [red]does not exist.[/]");
            return 1;
        }

        console.MarkupLine($"\ud83d\udd27 Building [aqua]{displayPath}[/]...");

        var (ast, time) = CoreCompile(file);

        console.MarkupLine($"\ud83d\udd52 Build took [aqua]{time}ms[/]");
        PrintStatus(console, ast.Diagnostics);
        
        return 0;
    }
}
