using System.Diagnostics;
using Cocona;
using Noa.Compiler;
using Spectre.Console;

namespace Noa.Cli;

public sealed class Run(IAnsiConsole console, CancellationToken ct) : CommandBase(console)
{
    [Command("run", Description = "Compiles and runs a source file")]
    public int Execute(
        [Argument("input-file", Description = "The file to run")] string inputFilePath,
        [Option("output-file", ['o'], Description = "The output Ark file", ValueName = "path")] string? outputFilePath,
        [Option("runtime", Description = "The path to the runtime executable", ValueName = "path")] string? runtimePath,
        [Option("print-ret", Description = "Prints the return value from the main function")] bool printReturnValue,
        [Option("time", Description = "Prints the execution time of the program")] bool doTime)
    {
        var inputFile = new FileInfo(inputFilePath);
        var inputDisplayPath = GetDisplayPath(inputFile);
        
        if (!inputFile.Exists)
        {
            console.MarkupLine($"{Emoji.Known.WhiteQuestionMark} [aqua]{inputDisplayPath}[/] [red]does not exist.[/]");
            return 1;
        }
        
        Ast ast;
        try
        {
            (ast, _) = CoreCompile(inputFile, ct);
        }
        catch (OperationCanceledException)
        {
            console.MarkupLine($"{Emoji.Known.Multiply}  [red]Build cancelled[/]️");
            
            return 1;
        }

        if (ast.HasErrors)
        {
            PrintStatus(ast.Source, ast.Diagnostics);
            return 1;
        }
        
        var outputFileName = $"{Path.GetFileNameWithoutExtension(inputFile.Name)}.ark";
        outputFilePath ??= Path.Combine(inputFile.Directory?.FullName ?? "", outputFileName);
        
        var outputFile = new FileInfo(outputFilePath);

        using (var stream = outputFile.OpenWrite())
        {
            ast.Emit(stream);
        }

        if (runtimePath is null)
        {
            const string runtimePathEnvVar = "NOA_RUNTIME";
            runtimePath = Environment.GetEnvironmentVariable(runtimePathEnvVar);
            if (runtimePath is null)
            {
                console.MarkupLine($"{Emoji.Known.WhiteQuestionMark} [red]The environment variable [/][aqua]{runtimePathEnvVar}[/] [red]is not set.[/]");
                console.MarkupLine("[gray]Make sure the variable is set for the process and contains the path to the runtime executable, " +
                                   "or specify the [/][white]--runtime[/][gray] option with the path.[/]");
                return 1;
            }
        }

        if (!File.Exists(runtimePath))
        {
            console.MarkupLine($"{Emoji.Known.WhiteQuestionMark} [red]The specified runtime executable path [/][aqua]{runtimePath}[/][red] does not exist.[/]");
            return 1;
        }

        var runtimeArgs = new List<string> { $"-f {outputFilePath}" };
        if (printReturnValue) runtimeArgs.Add("--print-ret");

        var process = new Process()
        {
            StartInfo =
            {
                FileName = runtimePath,
                Arguments = string.Join(" ", runtimeArgs),
                UseShellExecute = true,
                CreateNoWindow = true,
            }
        };

        var timer = new Stopwatch();
        timer.Start();
        
        process.Start();
        process.WaitForExit();
        
        timer.Stop();
        var time = timer.Elapsed;

        if (doTime)
        {
            var duration = DisplayDuration(time);
            console.MarkupLine($"{Emoji.Known.Stopwatch}  Execution took [aqua]{duration}[/]");
        }

        return process.ExitCode;
    }
}
