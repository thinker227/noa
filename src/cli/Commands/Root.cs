using System.CommandLine;
using System.CommandLine.Help;
using System.Diagnostics;
using Noa.Compiler;
using Spectre.Console;

namespace Noa.Cli.Commands;

public sealed class Root(
    IAnsiConsole console,
    CancellationToken ct,
    FileInfo inputFile,
    string[] args,
    FileInfo? outputFile,
    FileInfo? runtime,
    bool printReturnValue,
    bool doTime)
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

        command.SetAction((ctx, ct) =>
            Task.FromResult(
                new Root(
                    console,
                    ct,
                    ctx.GetValue(inputFileArgument)!,
                    ctx.GetValue(argsArgument) ?? [],
                    ctx.GetValue(outputFileOption),
                    ctx.GetValue(runtimeOption),
                    ctx.GetValue(printRetOption),
                    ctx.GetValue(timeOption))
                .Execute()));

        return command;
    }

    private int Execute()
    {
        var inputDisplayPath = Compile.GetDisplayPath(inputFile);
        
        Ast ast;
        try
        {
            (ast, _) = Compile.CoreCompile(inputFile, ct);
        }
        catch (OperationCanceledException)
        {
            console.MarkupLine($"{Emoji.Known.Multiply}  [red]Build cancelled[/]️");
            
            return 1;
        }

        if (ast.HasErrors)
        {
            Compile.PrintStatus(console, ast.Source, ast.Diagnostics);
            return 1;
        }
        
        if (outputFile is null)
        {
            var outputFileName = $"{Path.GetFileNameWithoutExtension(inputFile.Name)}.ark";
            var outputFilePath = Path.Combine(inputFile.Directory?.FullName ?? "", outputFileName);
            outputFile = new(outputFilePath);
        }

        using (var stream = outputFile.OpenWrite())
        {
            ast.Emit(stream);
        }

        var result = ExecuteArk(runtime, outputFile, printReturnValue);
        if (result is not var (time, exitCode)) return 1;

        if (doTime)
        {
            var duration = Compile.DisplayDuration(time);
            console.MarkupLine($"{Emoji.Known.Stopwatch}  Execution took [aqua]{duration}[/]");
        }

        return exitCode;
    }

    private (TimeSpan, int)? ExecuteArk(FileInfo? runtime, FileInfo arkFile, bool printReturnValue)
    {
        if (runtime is null)
        {
            const string runtimePathEnvVar = "NOA_RUNTIME";
            var runtimePath = Environment.GetEnvironmentVariable(runtimePathEnvVar);
            if (runtimePath is null)
            {
                console.MarkupLine($"{Emoji.Known.WhiteQuestionMark} [red]The environment variable [/][aqua]{runtimePathEnvVar}[/] [red]is not set.[/]");
                console.MarkupLine("[gray]Make sure the variable is set for the process and contains the path to the runtime executable, " +
                                   "or specify the [/][white]--runtime[/][gray] option with the path.[/]");
                return null;
            }
            else
            {
                runtime = new(runtimePath);
            }
        }

        if (!runtime.Exists)
        {
            console.MarkupLine($"{Emoji.Known.WhiteQuestionMark} [red]The specified runtime executable path [/][aqua]{runtime.FullName}[/][red] does not exist.[/]");
            return null;
        }

        var runtimeArgs = new List<string> { $"-f {arkFile.FullName}" };
        if (printReturnValue) runtimeArgs.Add("--print-ret");

        var process = new Process()
        {
            StartInfo =
            {
                FileName = runtime.FullName,
                Arguments = string.Join(" ", runtimeArgs),
                UseShellExecute = true,
                CreateNoWindow = true,
            }
        };

        GC.Collect();
        
        var timer = new Stopwatch();
        timer.Start();
        
        process.Start();
        process.WaitForExit();
        
        timer.Stop();
        var time = timer.Elapsed;

        return (time, process.ExitCode);
    }
}
