using System.CommandLine;
using System.CommandLine.Help;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Noa.Compiler;
using Spectre.Console;

namespace Noa.Cli.Commands;

public sealed class Root(
    IAnsiConsole console,
    CancellationToken ct,
    FileInfo inputFile,
    string[] args,
    FileInfo? outputFile,
    FileInfo? runtimeOverride,
    bool printReturnValue,
    bool doTime)
{
    public static Command CreateCommand(
        HelpBuilder helpBuilder,
        IAnsiConsole console)
    {
        var command = new Command ("noa")
        {
            Description = "Compiles and runs a Noa file."
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
            Description  = "The command-line arguments to pass to the compiled program when running it."
        };

        var outputFileOption = new ExtraHelpOption<FileInfo>("--output", "-o")
        {
            Description = "A path to the intermediate .ark file to output.",
            HelpValue = "path"
        };
        outputFileOption.AcceptLegalFilePathsOnly();

        var runtimeOption = new ExtraHelpOption<FileInfo>("--runtime", "-r")
        {
            Description = """
                The path to the runtime executable to invoke to run the program.
                If specified, overrides any existing runtime executable.
                """,
            HelpValue = "executable"
        };
        runtimeOption.AcceptLegalFilePathsOnly();
        runtimeOption.AcceptExistingOnly();

        var printRetOption = new Option<bool>("--print-ret", "-p")
        {
            Description = "Prints value returned from the main function of the program."
        };

        var timeOption = new Option<bool>("--time")
        {
            Description = "Prints the time the program takes to execute once it has exited."
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
        if (FindRuntime() is not {} runtime) return 1;

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

        if (!outputFile.Directory!.Exists) Directory.CreateDirectory(outputFile.Directory.FullName);

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

    private FileInfo? FindRuntime()
    {
        if (runtimeOverride is not null) return runtimeOverride;

        const string runtimePathEnvVar = "NOA_RUNTIME";
        var envRuntimePath = Environment.GetEnvironmentVariable(runtimePathEnvVar);
        var envVarIsSet = false;
        if (envRuntimePath is not null)
        {
            envVarIsSet = true;

            var envProvidedRuntime = new FileInfo(envRuntimePath);

            if (envProvidedRuntime.Exists) return envProvidedRuntime;
        }

        var siblingRuntimeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "noa_runtime.exe"
            : "noa_runtime";
        var processPath = Environment.ProcessPath!;
        var processDirectory = Path.GetDirectoryName(processPath)!;
        var siblingRuntimePath = Path.Combine(processDirectory, siblingRuntimeName);
        var siblingRuntime = new FileInfo(siblingRuntimePath);

        if (!siblingRuntime.Exists)
        {
            var envVarMessage = envVarIsSet
                ? ", [yellow]which was set but did not refer to a valid file.[/]"
                : ".";

            console.MarkupLine($"""
                [red]Cannot find the Noa runtime. Tried to find the runtime in the following places (in this order):
                  - A path provided by the [white]--runtime|-r[/] CLI option.
                  - A path provided by the [white]NOA_RUNTIME[/] environment variable{envVarMessage}
                  - [white]{siblingRuntimePath}[/].
                
                By default, the runtime should be located at [white]{siblingRuntimePath}[/].
                If running in a development environment, specify the path manually or set the environment variable.
                There's probably a runtime around here somewhere...
                [/]
                """);
            
            return null;
        }

        return siblingRuntime;
    }

    private (TimeSpan, int)? ExecuteArk(FileInfo runtime, FileInfo arkFile, bool printReturnValue)
    {
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
