using System.Runtime.InteropServices;
using Spectre.Console;

namespace Noa.Cli;

public static class FindRuntime
{
    public static FileInfo? Search(IAnsiConsole? console, FileInfo? runtimeOverride)
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
            if (console is not null)
            {
                PrintErrorMessage(console, envVarIsSet, siblingRuntimePath);
            }
            
            return null;
        }

        return siblingRuntime;
    }

    private static void PrintErrorMessage(IAnsiConsole console, bool envVarIsSet, string siblingRuntimePath)
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
            There's probably a runtime somewhere around here...
            [/]
            """);
    }
}
