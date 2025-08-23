using System.Runtime.InteropServices;
using Spectre.Console;

namespace Noa.Cli;

public static class FindRuntime
{
    public const string RuntimePathEnvVar = "NOA_RUNTIME";

    public static FileInfo? Search(IAnsiConsole? console, Config? config, FileInfo? runtimeOverride)
    {
        if (runtimeOverride is not null) return runtimeOverride;

        if (config?.RuntimePath is not null && File.Exists(config.RuntimePath)) return new(config.RuntimePath);

        var (envProvidedRuntime, envVarIsSet) = TryFindEnv();
        if (envProvidedRuntime is not null) return envProvidedRuntime;

        var siblingRuntime = FindSibling();
        if (siblingRuntime.Exists) return siblingRuntime;

        if (console is not null)
        {
            PrintErrorMessage(console, envVarIsSet, siblingRuntime.FullName);
        }
        
        return null;
    }

    private static (FileInfo? envProvidedRuntime, bool isSet) TryFindEnv()
    {
        var envRuntimePath = Environment.GetEnvironmentVariable(RuntimePathEnvVar);
        var isSet = false;

        if (envRuntimePath is not null)
        {
            isSet = true;
            var envProvidedRuntime = new FileInfo(envRuntimePath);

            if (envProvidedRuntime.Exists) return (envProvidedRuntime, isSet);
        }

        return (null, isSet);
    }

    private static FileInfo FindSibling()
    {
        var executableName = GetExecutableName();
        var processDirectory = Path.GetDirectoryName(Environment.ProcessPath!)!;
        var siblingRuntimePath = Path.Combine(processDirectory, executableName);
        return new FileInfo(siblingRuntimePath);
    }

    private static string GetExecutableName() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "noa_runtime.exe"
            : "noa_runtime";

    private static void PrintErrorMessage(IAnsiConsole console, bool envVarIsSet, string siblingRuntimePath)
    {
        var currentDirectoryPath = Environment.CurrentDirectory;
        var configPath = Path.Combine(Config.DirectoryName, Config.FileName);
        var exampleConfigPath = Path.Combine(currentDirectoryPath, configPath);
        var relativeConfigPath = Path.Combine(".", configPath);

        var envVarMessage = envVarIsSet
            ? ", [yellow]which was set but did not refer to a valid file.[/]"
            : ".";

        console.MarkupLine($"""
            [red]Cannot find the Noa runtime.
            
            Tried to find the runtime in the following places (in this order):
            - A path provided by the [white]--runtime|-r[/] CLI option.
            - The [white]runtimePath[/] property of the root object within the current configuration file.
            - A path provided by the [white]{RuntimePathEnvVar}[/] environment variable{envVarMessage}
            - [white]{siblingRuntimePath}[/].
            [/]
            [aqua]Tips:
            By default, the runtime should be located at [white]{siblingRuntimePath}[/].
            The path can be set via a configuration file, located at [white]{relativeConfigPath}[/] within the current or any parent directory, for example [white]{exampleConfigPath}[/]. Read more about configuration files within the project repo's readme - [purple]https://www.github.com/thinker227/noa#configuration[/].
            You can also specify the path manually using the [white]--runtime|-r[/] CLI option or by setting the [white]{RuntimePathEnvVar}[/] environment variable, for instance if running in a development environment.
            The runtime can also be invoked manually, although this is not recommended.

            If you do not have the Noa runtime installed, you can find instructions on how to download/compile it within the project repo's readme - [purple]https://www.github.com/thinker227/noa#installation[/].
            [/]
            There's probably a runtime somewhere around here...

            """);

            // If you wish to set up a project-specific runtime path, create a [white]{Config.DirectoryName}[/] directory in the current or any parent directory with a [white]{Config.FileName}[/] file inside it. Inside this configuration file, the [yellow]runtimePath[/] property of the root object specifies the path. An example configuration would be [yellow]{ "runtimePath": "path/to/noa_runtime" }[/] at [white]{exampleConfigPath}[/].
    }
}
